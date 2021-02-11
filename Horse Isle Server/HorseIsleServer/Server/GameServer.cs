using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Drawing;

using HISP.Player;
using HISP.Game;
using HISP.Security;
using HISP.Game.Chat;
using HISP.Player.Equips;
using HISP.Game.Services;
using HISP.Game.Inventory;
using HISP.Game.SwfModules;
using HISP.Game.Horse;
using HISP.Game.Items;

namespace HISP.Server
{
    public class GameServer
    {

        public static Socket ServerSocket;

        public static GameClient[] ConnectedClients // Done to prevent Enumerator Changed errors.
        {
            get {
                return connectedClients.ToArray();
            }
        }

        public static int IdleTimeout;
        public static int IdleWarning;

        public static Random RandomNumberGenerator = new Random();

        /*
         *  Private stuff 
         */
        private static int gameTickSpeed = 4320; // Changing this to ANYTHING else will cause desync with the client.
        private static int totalMinutesElapsed = 0;
        private static int oneMinute = 1000 * 60;
        private static List<GameClient> connectedClients = new List<GameClient>();
        private static Timer gameTimer; // Controls in-game time.
        private static Timer minuteTimer; // ticks every real world minute.
        private static void onGameTick(object state)
        {
            World.TickWorldClock();
            Database.DecHorseTrainTimeout();    
            gameTimer.Change(gameTickSpeed, gameTickSpeed);
        }
        private static void onMinuteTick(object state)
        {
            totalMinutesElapsed++;

            if (totalMinutesElapsed % 8 == 0)
            {
                Database.IncAllUsersFreeTime(1);
            }

            if (totalMinutesElapsed % 25 == 0)
            {

                Logger.DebugPrint("Randomizing Weather...");
                foreach (World.Town town in World.Towns)
                {
                    if (RandomNumberGenerator.Next(0, 100) < 25)
                    {
                        town.Weather = town.SelectRandomWeather();
                    }
                }

                foreach (World.Isle isle in World.Isles)
                {
                    if (RandomNumberGenerator.Next(0, 100) < 25)
                    {
                        isle.Weather = isle.SelectRandomWeather();
                    }
                }
            }

            foreach(GameClient client in ConnectedClients)
                if (client.LoggedIn)
                    if (!client.LoggedinUser.MetaPriority)
                        UpdateArea(client);

            Treasure.AddValue();
            Database.IncPlayerTirednessForOfflineUsers();
            DroppedItems.Update();
            WildHorse.Update();
            minuteTimer.Change(oneMinute, oneMinute);
        }

        /*
         * This section is where all the event handlers live, 
         * eg: OnMovementPacket is whenever the server receies a movement request from the client.
         */
        public static void OnCrossdomainPolicyRequest(GameClient sender)
        {
            Logger.DebugPrint("Cross-Domain-Policy request received from: " + sender.RemoteIp);

            byte[] crossDomainPolicyResponse = CrossDomainPolicy.GetPolicy();

            sender.SendPacket(crossDomainPolicyResponse);
        }


        public static void OnHorseInteraction(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent horse interaction when not logged in.");
                return;
            }

            if(packet.Length < 3)
            {
                Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid sized horse interaction packet: " + BitConverter.ToString(packet).Replace("-", " "));
                return;
            }

            byte method = packet[1];
            switch(method)
            {
                case PacketBuilder.HORSE_LIST:
                    sender.LoggedinUser.MetaPriority = true;
                    byte[] metaTags = PacketBuilder.CreateMetaPacket(Meta.BuildHorseInventory(sender.LoggedinUser));
                    sender.SendPacket(metaTags);
                    break;
                case PacketBuilder.HORSE_PROFILE:
                    byte methodProfileEdit = packet[2]; 
                    if(methodProfileEdit == PacketBuilder.HORSE_PROFILE_EDIT)
                    {
                        if (sender.LoggedinUser.LastViewedHorse != null)
                        {
                            sender.LoggedinUser.MetaPriority = true;
                            byte[] metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildHorseDescriptionEditMeta(sender.LoggedinUser.LastViewedHorse));
                            sender.SendPacket(metaPacket);
                        }
                        else
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + "Trying to edit description of no horse");
                        }
                    }
                    else
                    {
                        Logger.InfoPrint(BitConverter.ToString(packet).Replace("-", " "));
                    }
                    break;
                case PacketBuilder.HORSE_FEED:
                    int randomId = 0;
                    string packetStr = Encoding.UTF8.GetString(packet);
                    string randomIdStr = packetStr.Substring(2, packetStr.Length - 4);
                    try
                    {
                        randomId = int.Parse(randomIdStr);

                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if (sender.LoggedinUser.HorseInventory.HorseIdExist(randomId))
                    {
                        HorseInstance horseFeedInst = sender.LoggedinUser.HorseInventory.GetHorseById(randomId);

                        sender.LoggedinUser.LastViewedHorse = horseFeedInst;
                        sender.LoggedinUser.MetaPriority = true;
                        byte[] metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildHorseFeedMenu(horseFeedInst, sender.LoggedinUser));
                        sender.SendPacket(metaPacket);
                        break;
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to feed at a non existant horse.");
                        break;
                    }
                case PacketBuilder.HORSE_PET:
                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packetStr.Length - 4);
                    try
                    {
                        randomId = int.Parse(randomIdStr);

                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if (sender.LoggedinUser.HorseInventory.HorseIdExist(randomId))
                    {
                        HorseInstance horsePetInst = sender.LoggedinUser.HorseInventory.GetHorseById(randomId);
                        sender.LoggedinUser.LastViewedHorse = horsePetInst;
                        int randMoodAddition = RandomNumberGenerator.Next(1, 20);
                        int randTiredMinus = RandomNumberGenerator.Next(1, 10);



                        string msgs = "";
                        if (horsePetInst.BasicStats.Mood + randMoodAddition >= 1000)
                            msgs += Messages.HorsePetTooHappy;
                        if (horsePetInst.BasicStats.Tiredness - randTiredMinus <= 0)
                            msgs += Messages.HorsePetTooTired;




                        horsePetInst.BasicStats.Tiredness -= randTiredMinus;
                        horsePetInst.BasicStats.Mood += randMoodAddition;

                        byte[] petMessagePacket = PacketBuilder.CreateChat(Messages.FormatHorsePetMessage(msgs,randMoodAddition, randTiredMinus), PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(petMessagePacket);

                        break;
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to feed at a non existant horse.");
                        break;
                    }
                case PacketBuilder.HORSE_VET_SERVICE_ALL:

                    if (World.InSpecialTile(sender.LoggedinUser.X, sender.LoggedinUser.Y))
                    {
                        World.SpecialTile tile = World.GetSpecialTile(sender.LoggedinUser.X, sender.LoggedinUser.Y);
                        if (tile.Code.StartsWith("VET-"))
                        {
                            string[] vetInfo = tile.Code.Split('-');
                            int vetId = int.Parse(vetInfo[1]);
                            Vet vet = Vet.GetVetById(vetId);
                            int price = 0;

                            foreach (HorseInstance horse in sender.LoggedinUser.HorseInventory.HorseList)
                                price += vet.CalculatePrice(horse.BasicStats.Health);
                            if(price == 0)
                            {
                                byte[] notNeededMessagePacket = PacketBuilder.CreateChat(Messages.VetServicesNotNeededAll, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(notNeededMessagePacket);
                                break;
                            }
                            else if (sender.LoggedinUser.Money >= price)
                            {
                                foreach (HorseInstance horse in sender.LoggedinUser.HorseInventory.HorseList)
                                    horse.BasicStats.Health = 1000;

                                byte[] healedMessagePacket = PacketBuilder.CreateChat(Messages.VetAllFullHealthRecoveredMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(healedMessagePacket);

                                sender.LoggedinUser.Money -= price;

                            }
                            else
                            {
                                byte[] cannotAffordMessagePacket = PacketBuilder.CreateChat(Messages.VetCannotAffordMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(cannotAffordMessagePacket);
                                break;
                            }
                            UpdateArea(sender);
                        }
                    }
                    break;
                case PacketBuilder.HORSE_VET_SERVICE:
                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packetStr.Length - 4);

                    if (randomIdStr == "NaN")
                        break;
                        
                    try
                    {
                        randomId = int.Parse(randomIdStr);
                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if (sender.LoggedinUser.HorseInventory.HorseIdExist(randomId))
                    {
                        HorseInstance horseVetServiceInst = sender.LoggedinUser.HorseInventory.GetHorseById(randomId);
                        sender.LoggedinUser.LastViewedHorse = horseVetServiceInst;

                        if(World.InSpecialTile(sender.LoggedinUser.X, sender.LoggedinUser.Y))
                        {
                            World.SpecialTile tile = World.GetSpecialTile(sender.LoggedinUser.X, sender.LoggedinUser.Y);
                            if(tile.Code.StartsWith("VET-"))
                            {
                                string[] vetInfo = tile.Code.Split('-');
                                int vetId = int.Parse(vetInfo[1]);

                                Vet vet = Vet.GetVetById(vetId);
                                int price = vet.CalculatePrice(horseVetServiceInst.BasicStats.Health);
                                if(sender.LoggedinUser.Money >= price)
                                {
                                    horseVetServiceInst.BasicStats.Health = 1000;
                                    sender.LoggedinUser.Money -= price;

                                    byte[] messagePacket = PacketBuilder.CreateChat(Messages.FormatVetHorseAtFullHealthMessage(horseVetServiceInst.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(messagePacket);
                                }
                                else
                                {
                                    byte[] cantAffordMessage = PacketBuilder.CreateChat(Messages.VetCannotAffordMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(cantAffordMessage);
                                }
                                UpdateArea(sender);
                            }
                        }
                        break;
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to use vet services on a non existant horse.");
                        break;
                    }
                case PacketBuilder.HORSE_GIVE_FEED:
                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packetStr.Length - 4);
                    try
                    {
                        randomId = int.Parse(randomIdStr);

                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if(sender.LoggedinUser.LastViewedHorse == null)
                    {
                        Logger.InfoPrint(sender.LoggedinUser.Username + " Tried to feed a non existant horse.");
                        break;
                    }
                    if (sender.LoggedinUser.Inventory.HasItem(randomId))
                    {
                        InventoryItem item = sender.LoggedinUser.Inventory.GetItemByRandomid(randomId);
                        Item.ItemInformation itemInfo = item.ItemInstances[0].GetItemInfo();
                        HorseInstance horseInstance = sender.LoggedinUser.LastViewedHorse;
                        bool tooMuch = false;
                        if (itemInfo.Type == "HORSEFOOD")
                        {
                            foreach(Item.Effects effect in itemInfo.Effects)
                            {
                                switch(effect.EffectsWhat)
                                {
                                    case "HEALTH":
                                        if (horseInstance.BasicStats.Health + effect.EffectAmount > 1000)
                                            tooMuch = true;
                                        horseInstance.BasicStats.Health += effect.EffectAmount;
                                        break;
                                    case "HUNGER":
                                        if (horseInstance.BasicStats.Hunger + effect.EffectAmount > 1000)
                                            tooMuch = true;
                                        horseInstance.BasicStats.Hunger += effect.EffectAmount;
                                        break;
                                    case "MOOD":
                                        if (horseInstance.BasicStats.Mood + effect.EffectAmount > 1000)
                                            tooMuch = true;
                                        horseInstance.BasicStats.Mood += effect.EffectAmount;
                                        break;
                                    case "GROOM":
                                        if (horseInstance.BasicStats.Groom + effect.EffectAmount > 1000)
                                            tooMuch = true;
                                        horseInstance.BasicStats.Groom += effect.EffectAmount;
                                        break;
                                    case "SHOES":
                                        if (horseInstance.BasicStats.Shoes + effect.EffectAmount > 1000)
                                            tooMuch = true;
                                        horseInstance.BasicStats.Shoes += effect.EffectAmount;
                                        break;
                                    case "THIRST":
                                        if (horseInstance.BasicStats.Thirst + effect.EffectAmount > 1000)
                                            tooMuch = true;
                                        horseInstance.BasicStats.Thirst += effect.EffectAmount;
                                        break;
                                    case "TIREDNESS":
                                        if (horseInstance.BasicStats.Tiredness + effect.EffectAmount > 1000)
                                            tooMuch = true;
                                        horseInstance.BasicStats.Tiredness += effect.EffectAmount;
                                        break;

                                    case "INTELLIGENCEOFFSET":
                                        horseInstance.AdvancedStats.Inteligence += effect.EffectAmount;
                                        horseInstance.MagicUsed++;
                                        break;
                                    case "PERSONALITYOFFSET":
                                        horseInstance.AdvancedStats.Personality += effect.EffectAmount;
                                        horseInstance.MagicUsed++;
                                        break;
                                    case "SPOILED":
                                        horseInstance.Spoiled += effect.EffectAmount;
                                        break;
                                }
                            }
                            sender.LoggedinUser.Inventory.Remove(item.ItemInstances[0]);

                            byte[] horseNeighThanksPacket = PacketBuilder.CreateChat(Messages.HorseNeighsThanks, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(horseNeighThanksPacket);

                            if (tooMuch)
                            {
                                byte[] horseCouldntFinishItAll = PacketBuilder.CreateChat(Messages.HorseCouldNotFinish, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(horseCouldntFinishItAll);
                            }

                            sender.LoggedinUser.MetaPriority = true;
                            byte[] metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildHorseFeedMenu(sender.LoggedinUser.LastViewedHorse, sender.LoggedinUser));
                            sender.SendPacket(metaPacket);
                            break;
                        }
                        else
                        {
                            Logger.HackerPrint(sender.LoggedinUser.Username + "Tried to feed a horse a non-HORSEFOOD item.");
                        }
                        break;
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to feed a non existant item to a horse.");
                        break;
                    }
                case PacketBuilder.HORSE_RELEASE:
                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packetStr.Length - 4);
                    try
                    {
                        randomId = int.Parse(randomIdStr);

                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if (sender.LoggedinUser.HorseInventory.HorseIdExist(randomId))
                    {
                        if(World.InTown(sender.LoggedinUser.X, sender.LoggedinUser.Y))
                        {
                            Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to reelease a horse while inside a town....");
                            break;
                        }


                        HorseInstance horseReleaseInst = sender.LoggedinUser.HorseInventory.GetHorseById(randomId);
                        if(sender.LoggedinUser.CurrentlyRidingHorse != null)
                        {
                            if(horseReleaseInst.RandomId == sender.LoggedinUser.CurrentlyRidingHorse.RandomId) 
                            {
                                byte[] errorChatPacket = PacketBuilder.CreateChat(Messages.HorseCantReleaseTheHorseYourRidingOn, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(errorChatPacket);
                                break;
                            }

                        }

                        if (horseReleaseInst.Description == "")
                            horseReleaseInst.Description += Messages.FormatHorseReleasedBy(sender.LoggedinUser.Username);

                        Logger.InfoPrint(sender.LoggedinUser.Username + " RELEASED HORSE: " + horseReleaseInst.Name + " (a " + horseReleaseInst.Breed.Name + ").");

                        sender.LoggedinUser.HorseInventory.DeleteHorse(horseReleaseInst);
                        new WildHorse(horseReleaseInst, sender.LoggedinUser.X, sender.LoggedinUser.Y, 60, true);
                        
                        sender.LoggedinUser.LastViewedHorse = horseReleaseInst;
                        sender.LoggedinUser.MetaPriority = true;
                        byte[] metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildHorseReleased());
                        sender.SendPacket(metaPacket);
                        break;
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to release at a non existant horse.");
                        break;
                    }
                case PacketBuilder.HORSE_TACK:
                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packetStr.Length - 4);
                    try
                    {
                        randomId = int.Parse(randomIdStr);

                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if (sender.LoggedinUser.HorseInventory.HorseIdExist(randomId))
                    {
                        HorseInstance horseTackInst = sender.LoggedinUser.HorseInventory.GetHorseById(randomId);
                        
                        sender.LoggedinUser.LastViewedHorse = horseTackInst;
                        sender.LoggedinUser.MetaPriority = true;
                        byte[] metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildTackMenu(horseTackInst, sender.LoggedinUser));
                        sender.SendPacket(metaPacket);
                        break;
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to tack at a non existant horse.");
                        break;
                    }
                case PacketBuilder.HORSE_DRINK:
                    if(World.InSpecialTile(sender.LoggedinUser.X, sender.LoggedinUser.Y))
                    {
                        World.SpecialTile tile = World.GetSpecialTile(sender.LoggedinUser.X, sender.LoggedinUser.Y);
                        if(tile.Code != "POND")
                        {
                            Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to drink from a pond when not on one.");
                            break;
                        }
                    }

                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packetStr.Length - 4);
                    try
                    {
                        randomId = int.Parse(randomIdStr);

                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if (sender.LoggedinUser.HorseInventory.HorseIdExist(randomId))
                    {
                        HorseInstance horseDrinkInst = sender.LoggedinUser.HorseInventory.GetHorseById(randomId);

                        if(horseDrinkInst.BasicStats.Health < 200)
                        {
                            byte[] hpToLow = PacketBuilder.CreateChat(Messages.FormatPondHpLowMessage(horseDrinkInst.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(hpToLow);
                            break;
                        }

                        if(horseDrinkInst.BasicStats.Thirst < 1000)
                        {
                            horseDrinkInst.BasicStats.Thirst = 1000;
                            byte[] drinkFull = PacketBuilder.CreateChat(Messages.FormatPondDrinkFull(horseDrinkInst.Name),PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(drinkFull);

                            if(RandomNumberGenerator.Next(0, 100) < 25)
                            {
                                horseDrinkInst.BasicStats.Health -= 200;
                                byte[] ohNoes = PacketBuilder.CreateChat(Messages.FormatPondDrinkOhNoes(horseDrinkInst.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(ohNoes);
                            }

                            UpdateArea(sender);
                        }
                        else
                        {
                            byte[] notThirsty = PacketBuilder.CreateChat(Messages.FormatPondNotThirsty(horseDrinkInst.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(notThirsty);
                            break;
                        }
                        break;
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to tack at a non existant horse.");
                        break;
                    }
                case PacketBuilder.HORSE_TACK_EQUIP:

                    int itemId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    string itemIdStr = packetStr.Substring(2, packetStr.Length - 4);
                    try
                    {
                        itemId = int.Parse(itemIdStr);
                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if(Item.ItemIdExist(itemId))
                    {
                        if(sender.LoggedinUser.LastViewedHorse != null)
                        {
                            if(sender.LoggedinUser.LastViewedHorse.AutoSell > 0)
                            {
                                byte[] failMessagePacket = PacketBuilder.CreateChat(Messages.HorseTackFailAutoSell, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(failMessagePacket);
                                break;
                            }

                            if(sender.LoggedinUser.Inventory.HasItemId(itemId))
                            {
                                Item.ItemInformation itemInfo = Item.GetItemById(itemId);
                                if (itemInfo.Type == "TACK")
                                {
                                    switch (itemInfo.GetMiscFlag(0))
                                    {
                                        case 1: // Saddle
                                            if(sender.LoggedinUser.LastViewedHorse.Equipment.Saddle != null)
                                                sender.LoggedinUser.Inventory.AddIgnoringFull(new ItemInstance(sender.LoggedinUser.LastViewedHorse.Equipment.Saddle.Id));
                                            Database.SetSaddle(sender.LoggedinUser.LastViewedHorse.RandomId, itemInfo.Id);
                                            sender.LoggedinUser.LastViewedHorse.Equipment.Saddle = itemInfo;
                                            break;
                                        case 2: // Saddle Pad
                                            if (sender.LoggedinUser.LastViewedHorse.Equipment.SaddlePad != null)
                                                sender.LoggedinUser.Inventory.AddIgnoringFull(new ItemInstance(sender.LoggedinUser.LastViewedHorse.Equipment.SaddlePad.Id));
                                            Database.SetSaddlePad(sender.LoggedinUser.LastViewedHorse.RandomId, itemInfo.Id);
                                            sender.LoggedinUser.LastViewedHorse.Equipment.SaddlePad = itemInfo;
                                            break;
                                        case 3: // Bridle
                                            if (sender.LoggedinUser.LastViewedHorse.Equipment.Bridle != null)
                                                sender.LoggedinUser.Inventory.AddIgnoringFull(new ItemInstance(sender.LoggedinUser.LastViewedHorse.Equipment.Bridle.Id));
                                            Database.SetBridle(sender.LoggedinUser.LastViewedHorse.RandomId, itemInfo.Id);
                                            sender.LoggedinUser.LastViewedHorse.Equipment.Bridle = itemInfo;
                                            break;
                                    }


                                    sender.LoggedinUser.Inventory.Remove(sender.LoggedinUser.Inventory.GetItemByItemId(itemId).ItemInstances[0]); // Remove item from inventory.

                                    sender.LoggedinUser.MetaPriority = true;
                                    byte[] metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildTackMenu(sender.LoggedinUser.LastViewedHorse, sender.LoggedinUser));
                                    sender.SendPacket(metaPacket);

                                    byte[] equipMsgPacket = PacketBuilder.CreateChat(Messages.FormatEquipTackMessage(itemInfo.Name, sender.LoggedinUser.LastViewedHorse.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(equipMsgPacket);

                                }
                                else if(itemInfo.Type == "COMPANION")
                                {
                                    if (sender.LoggedinUser.LastViewedHorse.Equipment.Companion != null)
                                        sender.LoggedinUser.Inventory.AddIgnoringFull(new ItemInstance(sender.LoggedinUser.LastViewedHorse.Equipment.Companion.Id));
                                    Database.SetCompanion(sender.LoggedinUser.LastViewedHorse.RandomId, itemInfo.Id);
                                    sender.LoggedinUser.LastViewedHorse.Equipment.Companion = itemInfo;

                                    sender.LoggedinUser.Inventory.Remove(sender.LoggedinUser.Inventory.GetItemByItemId(itemId).ItemInstances[0]); // Remove item from inventory.

                                    sender.LoggedinUser.MetaPriority = true;
                                    byte[] metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildHorseCompanionEquipMenu(sender.LoggedinUser.LastViewedHorse, sender.LoggedinUser));
                                    sender.SendPacket(metaPacket);

                                    byte[] equipMsgPacket = PacketBuilder.CreateChat(Messages.FormatHorseCompanionEquipMessage(sender.LoggedinUser.LastViewedHorse.Name, itemInfo.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(equipMsgPacket);
                                }
                                else
                                {
                                    Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to equip a tack item to a hrose but that item was not of type \"TACK\".");
                                }
                            }
                            else
                            {
                                Logger.HackerPrint(sender.LoggedinUser.Username + " tried to equip tack he doesnt have");
                                break;
                            }
                        }
                        else
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to equip tack to a horse when not viewing one.");
                            break;
                        }
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " tried to equip tack he doesnt exist");
                        break;
                    }

                    break;
                case PacketBuilder.HORSE_TACK_UNEQUIP:
                    if (sender.LoggedinUser.LastViewedHorse != null)
                    {
                        byte equipSlot = packet[2];
                        switch(equipSlot)
                        {
                            case 0x31: // Saddle
                                if (sender.LoggedinUser.LastViewedHorse.Equipment.Saddle != null)
                                    sender.LoggedinUser.Inventory.AddIgnoringFull(new ItemInstance(sender.LoggedinUser.LastViewedHorse.Equipment.Saddle.Id));
                                Database.ClearSaddle(sender.LoggedinUser.LastViewedHorse.RandomId);
                                sender.LoggedinUser.LastViewedHorse.Equipment.Saddle = null;
                                break;
                            case 0x32: // Saddle Pad
                                if (sender.LoggedinUser.LastViewedHorse.Equipment.SaddlePad != null)
                                    sender.LoggedinUser.Inventory.AddIgnoringFull(new ItemInstance(sender.LoggedinUser.LastViewedHorse.Equipment.SaddlePad.Id));
                                Database.ClearSaddlePad(sender.LoggedinUser.LastViewedHorse.RandomId);
                                sender.LoggedinUser.LastViewedHorse.Equipment.SaddlePad = null;
                                break;
                            case 0x33: // Bridle
                                if (sender.LoggedinUser.LastViewedHorse.Equipment.Bridle != null)
                                    sender.LoggedinUser.Inventory.AddIgnoringFull(new ItemInstance(sender.LoggedinUser.LastViewedHorse.Equipment.Bridle.Id));
                                Database.ClearBridle(sender.LoggedinUser.LastViewedHorse.RandomId);
                                sender.LoggedinUser.LastViewedHorse.Equipment.Bridle = null;
                                break;
                            case 0x34: // Companion
                                if (sender.LoggedinUser.LastViewedHorse.Equipment.Companion != null)
                                    sender.LoggedinUser.Inventory.AddIgnoringFull(new ItemInstance(sender.LoggedinUser.LastViewedHorse.Equipment.Companion.Id));
                                Database.ClearCompanion(sender.LoggedinUser.LastViewedHorse.RandomId);
                                sender.LoggedinUser.LastViewedHorse.Equipment.Companion = null;
                                goto companionRemove;
                            default:
                                Logger.ErrorPrint("Unknown equip slot: " + equipSlot.ToString("X"));
                                break;
                        }
                        byte[] itemUnequipedMessage = PacketBuilder.CreateChat(Messages.FormatUnEquipTackMessage(sender.LoggedinUser.LastViewedHorse.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(itemUnequipedMessage);

                        if(sender.LoggedinUser.CurrentlyRidingHorse != null)
                        {
                            if(sender.LoggedinUser.CurrentlyRidingHorse.RandomId == sender.LoggedinUser.LastViewedHorse.RandomId)
                            {
                                byte[] disMounted = PacketBuilder.CreateChat(Messages.FormatHorseDismountedBecauseTackedMessage(sender.LoggedinUser.CurrentlyRidingHorse.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.LoggedinUser.Facing %= 5;
                                sender.LoggedinUser.CurrentlyRidingHorse = null;
                                sender.SendPacket(disMounted);
                            }
                        }

                        sender.LoggedinUser.MetaPriority = true;
                        byte[] metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildTackMenu(sender.LoggedinUser.LastViewedHorse, sender.LoggedinUser));
                        sender.SendPacket(metaPacket);
                        break;
                    companionRemove:;
                        itemUnequipedMessage = PacketBuilder.CreateChat(Messages.FormatHorseCompanionRemoveMessage(sender.LoggedinUser.LastViewedHorse.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(itemUnequipedMessage);

                        sender.LoggedinUser.MetaPriority = true;
                        metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildHorseCompanionEquipMenu(sender.LoggedinUser.LastViewedHorse, sender.LoggedinUser));
                        sender.SendPacket(metaPacket);
                        break;

                    }
                    else
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to unequip items from non existnat horse");
                    }
                    break;
                case PacketBuilder.HORSE_DISMOUNT:
                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packetStr.Length - 4);
                    try
                    {
                        randomId = int.Parse(randomIdStr);

                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if (sender.LoggedinUser.HorseInventory.HorseIdExist(randomId))
                    {
                        sender.LoggedinUser.CurrentlyRidingHorse = null;

                        byte[] stopRidingHorseMessagePacket = PacketBuilder.CreateChat(Messages.HorseStopRidingMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(stopRidingHorseMessagePacket);


                        sender.LoggedinUser.Facing %= 5;
                        byte[] rideHorsePacket = PacketBuilder.CreateHorseRidePacket(sender.LoggedinUser.X, sender.LoggedinUser.Y, sender.LoggedinUser.CharacterId, sender.LoggedinUser.Facing, 10, true);
                        sender.SendPacket(rideHorsePacket);
                        sender.LoggedinUser.NoClip = false;

                        UpdateUserInfo(sender.LoggedinUser);
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to dismount at a non existant horse.");
                        break;
                    }
                    break;
                case PacketBuilder.HORSE_MOUNT:
                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packetStr.Length - 4);
                    try
                    {
                        randomId = int.Parse(randomIdStr);

                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid randomid to horse interaction packet ");
                        break;
                    }
                    if (sender.LoggedinUser.HorseInventory.HorseIdExist(randomId))
                    {
                        HorseInstance horseMountInst = sender.LoggedinUser.HorseInventory.GetHorseById(randomId);
                        
                        if(horseMountInst.Equipment.Saddle == null || horseMountInst.Equipment.SaddlePad == null || horseMountInst.Equipment.Bridle == null)
                        {
                            byte[] horseNotTackedMessage = PacketBuilder.CreateChat(Messages.HorseCannotMountUntilTackedMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(horseNotTackedMessage);
                            break;
                        }

                        string ridingHorseMessage = Messages.FormatHorseRidingMessage(horseMountInst.Name);
                        byte[] ridingHorseMessagePacket = PacketBuilder.CreateChat(ridingHorseMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(ridingHorseMessagePacket);

                        sender.LoggedinUser.CurrentlyRidingHorse = horseMountInst;

                        // Determine what sprite to use;
                        int incBy = 0;
                        switch(horseMountInst.Color)
                        {
                            case "brown":
                                incBy = 1;
                                break;
                            case "cremello":
                            case "white":
                                incBy = 2;
                                break;
                            case "black":
                                incBy = 3;
                                break;
                            case "chestnut":
                                incBy = 4;
                                break;
                            case "bay":
                                incBy = 5;
                                break;
                            case "grey":
                                incBy = 6;
                                break;
                            case "dun":
                                incBy = 7;
                                break;
                            case "palomino":
                                incBy = 8;
                                break;
                            case "roan":
                                incBy = 9;
                                break;
                            case "pinto":
                                incBy = 10;
                                break;
                        }


                        if(horseMountInst.Breed.Type == "zebra")
                        {
                            incBy = 11;
                        }
                        if(horseMountInst.Breed.Id == 5) // Appaloosa
                        {
                            if(horseMountInst.Color == "brown")
                                incBy = 12;
                        }
                        if (horseMountInst.Breed.Type == "camel")
                        {
                            if (horseMountInst.Color == "brown")
                                incBy = 13;
                            if (horseMountInst.Color == "white")
                                incBy = 14;

                        }
                        if (horseMountInst.Breed.Type == "unicorn")
                        {
                            incBy = 15;
                        }
                        if (horseMountInst.Breed.Type == "pegasus")
                        {
                            incBy = 16;
                            sender.LoggedinUser.NoClip = true;
                        }
                        if(horseMountInst.Breed.Id == 170) // Unipeg
                        {
                            incBy = 17;
                            sender.LoggedinUser.NoClip = true;
                        }

                        incBy *= 5;
                        sender.LoggedinUser.Facing %= 5;
                        sender.LoggedinUser.Facing += incBy;


                        byte[] rideHorsePacket = PacketBuilder.CreateHorseRidePacket(sender.LoggedinUser.X, sender.LoggedinUser.Y, sender.LoggedinUser.CharacterId, sender.LoggedinUser.Facing, 10, true);
                        sender.SendPacket(rideHorsePacket);

                        UpdateUserInfo(sender.LoggedinUser);
                        break;
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to mount at a non existant horse.");
                        break;
                    }
                case PacketBuilder.HORSE_LOOK:
                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packetStr.Length - 4);
                    HorseInstance horseInst;
                    try
                    {
                        randomId = int.Parse(randomIdStr);

                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid randomid to horse interaction packet ");
                        return;
                    }
                    if(sender.LoggedinUser.HorseInventory.HorseIdExist(randomId))
                    {
                        horseInst = sender.LoggedinUser.HorseInventory.GetHorseById(randomId);
                        UpdateHorseMenu(sender, horseInst);
                    }
                    else
                    {
                        try
                        { // Not your horse? possibly viewed inside a ranch?
                            horseInst = Database.GetPlayerHorse(randomId);
                            UpdateHorseMenu(sender, horseInst);
                            break;
                        }
                        catch(Exception)
                        {
                            Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to look at a non existant horse.");
                            break;
                        }
                    }

                    break;
                case PacketBuilder.HORSE_ESCAPE:
                    if(WildHorse.DoesHorseExist(sender.LoggedinUser.CapturingHorseId))
                    {
                        WildHorse capturing = WildHorse.GetHorseById(sender.LoggedinUser.CapturingHorseId);
                        sender.LoggedinUser.CapturingHorseId = 0;

                        capturing.Escape();
                        Logger.InfoPrint(sender.LoggedinUser.Username + " Failed to capture: " + capturing.Instance.Breed.Name + " new location: " + capturing.X + ", " + capturing.Y);

                        sender.LoggedinUser.MetaPriority = true;
                        byte[] metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildHorseEscapedMessage());
                        sender.SendPacket(metaPacket);

                        UpdateAreaForAll(sender.LoggedinUser.X, sender.LoggedinUser.Y);
                    }

                    break;
                case PacketBuilder.HORSE_CAUGHT:
                    if (WildHorse.DoesHorseExist(sender.LoggedinUser.CapturingHorseId))
                    {
                        WildHorse capturing = WildHorse.GetHorseById(sender.LoggedinUser.CapturingHorseId);
                        sender.LoggedinUser.CapturingHorseId = 0;

                        try
                        {
                            capturing.Capture(sender.LoggedinUser);
                        }
                        catch(InventoryFullException)
                        {
                            byte[] chatMsg = PacketBuilder.CreateChat(Messages.TooManyHorses, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(chatMsg);
                            break;
                        }

                        sender.LoggedinUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.HorseCapture).Count++;

                        if(sender.LoggedinUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.HorseCapture).Count >= 100)
                            sender.LoggedinUser.Awards.AddAward(Award.GetAwardById(24)); // Wrangler
                        if (sender.LoggedinUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.HorseCapture).Count >= 1000)
                            sender.LoggedinUser.Awards.AddAward(Award.GetAwardById(25)); // Pro Wrangler

                        Logger.InfoPrint(sender.LoggedinUser.Username + " Captured a: " + capturing.Instance.Breed.Name + " new location: " + capturing.X + ", " + capturing.Y);

                        sender.LoggedinUser.MetaPriority = true;
                        byte[] metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildHorseCaughtMessage());
                        sender.SendPacket(metaPacket);

                        UpdateAreaForAll(sender.LoggedinUser.X, sender.LoggedinUser.Y);
                    }

                    break;
                case PacketBuilder.HORSE_TRY_CAPTURE:
                    randomId = 0;
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packetStr.Length - 4);
                    try
                    {
                        randomId = int.Parse(randomIdStr);
                        
                    }
                    catch (Exception)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid randomid to horse interaction packet ");
                        return;
                    }
                    if (!WildHorse.DoesHorseExist(randomId))
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to catch a horse that doesnt exist.");
                        return;
                    }
                    sender.LoggedinUser.CapturingHorseId = randomId;
                    byte[] chatPacket = PacketBuilder.CreateChat(Messages.HorseCaptureTimer, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(chatPacket);
                    byte[] swfModulePacket = PacketBuilder.CreateSwfModulePacket("catchhorse", PacketBuilder.PACKET_SWF_MODULE_FORCE);
                    sender.SendPacket(swfModulePacket);

                    break;
                default:
                    Logger.DebugPrint("Unknown horse packet: " + BitConverter.ToString(packet).Replace("-", " "));
                    break;
            }
        }

        public static void OnDynamicInputReceived(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent dyamic input when not logged in.");
                return;
            }
            string packetStr = Encoding.UTF8.GetString(packet);
            string dynamicInputStr = packetStr.Substring(1, packetStr.Length - 3);
            if(dynamicInputStr.Contains("|"))
            {
                string[] dynamicInput = dynamicInputStr.Split('|');
                if(dynamicInput.Length >= 1)
                {
                    int inputId = 0;
                    try
                    {
                        inputId = int.Parse(dynamicInput[0]);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to send a invalid dynamic input ");
                        return;
                    }

                    switch(inputId) 
                    {
                        case 1: // Bank
                            if (dynamicInput.Length >= 2)
                            {
                                int moneyDeposited = 0;
                                int moneyWithdrawn = 0;
                                try
                                {
                                    moneyDeposited = int.Parse(dynamicInput[1]);
                                    moneyWithdrawn = int.Parse(dynamicInput[2]);
                                }
                                catch (FormatException)
                                {
                                    Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to deposit/witthdraw NaN money....");
                                    break;
                                }

                                if((moneyDeposited <= sender.LoggedinUser.Money) && moneyDeposited != 0)
                                {
                                    sender.LoggedinUser.Money -= moneyDeposited;
                                    sender.LoggedinUser.BankMoney += Convert.ToUInt64(moneyDeposited);

                                    byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatDepositedMoneyMessage(moneyDeposited), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(chatPacket);
                                }

                                if ((Convert.ToUInt64(moneyWithdrawn) <= sender.LoggedinUser.BankMoney) && moneyWithdrawn != 0)
                                {
                                    sender.LoggedinUser.BankMoney -= Convert.ToUInt64(moneyWithdrawn);
                                    sender.LoggedinUser.Money += moneyWithdrawn;

                                    byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatWithdrawMoneyMessage(moneyWithdrawn), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(chatPacket);
                                }

                                UpdateArea(sender);
                                break;
                            }
                            else
                            {
                                Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to send a invalid dynamic input (private notes, wrong size)");
                                break;
                            }
                        case 6: // Riddle Room
                            if (dynamicInput.Length >= 2)
                            {
                                if (sender.LoggedinUser.LastRiddle != null)
                                {
                                    string answer = dynamicInput[1];
                                    if(sender.LoggedinUser.LastRiddle.CheckAnswer(sender.LoggedinUser, answer))
                                        sender.LoggedinUser.LastRiddle = null;
                                    UpdateArea(sender);
                                }
                                break;
                            }
                            else
                            {
                                Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to send a invalid dynamic input (LastRiddle, wrong size)");
                                break;
                            }
                        case 5: // Horse Description
                            if (dynamicInput.Length >= 3)
                            {
                                if(sender.LoggedinUser.LastViewedHorse != null)
                                {
                                    sender.LoggedinUser.MetaPriority = true;
                                    sender.LoggedinUser.LastViewedHorse.Name = dynamicInput[1];
                                    sender.LoggedinUser.LastViewedHorse.Description = dynamicInput[2];
                                    byte[] horseNameSavedPacket = PacketBuilder.CreateChat(Messages.FormatHorseSavedProfileMessage(sender.LoggedinUser.LastViewedHorse.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(horseNameSavedPacket);
                                    UpdateHorseMenu(sender, sender.LoggedinUser.LastViewedHorse);
                                }
                                break;
                            }
                            else
                            {
                                Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to send a invalid dynamic input (NPC Search, wrong size)");
                                break;
                            }
                        case 4: // NPC Search
                            if(dynamicInput.Length >= 2)
                            {
                                sender.LoggedinUser.MetaPriority = true;
                                string metaWindow = Meta.BuildNpcSearch(dynamicInput[1]);
                                byte[] metaPacket = PacketBuilder.CreateMetaPacket(metaWindow);
                                sender.SendPacket(metaPacket);
                                break;
                            }
                            else
                            {
                                Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to send a invalid dynamic input (NPC Search, wrong size)");
                                break;
                            }
                        case 7: // Private Notes
                            if (dynamicInput.Length >= 2)
                            {
                                sender.LoggedinUser.PrivateNotes = dynamicInput[1];
                                UpdateStats(sender);
                                byte[] chatPacket = PacketBuilder.CreateChat(Messages.PrivateNotesSavedMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(chatPacket);
                                break;
                            }
                            else
                            {
                                Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to send a invalid dynamic input (private notes, wrong size)");
                                break;
                            }
                        case 10: // Change auto sell price
                            if (dynamicInput.Length >= 2)
                            {
                                if (sender.LoggedinUser.LastViewedHorse != null)
                                {
                                    sender.LoggedinUser.MetaPriority = true;
                                    HorseInstance horseInstance = sender.LoggedinUser.LastViewedHorse;
                                    int newSellPrice = 0;
                                    try
                                    {
                                        newSellPrice = int.Parse(dynamicInput[1]);
                                    }
                                    catch (FormatException)
                                    {
                                        Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to set sell price to non int value.");
                                        break;
                                    }

                                    byte[] sellPricePacket;
                                    if (newSellPrice > 0)
                                        sellPricePacket = PacketBuilder.CreateChat(Messages.FormatAutoSellConfirmedMessage(newSellPrice), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    else
                                        sellPricePacket = PacketBuilder.CreateChat(Messages.HorseAutoSellRemoved, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(sellPricePacket);
                                    horseInstance.AutoSell = newSellPrice;

                                    UpdateHorseMenu(sender, sender.LoggedinUser.LastViewedHorse);
                                }
                            }
                            else
                            {
                                Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to send a invalid dynamic input (autosell, wrong size)");
                                break;
                            }
                            break;
                        case 11: // Ranch Description Edit
                            if (dynamicInput.Length >= 2)
                            {
                                string title = dynamicInput[1];
                                string desc = dynamicInput[2];
                                if(sender.LoggedinUser.OwnedRanch != null)
                                {
                                    sender.LoggedinUser.OwnedRanch.Title = title;
                                    sender.LoggedinUser.OwnedRanch.Description = desc;
                                }
                                byte[] descriptionEditedMessage = PacketBuilder.CreateChat(Messages.RanchSavedRanchDescripton, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(descriptionEditedMessage);
                                UpdateArea(sender);
                            }
                            else
                            {
                                Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to send a invalid dynamic input (ranch description, wrong size)");
                                break;
                            }
                            break;
                        case 12: // Abuse Report
                            if (dynamicInput.Length >= 2)
                            {
                                string userName = dynamicInput[1];
                                string reason = dynamicInput[2];
                                if(Database.CheckUserExist(userName))
                                {
                                    if(reason == "")
                                    {
                                        byte[] validReasonPlz = PacketBuilder.CreateChat(Messages.AbuseReportProvideValidReason, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                        sender.SendPacket(validReasonPlz);
                                        break;
                                    }

                                    Database.AddReport(sender.LoggedinUser.Username, userName, reason);
                                    byte[] chatPacket = PacketBuilder.CreateChat(Messages.AbuseReportFiled, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(chatPacket);
                                    UpdateArea(sender);
                                    break;
                                }
                                else
                                {
                                    byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatAbuseReportPlayerNotFound(userName), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(chatPacket);
                                    break;
                                }
                            }
                            else
                            {
                                Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to send a invalid dynamic input (private notes, wrong size)");
                                break;
                            }
                        case 14:
                            if(dynamicInput.Length >= 1)
                            {
                                string password = dynamicInput[1];
                                // Get current tile
                                if(World.InSpecialTile(sender.LoggedinUser.X, sender.LoggedinUser.Y))
                                {
                                    World.SpecialTile tile = World.GetSpecialTile(sender.LoggedinUser.X, sender.LoggedinUser.Y);
                                    if(tile.Code.StartsWith("PASSWORD-"))
                                    {
                                        string[] args = tile.Code.Replace("!","-").Split('-');
                                        if(args.Length >= 3)
                                        {
                                            string expectedPassword = args[1];
                                            int questId = int.Parse(args[2]);
                                            if(password.ToLower() == expectedPassword.ToLower())
                                            {
                                                Quest.CompleteQuest(sender.LoggedinUser, Quest.GetQuestById(questId), false);
                                            }
                                            else
                                            {
                                                Quest.QuestResult result = Quest.FailQuest(sender.LoggedinUser, Quest.GetQuestById(questId), true);
                                                if (result.NpcChat == null || result.NpcChat == "")
                                                    result.NpcChat = Messages.IncorrectPasswordMessage;
                                                byte[] ChatPacket = PacketBuilder.CreateChat(result.NpcChat, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                                sender.SendPacket(ChatPacket);
                                            }
                                        }
                                        else
                                        {
                                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Send invalid password input request. (Too few arguments!)");
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Send password input request. (Not on password tile!)");
                                        break;
                                    }
                                }
                                else
                                {
                                    Logger.HackerPrint(sender.LoggedinUser.Username + " Sent a password while not in a special tile.");
                                    break;
                                }
                            }
                            else
                            {
                                Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to send a invalid password request, (wrong size)");
                                break;
                            }

                            break;
                        default:
                            Logger.ErrorPrint("Unknown dynamic input: " + inputId.ToString() + " packet dump: " + BitConverter.ToString(packet).Replace("-", " "));
                            break;
                    }


                }
                else
                {
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to send a invalid dynamic input (wrong size)");
                    return;
                }
            }
            

        }

        public static void OnPlayerInfoPacket(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Requests player info when not logged in.");
                return;
            }
            if(packet.Length < 3)
            {
                Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent playerinfo packet of wrong size");
            }

            byte method = packet[1];
            switch(method)
            {
                case PacketBuilder.PLAYERINFO_PLAYER_LIST:
                    sender.LoggedinUser.MetaPriority = true;
                    byte[] metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildPlayerList(sender.LoggedinUser));
                    sender.SendPacket(metaPacket);
                    break;
            }

        }
        public static void OnDynamicButtonPressed(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Clicked dyamic button when not logged in.");
                return;
            }
            string packetStr = Encoding.UTF8.GetString(packet);
            string buttonIdStr = packetStr.Substring(1, packetStr.Length - 3);

            switch(buttonIdStr)
            {
                case "3": // Quest Log
                    sender.LoggedinUser.MetaPriority = true;
                    byte[] metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildQuestLog(sender.LoggedinUser));
                    sender.SendPacket(metaPacket);
                    break;
                case "4": // View Horse Breeds
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildHorseList());
                    sender.SendPacket(metaPacket);
                    break;
                case "5": // Back to horse
                    if (sender.LoggedinUser.LastViewedHorse != null)
                        UpdateHorseMenu(sender, sender.LoggedinUser.LastViewedHorse);
                    break;
                case "6": // Equip companion
                    if (sender.LoggedinUser.LastViewedHorse != null)
                    {
                        sender.LoggedinUser.MetaPriority = true;
                        HorseInstance horseInstance = sender.LoggedinUser.LastViewedHorse;
                        metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildHorseCompanionEquipMenu(horseInstance,sender.LoggedinUser));
                        sender.SendPacket(metaPacket);
                    }
                    break;
                case "7": // TP To nearest wagon (ranch)
                    if(sender.LoggedinUser.OwnedRanch != null)
                    {
                        if(sender.LoggedinUser.OwnedRanch.GetBuildingCount(7) > 0) // Wagon
                        {
                            int ranchX = sender.LoggedinUser.OwnedRanch.X;
                            int ranchY = sender.LoggedinUser.OwnedRanch.Y;

                            double smallestDistance = Double.PositiveInfinity;
                            int smalestTransportPointId = 0;
                            for (int i = 0; i < Transport.TransportPoints.Count; i++) 
                            {
                                Transport.TransportPoint tpPoint = Transport.TransportPoints[i];

                                if(Transport.GetTransportLocation(tpPoint.Locations[0]).Type == "WAGON") // is wagon?
                                {
                                    double distance = Converters.PointsToDistance(ranchX, ranchY, tpPoint.X, tpPoint.Y);
                                    if(distance < smallestDistance)
                                    {
                                        smallestDistance = distance;
                                        smalestTransportPointId = i;
                                    }
                                }
                            }
                            Transport.TransportPoint newPoint = Transport.TransportPoints[smalestTransportPointId];

                            int newX = newPoint.X;
                            int newY = newPoint.Y;

                            if (World.InSpecialTile(newX, newY))
                            {
                                World.SpecialTile tile = World.GetSpecialTile(newX, newY);
                                if (tile.ExitX != 0)
                                    newX = tile.ExitX;
                                if (tile.ExitY != 0)
                                    newY = tile.ExitY;
                            }

                            byte[] transported = PacketBuilder.CreateChat(Messages.RanchWagonDroppedYouOff, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(transported);
                            sender.LoggedinUser.Teleport(newX, newY);
                        }
                    }
                    break;
                case "8":
                    if(sender.LoggedinUser.LastViewedHorse != null)
                    {
                        sender.LoggedinUser.MetaPriority = true;
                        HorseInstance horseInstance = sender.LoggedinUser.LastViewedHorse;
                        metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildHorseReleaseConfirmationMessage(horseInstance));
                        sender.SendPacket(metaPacket);
                    }
                    break;
                case "9": // View Tack (Libary)
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildTackLibary());
                    sender.SendPacket(metaPacket);
                    break;
                case "10": // View Companions (Libary)
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildCompanionLibary());
                    sender.SendPacket(metaPacket);
                    break;
                case "11": // Randomize horse name
                    if (sender.LoggedinUser.LastViewedHorse != null)
                    {
                        sender.LoggedinUser.MetaPriority = true;
                        HorseInstance horseInstance = sender.LoggedinUser.LastViewedHorse;
                        horseInstance.ChangeNameWithoutUpdatingDatabase(HorseInfo.GenerateHorseName());
                        metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildHorseDescriptionEditMeta(horseInstance));
                        sender.SendPacket(metaPacket);
                    }
                    break;
                case "12": // View Minigames (Libary)
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildMinigamesLibary());
                    sender.SendPacket(metaPacket);
                    break;
                case "13": // Train All (Ranch)
                    if (sender.LoggedinUser.OwnedRanch != null)
                    {
                        if(sender.LoggedinUser.OwnedRanch.GetBuildingCount(6) > 0) // Training Pen
                        {
                            sender.LoggedinUser.MetaPriority = true;
                            metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildRanchTraining(sender.LoggedinUser));
                            sender.SendPacket(metaPacket);
                        }
                    }
                    break;
                case "20": // Minigame Rankings
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildMinigameRankingsForUser(sender.LoggedinUser));
                    sender.SendPacket(metaPacket);
                    break;
                case "21": // Private Notes
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildPrivateNotes(sender.LoggedinUser));
                    sender.SendPacket(metaPacket);
                    break;
                case "22": // View Locations (Libary)
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildLocationsLibary());
                    sender.SendPacket(metaPacket);
                    break;
                case "23": // View Awards (Libary)
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildAwardsLibary());
                    sender.SendPacket(metaPacket);
                    break;
                case "24": // Award List
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildAwardList(sender.LoggedinUser));
                    sender.SendPacket(metaPacket);
                    break;
                case "27": // Ranch Edit
                    if(sender.LoggedinUser.OwnedRanch != null)
                    {
                        sender.LoggedinUser.MetaPriority = true;
                        metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildRanchEdit(sender.LoggedinUser.OwnedRanch));
                        sender.SendPacket(metaPacket);
                    }
                    break;
                case "35": // Buddy List
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildBuddyList(sender.LoggedinUser));
                    sender.SendPacket(metaPacket);
                    break;
                case "36":
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildNearbyList(sender.LoggedinUser));
                    sender.SendPacket(metaPacket);
                    break;
                case "37": // All Players List
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildPlayerList());
                    sender.SendPacket(metaPacket);
                    break;
                case "40": // All Players Alphabetical
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildPlayerListAlphabetical());
                    sender.SendPacket(metaPacket);
                    break;
                case "30": // Find NPC
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildFindNpcMenu());
                    sender.SendPacket(metaPacket);
                    break;
                case "25": // Set auto sell price
                    if (sender.LoggedinUser.LastViewedHorse != null)
                    {
                        sender.LoggedinUser.MetaPriority = true;
                        HorseInstance horseInstance = sender.LoggedinUser.LastViewedHorse;
                        metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildAutoSellMenu(horseInstance));
                        sender.SendPacket(metaPacket);
                    }
                    break;
                case "33": // View All stats (Horse)
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildAllBasicStats(sender.LoggedinUser));
                    sender.SendPacket(metaPacket);
                    break;
                case "34": // View Basic stats (Horse)
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildAllStats(sender.LoggedinUser));
                    sender.SendPacket(metaPacket);
                    break;
                case "38": // Read Books
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildBooksLibary());
                    sender.SendPacket(metaPacket);
                    break;
                case "53": // Misc Stats / Tracked Items
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildMiscStats(sender.LoggedinUser));
                    sender.SendPacket(metaPacket);
                    break;
                case "60": // Ranch Sell
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildRanchSellConfirmation());
                    sender.SendPacket(metaPacket);
                    break;
                case "28c1": // Abuse Report
                    sender.LoggedinUser.MetaPriority = true;
                    metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildAbuseReportPage());
                    sender.SendPacket(metaPacket);
                    break;
                case "52c1": // Horse set to KEEPER
                    string category = "KEEPER";
                    goto setCategory;
                case "52c2": // Horse set to TRAINING
                    category = "TRAINING";
                    goto setCategory;
                case "52c3": // Horse set to TRADING
                    category = "TRADING";
                    goto setCategory;
                case "52c4": // Horse set to RETIRED
                    category = "RETIRED";
                    goto setCategory;
                setCategory:;
                    if (sender.LoggedinUser.LastViewedHorse != null)
                    {
                        sender.LoggedinUser.LastViewedHorse.Category = category;
                        byte[] categoryChangedPacket = PacketBuilder.CreateChat(Messages.FormatHorseSetToNewCategory(category), PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(categoryChangedPacket);

                        sender.LoggedinUser.MetaPriority = true;
                        UpdateHorseMenu(sender, sender.LoggedinUser.LastViewedHorse);
                    }
                    break;

                default:
                    if(buttonIdStr.StartsWith("39c")) // Book Read
                    {
                        string idStr = buttonIdStr.Substring(3);
                        int bookId = -1;
                        try
                        {
                            bookId = int.Parse(idStr);
                        }
                        catch (FormatException)
                        {
                            Logger.DebugPrint(sender.LoggedinUser.Username + " Tried to read a book of id NaN");
                            break;
                        };

                        if(Book.BookExists(bookId))
                        {
                            Book book = Book.GetBookById(bookId);
                            sender.LoggedinUser.MetaPriority = true;
                            metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildBookReadLibary(book));
                            sender.SendPacket(metaPacket);
                        }
                        else
                        {
                            Logger.HackerPrint(sender.LoggedinUser.Username + "Tried to read a book that doesnt exist.");
                        }
                        break;
                    }
                    if(buttonIdStr.StartsWith("32c")) // Horse Whisperer
                    {
                        string idStr = buttonIdStr.Substring(3);
                        int breedId = -1;
                        try
                        {
                            breedId = int.Parse(idStr);
                        }
                        catch (FormatException) {
                            Logger.DebugPrint(sender.LoggedinUser.Username + " Tried to whisper a horse with BreedId NaN.");
                            break; 
                        };
                        
                        if(sender.LoggedinUser.Money < 50000)
                        {
                            byte[] cannotAffordMessage = PacketBuilder.CreateChat(Messages.WhispererServiceCannotAfford, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(cannotAffordMessage);
                            break;
                        }

                        List<WildHorse> horsesFound = new List<WildHorse>();
                        foreach(WildHorse horse in WildHorse.WildHorses)
                        {
                            if(horse.Instance.Breed.Id == breedId)
                            {
                                horsesFound.Add(horse);
                            }
                        }
                        int cost = 0;
                        if(horsesFound.Count >= 1)
                        {
                            cost = 50000;
                        }
                        else
                        {
                            cost = 10000;
                        }

                        byte[] pricingMessage = PacketBuilder.CreateChat(Messages.FormatWhispererPrice(cost), PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(pricingMessage);

                        byte[] serachResultMeta = PacketBuilder.CreateMetaPacket(Meta.BuildWhisperSearchResults(horsesFound.ToArray()));
                        sender.SendPacket(serachResultMeta);
                        
                        sender.LoggedinUser.Money -= cost;
                        break;
                    }
                    else if(buttonIdStr.StartsWith("4c")) // Libary Breed Search
                    {
                        string idStr = buttonIdStr.Substring(2);
                        int breedId = -1;
                        HorseInfo.Breed horseBreed;
                        try
                        {
                            breedId = int.Parse(idStr);
                            horseBreed = HorseInfo.GetBreedById(breedId);
                        }
                        catch (Exception) {
                            Logger.DebugPrint(sender.LoggedinUser.Username + " Sent invalid libary breed viewer request.");
                            break; 
                        };
                        string metaTag = Meta.BuildBreedViewerLibary(horseBreed);
                        metaPacket = PacketBuilder.CreateMetaPacket(metaTag);
                        sender.SendPacket(metaPacket);

                        string swf = "breedviewer.swf?terrain=book&breed=" + horseBreed.Swf + "&j=";
                        byte[] loadSwf = PacketBuilder.CreateSwfModulePacket(swf, PacketBuilder.PACKET_SWF_MODULE_FORCE);
                        sender.SendPacket(loadSwf);

                        break;
                        
                    }
                    if(AbuseReport.DoesReasonExist(buttonIdStr))
                    {
                        sender.LoggedinUser.MetaPriority = true;
                        metaPacket = PacketBuilder.CreateMetaPacket(AbuseReport.GetReasonById(buttonIdStr).Meta);
                        sender.SendPacket(metaPacket);
                        break;
                    }

                    Logger.ErrorPrint("Dynamic button #" + buttonIdStr + " unknown... Packet Dump: "+BitConverter.ToString(packet).Replace("-", " "));
                    break;
            }
        }
        public static void OnUserInfoRequest(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Requested user information when not logged in.");
                return;
            }
            Database.AddOnlineUser(sender.LoggedinUser.Id, sender.LoggedinUser.Administrator, sender.LoggedinUser.Moderator, sender.LoggedinUser.Subscribed);
            
            Logger.DebugPrint(sender.LoggedinUser.Username + " Requested user information.");

            User user = sender.LoggedinUser;

            byte[] MovementPacket = PacketBuilder.CreateMovementPacket(user.X, user.Y, user.CharacterId, user.Facing, PacketBuilder.DIRECTION_TELEPORT, true);
            sender.SendPacket(MovementPacket);

            byte[] WelcomeMessage = PacketBuilder.CreateWelcomeMessage(user.Username);
            sender.SendPacket(WelcomeMessage);

            
            byte[] WorldData = PacketBuilder.CreateWorldData(World.ServerTime.Minutes, World.ServerTime.Days, World.ServerTime.Years, sender.LoggedinUser.GetWeatherSeen());
            sender.SendPacket(WorldData);

            // Send first time message;
            if (sender.LoggedinUser.NewPlayer)
            {
                byte[] NewUserMessage = PacketBuilder.CreateChat(Messages.NewUserMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                sender.SendPacket(NewUserMessage);
            }


            byte[] SecCodePacket = PacketBuilder.CreateSecCode(user.SecCodeSeeds, user.SecCodeInc, user.Administrator, user.Moderator);
            sender.SendPacket(SecCodePacket);

            byte[] BaseStatsPacketData = PacketBuilder.CreatePlayerData(user.Money, GameServer.GetNumberOfPlayers(), user.MailBox.MailCount);
            sender.SendPacket(BaseStatsPacketData);

            UpdateArea(sender);

            foreach (GameClient client in ConnectedClients)
            {
                if (client.LoggedIn)
                {
                    if (client.LoggedinUser.Id != user.Id)
                    {
                        byte[] PlayerInfo = PacketBuilder.CreatePlayerInfoUpdateOrCreate(client.LoggedinUser.X, client.LoggedinUser.Y, client.LoggedinUser.Facing, client.LoggedinUser.CharacterId, client.LoggedinUser.Username);
                        sender.SendPacket(PlayerInfo);
                    }
                }
            }

            foreach (User nearbyUser in GameServer.GetNearbyUsers(sender.LoggedinUser.X, sender.LoggedinUser.Y, false, false))
                if (nearbyUser.Id != sender.LoggedinUser.Id)
                    UpdateArea(nearbyUser.LoggedinClient);

            byte[] IsleData = PacketBuilder.CreatePlaceData(World.Isles.ToArray(), World.Towns.ToArray(), World.Areas.ToArray());
            sender.SendPacket(IsleData);

            byte[] TileFlags = PacketBuilder.CreateTileOverlayFlags(Map.OverlayTileDepth);
            sender.SendPacket(TileFlags);

            byte[] MotdData = PacketBuilder.CreateMotd();
            sender.SendPacket(MotdData);

        }

        public static void OnSwfModuleCommunication(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " tried to send swf communication when not logged in.");
                return;
            }
            if (packet.Length < 4)
            {
                Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid swf commmunication Packet");
                return;
            }


            byte module = packet[1];
            switch(module)
            {
                case PacketBuilder.SWFMODULE_DRAWINGROOM:
                    if(packet.Length < 3)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent invalid DRAWINGROOM packet (swf communication, WRONG SIZE)");
                        break;
                    }
                    if(packet[2] == PacketBuilder.DRAWINGROOM_GET_DRAWING)
                    {
                        if (packet.Length < 6)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent invalid DRAWINGROOM GET DRAWING packet (swf communication, WRONG SIZE)");
                            break;
                        }
                        int roomId = packet[3] - 40;
                        Drawingroom room;
                        try
                        {
                           room = Drawingroom.GetDrawingRoomById(roomId);
                        }
                        catch(KeyNotFoundException)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to load an invalid drawing room: " + roomId);
                            break;
                        }
                        if(room.Drawing != "")
                        {
                            byte[] drawingPacket = PacketBuilder.CreateDrawingUpdatePacket(room.Drawing);
                            sender.SendPacket(drawingPacket);
                        }

                    }
                    else if(packet[2] == PacketBuilder.DRAWINGROOM_SAVE)
                    {
                        if (packet.Length < 5)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent invalid DRAWINGROOM GET DRAWING packet (swf communication, WRONG SIZE)");
                            break;
                        }

                        /*
                         *   The lack of an if case for if the user isnt subscribed
                         *   is NOT a bug thats just how pinto does it.
                         *   you can save but not load if your subscribed. weird huh?
                         */

                        int roomId = packet[3] - 40;
                        Drawingroom room;
                        try
                        {
                            room = Drawingroom.GetDrawingRoomById(roomId);
                        }
                        catch (KeyNotFoundException)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to load an invalid drawing room: " + roomId);
                            break;
                        }

                        

                        if (!Database.SavedDrawingsExist(sender.LoggedinUser.Id))
                            Database.CreateSavedDrawings(sender.LoggedinUser.Id);

                        int slotNo = 0;
                        byte slot = packet[4];
                        switch (slot)
                        {
                            case 0x29: // Slot 1
                                Database.SaveDrawingSlot1(sender.LoggedinUser.Id, room.Drawing);
                                slotNo = 1;
                                break;
                            case 0x2A: // Slot 2
                                Database.SaveDrawingSlot2(sender.LoggedinUser.Id, room.Drawing);
                                slotNo = 2;
                                break;
                            case 0x2B: // Slot 3
                                Database.SaveDrawingSlot3(sender.LoggedinUser.Id, room.Drawing);
                                slotNo = 3;
                                break;
                        }

                        byte[] savedDrawingMessage = PacketBuilder.CreateChat(Messages.FormatDrawingRoomSaved(slotNo), PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(savedDrawingMessage);

                        break;
                    }
                    else if (packet[2] == PacketBuilder.DRAWINGROOM_LOAD)
                    {
                        if (packet.Length < 5)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent invalid DRAWINGROOM GET DRAWING packet (swf communication, WRONG SIZE)");
                            break;
                        }

                        if(!sender.LoggedinUser.Subscribed)
                        {
                            byte[] notSubscribedCantLoad = PacketBuilder.CreateChat(Messages.DrawingCannotLoadNotSubscribed, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(notSubscribedCantLoad);
                            break;
                        }

                        int roomId = packet[3] - 40;
                        Drawingroom room;
                        try
                        {
                            room = Drawingroom.GetDrawingRoomById(roomId);
                        }
                        catch (KeyNotFoundException)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to load an invalid drawing room: " + roomId);
                            break;
                        }

                        if (!Database.SavedDrawingsExist(sender.LoggedinUser.Id))
                            Database.CreateSavedDrawings(sender.LoggedinUser.Id);

                        int slotNo = 0;
                        byte slot = packet[4];
                        string drawingToAdd = "";
                        switch (slot)
                        {
                            case 0x29: // Slot 1
                                drawingToAdd = Database.LoadDrawingSlot1(sender.LoggedinUser.Id);
                                slotNo = 1;
                                break;
                            case 0x2A: // Slot 2
                                drawingToAdd = Database.LoadDrawingSlot2(sender.LoggedinUser.Id);
                                slotNo = 2;
                                break;
                            case 0x2B: // Slot 3
                                drawingToAdd = Database.LoadDrawingSlot3(sender.LoggedinUser.Id);
                                slotNo = 3;
                                break;
                        }

                        if (room.Drawing.Length + drawingToAdd.Length < 65535) // will this max out the db?
                        {
                            room.Drawing += drawingToAdd;
                            Database.SetLastPlayer("D" + room.Id.ToString(), sender.LoggedinUser.Id);
                        }
                        else
                        {
                            byte[] roomFullMessage = PacketBuilder.CreateChat(Messages.DrawingPlzClearLoad, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(roomFullMessage);
                            break;
                        }

                        room.Drawing += drawingToAdd;
                        UpdateDrawingForAll(sender, drawingToAdd, true);

                        byte[] loadedDrawingMessage = PacketBuilder.CreateChat(Messages.FormatDrawingRoomLoaded(slotNo), PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(loadedDrawingMessage);

                        break;
                    }
                    else // Default action- draw line
                    {
                        if (packet.Length < 5)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent invalid DRAWINGROOM GET DRAWING packet (swf communication, WRONG SIZE)");
                            break;
                        }

                        if(!sender.LoggedinUser.Subscribed)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to draw while not subscribed.");
                            byte[] notSubscribedMessage = PacketBuilder.CreateChat(Messages.DrawingNotSentNotSubscribed, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(notSubscribedMessage);
                            break;
                        }

                        int roomId = packet[2] - 40;
                        Drawingroom room;
                        try
                        {
                            room = Drawingroom.GetDrawingRoomById(roomId);
                        }
                        catch (KeyNotFoundException)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to load an invalid drawing room: " + roomId);
                            break;
                        }

                        string packetStr = Encoding.UTF8.GetString(packet);
                        
                        string drawing = packetStr.Substring(3, packetStr.Length - 5);
                        if (drawing.Contains("X")) // Clear byte
                        {
                            room.Drawing = "";
                        }
                        else if(room.Drawing.Length + drawing.Length < 65535) // will this max out the db?
                        {
                            room.Drawing += drawing;
                            Database.SetLastPlayer("D" + room.Id.ToString(), sender.LoggedinUser.Id);
                        }
                        else
                        {
                            byte[] roomFullMessage = PacketBuilder.CreateChat(Messages.DrawingPlzClearDraw, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(roomFullMessage);
                            break;
                        }

                        UpdateDrawingForAll(sender, drawing, false);

                    }

                    break;
                case PacketBuilder.SWFMODULE_BRICKPOET:
                    if(packet.Length < 5)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent invalid BRICKPOET packet (swf communication, WRONG SIZE)");
                        break;
                    }
                    if(packet[2] == PacketBuilder.BRICKPOET_LIST_ALL)
                    {
                        if (packet.Length < 6)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent invalid BRICKPOET LIST ALL packet (swf communication, WRONG SIZE)");
                            break;
                        }

                        int roomId = packet[3] - 40;
                        Brickpoet.PoetryPeice[] room;
                        try
                        {
                            room = Brickpoet.GetPoetryRoom(roomId);
                        }
                        catch(KeyNotFoundException)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to load an invalid brickpoet room: " + roomId);
                            break;
                        }

                        byte[] poetPacket = PacketBuilder.CreateBrickPoetListPacket(room);
                        sender.SendPacket(poetPacket);
                    }
                    else if(packet[3] == PacketBuilder.BRICKPOET_MOVE)
                    {
                        if (packet.Length < 0xB)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent invalid BRICKPOET MOVE packet (swf communication, WRONG SIZE)");
                            break;
                        }
                        string packetStr = Encoding.UTF8.GetString(packet);
                        if(!packetStr.Contains('|'))
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent invalid BRICKPOET MOVE packet (swf communication, NO | SEPERATOR)");
                            break;
                        }
                        string[] args = packetStr.Split('|');
                        if(args.Length < 5)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent invalid BRICKPOET MOVE Packet (swf communication, NOT ENOUGH | SEPERATORS.");
                            break;
                        }

                        int roomId = packet[2] - 40;
                        int peiceId;
                        int x;
                        int y;
                        Brickpoet.PoetryPeice[] room;
                        Brickpoet.PoetryPeice peice;

                        try
                        {
                            peiceId = int.Parse(args[1]);
                            x = int.Parse(args[2]);
                            y = int.Parse(args[3]);


                            room = Brickpoet.GetPoetryRoom(roomId);
                            peice = Brickpoet.GetPoetryPeice(room, peiceId);
                            
                        }
                        catch (Exception)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to move a peice in an invalid brickpoet room: " + roomId);
                            break;
                        }

                        peice.X = x;
                        peice.Y = y;

                        foreach(User user in GetUsersOnSpecialTileCode("MULTIROOM-" + "P" + roomId.ToString()))
                        {
                            if (user.Id == sender.LoggedinUser.Id)
                                continue;

                            byte[] updatePoetRoomPacket = PacketBuilder.CreateBrickPoetMovePacket(peice);
                            user.LoggedinClient.SendPacket(updatePoetRoomPacket);
                            
                        }

                        if (Database.GetLastPlayer("P" + roomId) != sender.LoggedinUser.Id)
                        {
                            Database.SetLastPlayer("P" + roomId, sender.LoggedinUser.Id);
                            UpdateAreaForAll(sender.LoggedinUser.X, sender.LoggedinUser.Y);
                        }

                        break;
                    }
                    else
                    {
                        Logger.DebugPrint(" packet dump: " + BitConverter.ToString(packet).Replace("-", " "));
                        break;
                    }

                    break;
                default:
                    Logger.DebugPrint("Unknown moduleid : " + module + " packet dump: " + BitConverter.ToString(packet).Replace("-"," "));
                    break;

            }

        }

        public static void OnWish(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " tried to wish when not logged in.");
                return;
            }

            if(packet.Length < 4)
            {
                Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid wish Packet");
                return;
            }

            if (!sender.LoggedinUser.Inventory.HasItemId(Item.WishingCoin))
            {
                Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to use a wishing well while having 0 coins.");
                return;
            }

            InventoryItem wishingCoinInvItems = sender.LoggedinUser.Inventory.GetItemByItemId(Item.WishingCoin);
            byte wishType = packet[1];
            string message = "";

            byte[] chatMsg = PacketBuilder.CreateChat(Messages.TossedCoin, PacketBuilder.CHAT_BOTTOM_RIGHT);
            sender.SendPacket(chatMsg);

            switch(wishType)
            {
                case PacketBuilder.WISH_MONEY:
                    int gainMoney = RandomNumberGenerator.Next(500, 1000);
                    sender.LoggedinUser.Money += gainMoney;
                    message = Messages.FormatWishMoneyMessage(gainMoney);
                    break;
                case PacketBuilder.WISH_ITEMS:
                    Item.ItemInformation[] wishableItmes = Item.GetAllWishableItems();
                    int item = RandomNumberGenerator.Next(0, wishableItmes.Length);
                    Item.ItemInformation itm = wishableItmes[item];
                    item = RandomNumberGenerator.Next(0, wishableItmes.Length);
                    Item.ItemInformation itm2 = wishableItmes[item];

                    sender.LoggedinUser.Inventory.AddIgnoringFull(new ItemInstance(itm.Id));
                    sender.LoggedinUser.Inventory.AddIgnoringFull(new ItemInstance(itm2.Id));

                    message = Messages.FormatWishThingsMessage(itm.Name, itm2.Name);
                    break;
                case PacketBuilder.WISH_WORLDPEACE:
                    byte[] tooDeep = PacketBuilder.CreateChat(Messages.WorldPeaceOnlySoDeep, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(tooDeep);

                    wishableItmes = Item.GetAllWishableItems();
                    item = RandomNumberGenerator.Next(0, wishableItmes.Length);
                    int earnMoney = RandomNumberGenerator.Next(0, 500);
                    itm = wishableItmes[item];


                    sender.LoggedinUser.Money += earnMoney;
                    sender.LoggedinUser.Inventory.AddIgnoringFull(new ItemInstance(itm.Id));

                    message = Messages.FormatWishWorldPeaceMessage(earnMoney, itm.Name);
                    break;
                default:
                    Logger.ErrorPrint("Unknnown Wish type: " + wishType.ToString("X"));
                    break;
            }
            sender.LoggedinUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.WishingWell).Count++;
            byte[] msg = PacketBuilder.CreateChat(message, PacketBuilder.CHAT_BOTTOM_RIGHT);
            sender.SendPacket(msg);

            sender.LoggedinUser.Inventory.Remove(wishingCoinInvItems.ItemInstances[0]);
            UpdateArea(sender);
        }
        public static void OnKeepAlive(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Requested update when not logged in.");
                return;
            }
            if (packet.Length < 2)
            {
                Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid update Packet");
                return;
            }

            if (packet[1] == PacketBuilder.PACKET_CLIENT_TERMINATOR)
            {
                Logger.DebugPrint("Sending " + sender.LoggedinUser.Username + " updated info...");
                UpdatePlayer(sender);
            }
        }
        public static void OnStatsPacket(GameClient sender, byte[] packet)
        {
            if(!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Requested stats when not logged in.");
                return;
            }
            if(packet.Length < 3)
            {
                Logger.ErrorPrint(sender.LoggedinUser.Username + "Sent an invalid Stats Packet");
                return;
            }


        }
        public static void OnProfilePacket(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Requested to change profile page when not logged in.");
                return;
            }
            if (packet.Length < 2)
            {
                Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid Profile Packet");
                return;
            }

            byte method = packet[1];
            if (method == PacketBuilder.PACKET_CLIENT_TERMINATOR)
            {
                UpdateStats(sender);
            }
            if (method == PacketBuilder.VIEW_PROFILE)
            {
                sender.LoggedinUser.MetaPriority = true;
                string profilePage = sender.LoggedinUser.ProfilePage;
                byte[] profilePacket = PacketBuilder.CreateProfilePacket(profilePage);
                sender.SendPacket(profilePacket);
            }
            else if (method == PacketBuilder.SAVE_PROFILE)
            {

                string packetStr = Encoding.UTF8.GetString(packet);
                if (packet.Length < 3 || !packetStr.Contains('|'))
                {
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid Profile SAVE Packet");
                    return;
                }

                int characterId = (packet[2] - 20) * 64 + (packet[3] - 20);

                string profilePage = packetStr.Split('|')[1];
                profilePage = profilePage.Substring(0, profilePage.Length - 2);
                profilePage = profilePage.Replace("[", "<");
                profilePage = profilePage.Replace("]", ">");
                sender.LoggedinUser.CharacterId = characterId;
                sender.LoggedinUser.ProfilePage = profilePage;

                Logger.DebugPrint(sender.LoggedinUser.Username + " Changed to character id: " + characterId + " and set there Profile Description to '" + profilePage + "'");

                byte[] chatPacket = PacketBuilder.CreateChat(Messages.ProfileSavedMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                sender.SendPacket(chatPacket);

                UpdateArea(sender);
                UpdateUserInfo(sender.LoggedinUser);
            }
            else if (method == PacketBuilder.SECCODE_AWARD)
            {
                byte[] ExpectedSecCode = sender.LoggedinUser.GenerateSecCode();
                byte[] GotSecCode = new byte[4];
                Array.ConstrainedCopy(packet, 2, GotSecCode, 0, GotSecCode.Length);
                Logger.DebugPrint(sender.LoggedinUser.Username + " Sent sec code: " + BitConverter.ToString(GotSecCode).Replace("-", " "));
                if (ExpectedSecCode.SequenceEqual(GotSecCode))
                {
                    if (packet.Length < 6)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent a seccode AWARD request with invalid size");
                        return;
                    }

                    string packetStr = Encoding.UTF8.GetString(packet);
                    string awardIdStr = packetStr.Substring(6, packetStr.Length - 6 - 2);

                    int value = -1;
                    try
                    {
                        value = int.Parse(awardIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent correct sec code, but invalid awardid value");
                        return;
                    }

                    sender.LoggedinUser.Awards.AddAward(Award.GetAwardById(value));
                    return;
                }
                else
                {
                    byte[] errorMessage = PacketBuilder.CreateChat(Messages.InvalidSecCodeError, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(errorMessage);
                    Logger.HackerPrint(sender.LoggedinUser.Username + " Sent invalid sec code");
                    return;
                }
            }
            else if (method == PacketBuilder.SECCODE_SCORE || method == PacketBuilder.SECCODE_TIME)
            {
                bool time = (method == PacketBuilder.SECCODE_TIME);

                byte[] ExpectedSecCode = sender.LoggedinUser.GenerateSecCode();
                byte[] GotSecCode = new byte[4];
                Array.ConstrainedCopy(packet, 2, GotSecCode, 0, GotSecCode.Length);
                Logger.DebugPrint(sender.LoggedinUser.Username + " Sent sec code: " + BitConverter.ToString(GotSecCode).Replace("-", " "));
                if (ExpectedSecCode.SequenceEqual(GotSecCode))
                {
                    if (packet.Length < 6)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent a seccode score request with invalid size");
                        return;
                    }

                    string packetStr = Encoding.UTF8.GetString(packet);
                    string gameInfoStr = packetStr.Substring(6, packetStr.Length - 6 - 2);
                    if (gameInfoStr.Contains("|"))
                    {
                        string[] gameInfo = gameInfoStr.Split('|');
                        if (gameInfo.Length < 2)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent a invalid seccode score request");
                            return;
                        }

                        string gameTitle = gameInfo[0];
                        string gameScoreStr = gameInfo[1];

                        int value = -1;
                        try
                        {
                            value = int.Parse(gameScoreStr);
                        }
                        catch (FormatException)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent correct sec code, but invalid score value");
                            return;
                        }
                        Highscore.HighscoreTableEntry[] scores = Database.GetTopScores(gameTitle, 5);
                        bool bestScoreEver = false;
                        if (scores.Length >= 1)
                            bestScoreEver = scores[0].Score <= value;

                        bool newHighscore = sender.LoggedinUser.Highscores.UpdateHighscore(gameTitle, value, time);
                        if(bestScoreEver && !time)
                        {
                            byte[] bestScoreBeaten = PacketBuilder.CreateChat(Messages.BeatBestHighscore, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(bestScoreBeaten);
                            sender.LoggedinUser.Money += 2500;
                        }
                        else if (newHighscore && !time)
                        {
                            byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatHighscoreBeatenMessage(value), PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(chatPacket);
                        }
                        else
                        {
                            byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatTimeBeatenMessage(value), PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(chatPacket);
                        }
                    }
                    else
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " didnt send a game name AND a score.");
                        return;
                    }

                }
                else
                {
                    byte[] errorMessage = PacketBuilder.CreateChat(Messages.InvalidSecCodeError, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(errorMessage);
                    Logger.HackerPrint(sender.LoggedinUser.Username + " Sent invalid sec code");
                    return;
                }
            }
            else if (method == PacketBuilder.SECCODE_MONEY)
            {

                byte[] ExpectedSecCode = sender.LoggedinUser.GenerateSecCode();
                byte[] GotSecCode = new byte[4];
                Array.ConstrainedCopy(packet, 2, GotSecCode, 0, GotSecCode.Length);
                Logger.DebugPrint(sender.LoggedinUser.Username + " Sent sec code: " + BitConverter.ToString(GotSecCode).Replace("-", " "));
                if (ExpectedSecCode.SequenceEqual(GotSecCode))
                {
                    if (packet.Length < 6)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent a seccode money request with invalid size");
                        return;
                    }

                    string packetStr = Encoding.UTF8.GetString(packet);
                    string gameInfoStr = packetStr.Substring(6, packetStr.Length - 6 - 2);
                    if (gameInfoStr.Contains("|"))
                    {
                        string[] moneyInfo = gameInfoStr.Split('|');
                        if (moneyInfo.Length < 2)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent a invalid money score request");
                            return;
                        }

                        string id = moneyInfo[0]; // not sure what this is for?

                        string moneyStr = moneyInfo[1];
                        int value = -1;
                        try
                        {
                            value = int.Parse(moneyStr);
                        }
                        catch (FormatException)
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent correct sec code, but invalid money value");
                            return;
                        }

                        int moneyEarned = value * 10;
                        Logger.InfoPrint(sender.LoggedinUser.Username + " Earned $" + moneyEarned + " In: " + id);

                        sender.LoggedinUser.Money += moneyEarned;
                        byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatMoneyEarnedMessage(moneyEarned), PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(chatPacket);

                    }
                    else
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " didnt send a game name AND a score.");
                        return;
                    }

                }
            }
            else if (method == PacketBuilder.SECCODE_GIVE_ITEM)
            {
                byte[] ExpectedSecCode = sender.LoggedinUser.GenerateSecCode();
                byte[] GotSecCode = new byte[4];
                Array.ConstrainedCopy(packet, 2, GotSecCode, 0, GotSecCode.Length);
                Logger.DebugPrint(sender.LoggedinUser.Username + " Sent sec code: " + BitConverter.ToString(GotSecCode).Replace("-", " "));
                if (ExpectedSecCode.SequenceEqual(GotSecCode))
                {
                    if (packet.Length < 6)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent a seccode item request with invalid size");
                        return;
                    }
                    string packetStr = Encoding.UTF8.GetString(packet);
                    string intStr = packetStr.Substring(6, packetStr.Length - 6 - 2);
                    int value = -1;
                    try
                    {
                        value = int.Parse(intStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent correct sec code, but invalid value");
                        return;
                    }


                    if (Item.ItemIdExist(value))
                    {
                        ItemInstance itm = new ItemInstance(value);

                        Item.ItemInformation itemInfo = Item.GetItemById(value);
                        string messageToSend = Messages.FormatYouEarnedAnItemMessage(itemInfo.Name);
                        try
                        {
                            sender.LoggedinUser.Inventory.Add(itm);
                        }
                        catch(InventoryException)
                        {
                            messageToSend = Messages.FormatYouEarnedAnItemButInventoryFullMessage(itemInfo.Name);
                        }

                        byte[] earnedItemMessage = PacketBuilder.CreateChat(messageToSend, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(earnedItemMessage);
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Sent correct sec code, but tried to give an non existant item");
                        return;
                    }

                }
                else
                {
                    byte[] errorMessage = PacketBuilder.CreateChat(Messages.InvalidSecCodeError, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(errorMessage);
                    Logger.HackerPrint(sender.LoggedinUser.Username + " Sent invalid sec code");
                    return;
                }
            }
            else if (method == PacketBuilder.SECCODE_DELETE_ITEM)
            {
                byte[] ExpectedSecCode = sender.LoggedinUser.GenerateSecCode();
                byte[] GotSecCode = new byte[4];
                Array.ConstrainedCopy(packet, 2, GotSecCode, 0, GotSecCode.Length);
                Logger.DebugPrint(sender.LoggedinUser.Username + " Sent sec code: " + BitConverter.ToString(GotSecCode).Replace("-", " "));
                if (ExpectedSecCode.SequenceEqual(GotSecCode))
                {
                    if (packet.Length < 6)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent a seccode item request with invalid size");
                        return;
                    }
                    string packetStr = Encoding.UTF8.GetString(packet);
                    string intStr = packetStr.Substring(6, packetStr.Length - 6 - 2);
                    int value = -1;
                    try
                    {
                        value = int.Parse(intStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent correct sec code, but invalid value");
                        return;
                    }


                    if (sender.LoggedinUser.Inventory.HasItemId(value))
                    {
                        InventoryItem item = sender.LoggedinUser.Inventory.GetItemByItemId(value);
                        sender.LoggedinUser.Inventory.Remove(item.ItemInstances[0]);

                        Item.ItemInformation itemInfo = Item.GetItemById(value);
                        byte[] lostItemMessage = PacketBuilder.CreateChat(Messages.FormatYouLostAnItemMessage(itemInfo.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(lostItemMessage);

                        UpdateArea(sender);
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Sent correct sec code, but tried to delete an non existant item");
                        return;
                    }

                }
                else
                {
                    byte[] errorMessage = PacketBuilder.CreateChat(Messages.InvalidSecCodeError, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(errorMessage);
                    Logger.HackerPrint(sender.LoggedinUser.Username + " Sent invalid sec code");
                    return;
                }
            }
            else if (method == PacketBuilder.SECCODE_QUEST)
            {
                byte[] ExpectedSecCode = sender.LoggedinUser.GenerateSecCode();
                byte[] GotSecCode = new byte[4];
                Array.ConstrainedCopy(packet, 2, GotSecCode, 0, GotSecCode.Length);
                Logger.DebugPrint(sender.LoggedinUser.Username + " Sent sec code: " + BitConverter.ToString(GotSecCode).Replace("-", " "));
                if (ExpectedSecCode.SequenceEqual(GotSecCode))
                {
                    if (packet.Length < 6)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent a seccode quest request with invalid size");
                        return;
                    }
                    string packetStr = Encoding.UTF8.GetString(packet);
                    string intStr = packetStr.Substring(6, packetStr.Length - 6 - 2);
                    int value = -1;
                    try
                    {
                        value = int.Parse(intStr);
                    }
                    catch (FormatException)
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Sent correct sec code, but invalid value");
                        return;
                    }


                    if (Quest.DoesQuestExist(value))
                    {
                        Quest.QuestEntry questEntry = Quest.GetQuestById(value);
                        Quest.ActivateQuest(sender.LoggedinUser, questEntry);
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Sent correct sec code, but tried to activate a non existant quest");
                        return;
                    }


                }
                else
                {
                    byte[] errorMessage = PacketBuilder.CreateChat(Messages.InvalidSecCodeError, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(errorMessage);
                    Logger.HackerPrint(sender.LoggedinUser.Username + " Sent invalid sec code");
                    return;
                }
            }
            else if (method == PacketBuilder.PROFILE_HIGHSCORES_LIST)
            {
                sender.LoggedinUser.MetaPriority = true;
                string packetStr = Encoding.UTF8.GetString(packet);
                string gameName = packetStr.Substring(2, packetStr.Length - 4);
                byte[] metaTag = PacketBuilder.CreateMetaPacket(Meta.BuildTopHighscores(gameName));
                sender.SendPacket(metaTag);
            }
            else if (method == PacketBuilder.PROFILE_BESTTIMES_LIST)
            {
                sender.LoggedinUser.MetaPriority = true;
                string packetStr = Encoding.UTF8.GetString(packet);
                string gameName = packetStr.Substring(2, packetStr.Length - 4);
                byte[] metaTag = PacketBuilder.CreateMetaPacket(Meta.BuildTopTimes(gameName));
                sender.SendPacket(metaTag);
            }

        }
        public static void OnMovementPacket(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent movement packet when not logged in.");
                return;
            }

            User loggedInUser = sender.LoggedinUser;

            // Pac-man the world.
            if (loggedInUser.X > Map.Width)
                loggedInUser.Teleport(2, loggedInUser.Y);
            else if (loggedInUser.X < 2)
                loggedInUser.Teleport(Map.Width-2, loggedInUser.Y);
            else if (loggedInUser.Y > Map.Height-2)
                loggedInUser.Teleport(loggedInUser.X, 2);
            else if (loggedInUser.Y < 2)
                loggedInUser.Teleport(loggedInUser.X, Map.Height-2);

            if (loggedInUser.CurrentlyRidingHorse != null)
            {
                if(loggedInUser.CurrentlyRidingHorse.BasicStats.Experience < 25)
                {
                    if(GameServer.RandomNumberGenerator.Next(0, 100) >= 97 || sender.LoggedinUser.Username.ToLower() == "dream")
                    {
                        loggedInUser.CurrentlyRidingHorse.BasicStats.Experience++;
                        sender.LoggedinUser.CurrentlyRidingHorse = null;
                        sender.LoggedinUser.Facing %= 5;
                        byte[] horseBuckedMessage = PacketBuilder.CreateChat(Messages.HorseBuckedYou, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(horseBuckedMessage);
                    }
                }
            }

            byte movementDirection = packet[1];

            if (loggedInUser.Thirst <= 0 || loggedInUser.Hunger <= 0 || loggedInUser.Tiredness <= 0)
            {
                if (RandomNumberGenerator.Next(0, 10) == 7 || sender.LoggedinUser.Username.ToLower() == "dream")
                {
                    byte[] possibleDirections = new byte[] { PacketBuilder.MOVE_UP, PacketBuilder.MOVE_DOWN, PacketBuilder.MOVE_RIGHT, PacketBuilder.MOVE_LEFT };

                    if (possibleDirections.Contains(movementDirection))
                    {
                        byte newDirection = possibleDirections[RandomNumberGenerator.Next(0, possibleDirections.Length)];
                        if (newDirection != movementDirection)
                        {
                            movementDirection = newDirection;
                            if (loggedInUser.Thirst <= 0)
                            {
                                byte[] chatMessage = PacketBuilder.CreateChat(Messages.FormatRandomMovementMessage(Messages.StatThirst.ToUpper()), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(chatMessage);
                            }
                            else if (loggedInUser.Hunger <= 0)
                            {
                                byte[] chatMessage = PacketBuilder.CreateChat(Messages.FormatRandomMovementMessage(Messages.StatHunger.ToUpper()), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(chatMessage);
                            }
                            else if (loggedInUser.Tiredness <= 0)
                            {
                                byte[] chatMessage = PacketBuilder.CreateChat(Messages.FormatRandomMovementMessage(Messages.StatTired.ToUpper()), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(chatMessage);
                            }

                        }
                    }
                }
            }



            int onHorse = 0;
            int facing = sender.LoggedinUser.Facing;
            while (facing >= 5)
            {
                facing = facing - 5;
                onHorse++;
            }
            byte direction = 0;
            int newX = loggedInUser.X;
            int newY = loggedInUser.Y;
            bool moveTwo = false;

            if (movementDirection == PacketBuilder.MOVE_ESCAPE)
            {

                byte Direction;
                if (World.InSpecialTile(loggedInUser.X, loggedInUser.Y))
                {

                    World.SpecialTile tile = World.GetSpecialTile(loggedInUser.X, loggedInUser.Y);
                    if (tile.ExitX != 0)
                        newX = tile.ExitX;
                    if (tile.ExitY != 0)
                        newY = tile.ExitY;
                    else
                        if (Map.CheckPassable(loggedInUser.X, loggedInUser.Y + 1) || loggedInUser.NoClip)
                            newY += 1;



                    if (loggedInUser.X + 1 == newX && loggedInUser.Y == newY)
                        Direction = PacketBuilder.DIRECTION_RIGHT;
                    else if (loggedInUser.X - 1 == newX && loggedInUser.Y == newY)
                        Direction = PacketBuilder.DIRECTION_LEFT;
                    else if (loggedInUser.Y + 1 == newY && loggedInUser.X == newX)
                        Direction = PacketBuilder.DIRECTION_DOWN;
                    else if (loggedInUser.Y - 1 == newY && loggedInUser.X == newX)
                        Direction = PacketBuilder.DIRECTION_UP;
                    else
                        Direction = PacketBuilder.DIRECTION_TELEPORT;

                    loggedInUser.X = newX;
                    loggedInUser.Y = newY;


                }
                else
                {
                    if (Map.CheckPassable(loggedInUser.X, loggedInUser.Y + 1) || loggedInUser.NoClip)
                        loggedInUser.Y += 1;

                    Direction = PacketBuilder.DIRECTION_DOWN;
                }


                loggedInUser.Facing = Direction + (onHorse * 5);
                Logger.DebugPrint("Exiting player: " + loggedInUser.Username + " to: " + loggedInUser.X + "," + loggedInUser.Y);
                byte[] moveResponse = PacketBuilder.CreateMovementPacket(loggedInUser.X, loggedInUser.Y, loggedInUser.CharacterId, loggedInUser.Facing, Direction, true);
                sender.SendPacket(moveResponse);
                Update(sender);
                return;
            }

            if (movementDirection == PacketBuilder.MOVE_UP)
            {
                direction = PacketBuilder.DIRECTION_UP;
                if (Map.CheckPassable(newX, newY - 1) || loggedInUser.NoClip)
                    newY -= 1;
                

                if (loggedInUser.Facing == (direction + (onHorse * 5)) && loggedInUser.CurrentlyRidingHorse != null && !World.InTown(loggedInUser.X, loggedInUser.Y)) // Double move
                    if (Map.CheckPassable(newX, newY - 1) || loggedInUser.NoClip)
                    {
                        newY -= 1;
                        moveTwo = true;
                    }
            }
            else if (movementDirection == PacketBuilder.MOVE_LEFT)
            {
                direction = PacketBuilder.DIRECTION_LEFT;
                if (Map.CheckPassable(newX - 1, newY) || loggedInUser.NoClip)
                    newX -= 1;


                if (loggedInUser.Facing == (direction + (onHorse * 5)) && loggedInUser.CurrentlyRidingHorse != null && !World.InTown(loggedInUser.X, loggedInUser.Y)) // Double move
                    if (Map.CheckPassable(newX - 1, newY) || loggedInUser.NoClip)
                    {
                        newX -= 1;
                        moveTwo = true;
                    }
            }
            else if (movementDirection == PacketBuilder.MOVE_RIGHT)
            {
                direction = PacketBuilder.DIRECTION_RIGHT;
                if (Map.CheckPassable(newX + 1, newY) || loggedInUser.NoClip)
                    newX += 1;


                if (loggedInUser.Facing == (direction + (onHorse * 5)) && loggedInUser.CurrentlyRidingHorse != null && !World.InTown(loggedInUser.X, loggedInUser.Y)) // Double move
                    if (Map.CheckPassable(newX + 1, newY) || loggedInUser.NoClip)
                    {
                        newX += 1;
                        moveTwo = true;
                    }
            }
            else if (movementDirection == PacketBuilder.MOVE_DOWN)
            {
                direction = PacketBuilder.DIRECTION_DOWN;
                if (Map.CheckPassable(newX, newY + 1) || loggedInUser.NoClip)
                    newY += 1;


                if (loggedInUser.Facing == (direction + (onHorse * 5)) && loggedInUser.CurrentlyRidingHorse != null && !World.InTown(loggedInUser.X, loggedInUser.Y)) // Double move
                    if (Map.CheckPassable(newX, newY + 1) || loggedInUser.NoClip)
                    {
                        newY += 1;
                        moveTwo = true;
                    }
            }
            else if(movementDirection == PacketBuilder.MOVE_UPDATE)
            {
                UpdateArea(sender);
                return;
            }

            if(loggedInUser.Y != newY || loggedInUser.X != newX)
            {
                loggedInUser.Facing = direction + (onHorse * 5);
                if (moveTwo)
                    direction += 20;
                loggedInUser.Y = newY;
                loggedInUser.X = newX;
                byte[] moveResponse = PacketBuilder.CreateMovementPacket(loggedInUser.X, loggedInUser.Y, loggedInUser.CharacterId, loggedInUser.Facing, direction, true);
                sender.SendPacket(moveResponse);
            }
            else
            {
                byte[] moveResponse = PacketBuilder.CreateMovementPacket(loggedInUser.X, loggedInUser.Y, loggedInUser.CharacterId, loggedInUser.Facing, PacketBuilder.DIRECTION_NONE, false);
                sender.SendPacket(moveResponse);
            }

            Update(sender);

        }
        public static void OnQuitPacket(GameClient sender, byte[] packet)
        {
            if(!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent quit packet when not logged in.");
                return;    
            }
            Logger.InfoPrint(sender.LoggedinUser.Username + " Clicked \"Quit Game\".. Disconnecting");
            sender.Disconnect();
        }
        public static void OnNpcInteraction(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent npc interaction packet when not logged in.");
                return;
            }
            if (packet.Length < 3)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent an invalid npc interaction packet.");
                return;
            }
            byte action = packet[1];
            if (action == PacketBuilder.NPC_START_CHAT)
            {

                string packetStr = Encoding.UTF8.GetString(packet);
                string number = packetStr.Substring(2, packetStr.Length - 4);
                int chatId = 0;
                try
                {
                    chatId = int.Parse(number);
                }
                catch (FormatException)
                {
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to start talking to an NPC with id that is NaN.");
                    return;
                }
                
                if(!Npc.NpcExists(chatId))
                {
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to start talking to an NPC that doesnt exist.");
                    return;
                }
                sender.LoggedinUser.MetaPriority = true;

                Npc.NpcEntry entry = Npc.GetNpcById(chatId);
                
                if(entry.Chatpoints.Length <= 0)
                {
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to start talking to an NPC with no chatpoints.");
                    return;
                }

                int defaultChatpointId = Npc.GetDefaultChatpoint(sender.LoggedinUser, entry);

                Npc.NpcChat startingChatpoint = Npc.GetNpcChatpoint(entry, defaultChatpointId);

                string metaInfo = Meta.BuildNpcChatpoint(sender.LoggedinUser, entry, startingChatpoint);
                byte[] metaPacket = PacketBuilder.CreateMetaPacket(metaInfo);
                sender.SendPacket(metaPacket);

                sender.LoggedinUser.LastTalkedToNpc = entry;
            }
            else if (action == PacketBuilder.NPC_CONTINUE_CHAT)
            {
                string packetStr = Encoding.UTF8.GetString(packet);
                string number = packetStr.Substring(2, packetStr.Length - 4);
                int replyId = 0;
                try
                {
                    replyId = int.Parse(number);
                }
                catch (FormatException)
                {
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to reply to an NPC with replyid that is NaN.");
                    return;
                }

                Npc.NpcEntry lastNpc = sender.LoggedinUser.LastTalkedToNpc;
                Npc.NpcReply reply;
                try
                {
                    reply = Npc.GetNpcReply(lastNpc, replyId);
                }
                catch(KeyNotFoundException)
                {
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to reply with replyid that does not exist.");
                    return;
                }

                if (reply.GotoChatpoint == -1)
                {
                    UpdateArea(sender);
                    return;
                }
                sender.LoggedinUser.MetaPriority = true;
                string metaInfo = Meta.BuildNpcChatpoint(sender.LoggedinUser, lastNpc, Npc.GetNpcChatpoint(lastNpc, reply.GotoChatpoint));
                byte[] metaPacket = PacketBuilder.CreateMetaPacket(metaInfo);
                sender.SendPacket(metaPacket);
                return;
            }
        }
        public static void OnTransportUsed(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent transport packet when not logged in.");
                return;
            }
            if (packet.Length < 3)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent an invalid transport packet.");
                return;
            }


            string packetStr = Encoding.UTF8.GetString(packet);
            string number = packetStr.Substring(1, packetStr.Length - 3);

            int transportid;
            try
            {
                transportid =  Int32.Parse(number);
            }
            catch(FormatException)
            {
                Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to use a transport with id that is NaN.");
                return;
            }
            try
            {
                Transport.TransportPoint transportPoint = Transport.GetTransportPoint(sender.LoggedinUser.X, sender.LoggedinUser.Y);
                if (transportPoint.X != sender.LoggedinUser.X && transportPoint.Y != sender.LoggedinUser.Y)
                {
                    Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to use transport id: " + transportid.ToString() + " while not the correct transport point!");
                    return;
                }

                Transport.TransportLocation transportLocation = Transport.GetTransportLocation(transportid);
                int cost = transportLocation.Cost;

                if (transportLocation.Type == "WAGON")
                {
                    if(sender.LoggedinUser.OwnedRanch != null)
                    {
                        if(sender.LoggedinUser.OwnedRanch.GetBuildingCount(7) > 0) // Wagon
                        {
                            cost = 0;
                        }
                    }
                }

                if (sender.LoggedinUser.Money >= cost)
                {
                    string swfToLoad = Messages.BoatCutscene;
                    if (transportLocation.Type == "WAGON")
                        swfToLoad = Messages.WagonCutscene;

                    if (transportLocation.Type != "ROWBOAT")
                    {
                        byte[] swfModulePacket = PacketBuilder.CreateSwfModulePacket(swfToLoad, PacketBuilder.PACKET_SWF_CUTSCENE);
                        sender.SendPacket(swfModulePacket);
                    }

                    sender.LoggedinUser.Teleport(transportLocation.GotoX, transportLocation.GotoY);
                    sender.LoggedinUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.Transport).Count++;


                    if (sender.LoggedinUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.Transport).Count >= 500)
                        sender.LoggedinUser.Awards.AddAward(Award.GetAwardById(27)); // Traveller
                    if (sender.LoggedinUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.Transport).Count >= 5000)
                        sender.LoggedinUser.Awards.AddAward(Award.GetAwardById(28)); // Globetrotter

                    byte[] welcomeToIslePacket = PacketBuilder.CreateChat(Messages.FormatWelcomeToAreaMessage(transportLocation.LocationTitle), PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(welcomeToIslePacket);

                    if(cost > 0)
                        sender.LoggedinUser.Money -= cost;
                }
                else
                {
                    byte[] cantAfford = PacketBuilder.CreateChat(Messages.CantAffordTransport, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(cantAfford);
                }
            }
            catch (KeyNotFoundException)
            {
                Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to use transport id: " + transportid.ToString() + " while not on a transport point!");
            }

         
        }
        public static void OnRanchPacket(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent ranch packet when not logged in.");
                return;
            }
            if (packet.Length < 4)
            {
                Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid ranch packet.");
                return;
            }
            string packetStr = Encoding.UTF8.GetString(packet);
            byte method = packet[1];

            if (method == PacketBuilder.RANCH_INFO)
            {
                string buildingIdStr = packetStr.Substring(2, packetStr.Length - 4);
                int buildingId = 0;
                try
                {
                    buildingId = int.Parse(buildingIdStr);
                }
                catch (FormatException)
                {
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to get info for building id NaN");
                    return;
                }
                if (Ranch.RanchBuilding.RanchBuildingExists(buildingId))
                {
                    Ranch.RanchBuilding building = Ranch.RanchBuilding.GetRanchBuildingById(buildingId);

                    byte[] ranchBuild = PacketBuilder.CreateChat(Messages.FormatBuildingInformaton(building.Title, building.Description), PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(ranchBuild);

                    return;
                }
                else
                {
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to get info for building id that didnt exist.");
                    return;
                }
            }
            else if (method == PacketBuilder.RANCH_SELL)
            {
                string NanSTR = packetStr.Substring(2, packetStr.Length - 4);
                if (NanSTR == "NaN")
                {
                    if (sender.LoggedinUser.OwnedRanch == null)
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to sell there ranch when they didnt own one.");
                        return;
                    }
                    int sellPrice = sender.LoggedinUser.OwnedRanch.GetSellPrice();
                    sender.LoggedinUser.Money += sellPrice;
                    byte[] sellPacket = PacketBuilder.CreateChat(Messages.FormatRanchSoldMessage(sellPrice), PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.LoggedinUser.OwnedRanch.OwnerId = -1;
                    sender.SendPacket(sellPacket);

                    // Change map sprite.
                    User[] users = GetUsersAt(sender.LoggedinUser.X, sender.LoggedinUser.Y, true, true);
                    foreach (User user in users)
                    {
                        byte[] MovementPacket = PacketBuilder.CreateMovementPacket(user.X, user.Y, user.CharacterId, user.Facing, PacketBuilder.DIRECTION_TELEPORT, true);
                        user.LoggedinClient.SendPacket(MovementPacket);
                    }
                    UpdateAreaForAll(sender.LoggedinUser.X, sender.LoggedinUser.Y, true);
                }
                else
                {
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to sell there ranch without sending NaN.");
                    return;
                }
            }
            else if (method == PacketBuilder.RANCH_UPGRADE)
            {
                string NanSTR = packetStr.Substring(2, packetStr.Length - 4);
                if (NanSTR == "NaN")
                {
                    if (sender.LoggedinUser.OwnedRanch != null)
                    {
                        Ranch.RanchUpgrade currentUpgrade = sender.LoggedinUser.OwnedRanch.GetRanchUpgrade();

                        if (!Ranch.RanchUpgrade.RanchUpgradeExists(currentUpgrade.Id + 1))
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to upgrade there ranch when it was max upgrade.");
                            return;
                        }

                        Ranch.RanchUpgrade nextUpgrade = Ranch.RanchUpgrade.GetRanchUpgradeById(currentUpgrade.Id + 1);
                        if (sender.LoggedinUser.Money >= nextUpgrade.Cost)
                        {
                            sender.LoggedinUser.Money -= nextUpgrade.Cost;
                            sender.LoggedinUser.OwnedRanch.InvestedMoney += nextUpgrade.Cost;
                            sender.LoggedinUser.OwnedRanch.UpgradedLevel++;

                            byte[] upgraded = PacketBuilder.CreateChat(Messages.UpgradedMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(upgraded);

                            // Change map sprite.
                            User[] users = GetUsersAt(sender.LoggedinUser.X, sender.LoggedinUser.Y, true, true);
                            foreach (User user in users)
                            {
                                byte[] MovementPacket = PacketBuilder.CreateMovementPacket(user.X, user.Y, user.CharacterId, user.Facing, PacketBuilder.DIRECTION_TELEPORT, true);
                                user.LoggedinClient.SendPacket(MovementPacket);
                            }
                            UpdateAreaForAll(sender.LoggedinUser.X, sender.LoggedinUser.Y, true);
                        }
                        else
                        {
                            byte[] cantAfford = PacketBuilder.CreateChat(Messages.UpgradeCannotAfford, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(cantAfford);
                        }
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to upgrade there ranch when they didnt own one.");
                    }
                }
                else
                {
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to upgrade there ranch without sending NaN.");
                    return;
                }
            }
            else if (method == PacketBuilder.RANCH_REMOVE)
            {
                string buildingIdStr = packetStr.Substring(2, packetStr.Length - 4);
                int buildingId = 0;
                try
                {
                    buildingId = int.Parse(buildingIdStr);
                }
                catch (FormatException)
                {
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to get info for building id NaN");
                    return;
                }
                if (Ranch.RanchBuilding.RanchBuildingExists(buildingId))
                {
                    Ranch.RanchBuilding building = Ranch.RanchBuilding.GetRanchBuildingById(buildingId);
                    int ranchBuild = sender.LoggedinUser.LastClickedRanchBuilding;
                    if (ranchBuild == 0)
                        return;
                    if (sender.LoggedinUser.OwnedRanch != null)
                    {
                        if (ranchBuild > sender.LoggedinUser.OwnedRanch.GetRanchUpgrade().Limit)
                        {
                            Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to remove more buildings than the limit.");
                            return;
                        }
                        Ranch.RanchBuilding ranchBuilding = sender.LoggedinUser.OwnedRanch.GetBuilding(ranchBuild - 1);
                        if (ranchBuilding.Id == buildingId)
                        {
                            sender.LoggedinUser.OwnedRanch.SetBuilding(ranchBuild - 1, null);
                            sender.LoggedinUser.Money += ranchBuilding.GetTeardownPrice();
                            sender.LoggedinUser.OwnedRanch.InvestedMoney -= building.Cost;
                            byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatBuildingTornDown(ranchBuilding.GetTeardownPrice()), PacketBuilder.CHAT_BOTTOM_RIGHT);

                            sender.SendPacket(chatPacket);
                            UpdateAreaForAll(sender.LoggedinUser.X, sender.LoggedinUser.Y, true);
                            return;
                        }
                        else
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to remove bulidingid: " + buildingId + " from building slot " + ranchBuild + " but the building was not found there.");
                        }

                    }
                    Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to remove in a ranch when they dont own one.");
                    return;
                }
                else
                {
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to get info for building id that didnt exist.");
                    return;
                }
            }
            else if (method == PacketBuilder.RANCH_BUILD)
            {
                string buildingIdStr = packetStr.Substring(2, packetStr.Length - 4);
                int buildingId = 0;
                try
                {
                    buildingId = int.Parse(buildingIdStr);
                }
                catch (FormatException)
                {
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to get info for building id NaN");
                    return;
                }
                if (Ranch.RanchBuilding.RanchBuildingExists(buildingId))
                {
                    Ranch.RanchBuilding building = Ranch.RanchBuilding.GetRanchBuildingById(buildingId);
                    int ranchBuild = sender.LoggedinUser.LastClickedRanchBuilding;
                    if (ranchBuild == 0)
                        return;
                    if (sender.LoggedinUser.OwnedRanch != null)
                    {
                        if (ranchBuild > sender.LoggedinUser.OwnedRanch.GetRanchUpgrade().Limit)
                        {
                            Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to build more buildings than the limit.");
                            return;
                        }

                        if (sender.LoggedinUser.Money >= building.Cost)
                        {
                            sender.LoggedinUser.OwnedRanch.SetBuilding(ranchBuild - 1, building);
                            sender.LoggedinUser.OwnedRanch.InvestedMoney += building.Cost;
                            sender.LoggedinUser.Money -= building.Cost;
                            byte[] chatPacket = PacketBuilder.CreateChat(Messages.RanchBuildingComplete, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(chatPacket);
                            UpdateAreaForAll(sender.LoggedinUser.X, sender.LoggedinUser.Y, true);
                            return;

                        }
                        else
                        {
                            byte[] chatPacket = PacketBuilder.CreateChat(Messages.RanchCantAffordThisBuilding, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(chatPacket);
                            return;
                        }
                    }
                    Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to build in a ranch when they dont own one.");
                    return;
                }
                else
                {
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to get info for building id that didnt exist.");
                    return;
                }
            }
            else if (method == PacketBuilder.RANCH_BUY)
            {
                string nan = packetStr.Substring(2, packetStr.Length - 4);
                if (nan == "NaN")
                {
                    if (Ranch.IsRanchHere(sender.LoggedinUser.X, sender.LoggedinUser.Y))
                    {
                        Ranch ranch = Ranch.GetRanchAt(sender.LoggedinUser.X, sender.LoggedinUser.Y);
                        if (sender.LoggedinUser.Money >= ranch.Value)
                        {
                            byte[] broughtRanch = PacketBuilder.CreateChat(Messages.FormatRanchBroughtMessage(ranch.Value), PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(broughtRanch);
                            sender.LoggedinUser.Money -= ranch.Value;
                            ranch.OwnerId = sender.LoggedinUser.Id;
                            ranch.InvestedMoney += ranch.Value;
                            sender.LoggedinUser.OwnedRanch = ranch;
                            sender.LoggedinUser.Inventory.AddIgnoringFull(new ItemInstance(Item.DorothyShoes));
                            UpdateAreaForAll(sender.LoggedinUser.X, sender.LoggedinUser.Y, true);

                        }
                        else
                        {
                            byte[] cantAfford = PacketBuilder.CreateChat(Messages.RanchCantAffordRanch, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(cantAfford);
                        }
                    }
                    else
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Tried to buy a non existant ranch.");
                        return;
                    }
                }
                else
                {
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent RANCH_BUY without \"NaN\".");
                    return;
                }
            }
            else if (method == PacketBuilder.RANCH_CLICK)
            {
                if (packet.Length < 6)
                {
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid ranch click packet.");
                    return;
                }
                byte action = packet[2];
                if (action == PacketBuilder.RANCH_CLICK_BUILD)
                {
                    if (Ranch.IsRanchHere(sender.LoggedinUser.X, sender.LoggedinUser.Y))
                    {
                        Ranch ranch = Ranch.GetRanchAt(sender.LoggedinUser.X, sender.LoggedinUser.Y);
                        if (sender.LoggedinUser.OwnedRanch != null)
                        {
                            if (sender.LoggedinUser.OwnedRanch.Id == ranch.Id)
                            {
                                int buildSlot = packet[3] - 40;
                                sender.LoggedinUser.LastClickedRanchBuilding = buildSlot;

                                if (buildSlot == 0)
                                {
                                    byte[] buildingsAvalible = PacketBuilder.CreateMetaPacket(Meta.BuildRanchUpgrade(ranch));
                                    sender.SendPacket(buildingsAvalible);

                                }
                                else
                                {
                                    byte[] buildingsAvalible = PacketBuilder.CreateMetaPacket(Meta.BuildRanchBuildingsAvalible(ranch, buildSlot));
                                    sender.SendPacket(buildingsAvalible);
                                }


                                return;
                            }
                        }
                    }

                    Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to build in a ranch they didnt own.");
                    return;
                }
                else if (action == PacketBuilder.RANCH_CLICK_NORM)
                {
                    if (Ranch.IsRanchHere(sender.LoggedinUser.X, sender.LoggedinUser.Y))
                    {
                        Ranch ranch = Ranch.GetRanchAt(sender.LoggedinUser.X, sender.LoggedinUser.Y);
                        int buildSlot = packet[3] - 40;
                        if (buildSlot == 0) // Main Building
                        {
                            byte[] upgradeDescription = PacketBuilder.CreateMetaPacket(Meta.BuildRanchBuilding(ranch, ranch.GetRanchUpgrade()));
                            sender.SendPacket(upgradeDescription);
                        }
                        else // Other Building
                        {
                            byte[] buildingDescription = PacketBuilder.CreateMetaPacket(Meta.BuildRanchBuilding(ranch, ranch.GetBuilding(buildSlot - 1)));
                            sender.SendPacket(buildingDescription);
                        }
                        return;
                    }
                    else
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " sent an Unknown ranch packet " + BitConverter.ToString(packet).Replace("-", " "));
                    }
                }
            }
            else
            {
                Logger.ErrorPrint(sender.LoggedinUser.Username + " sent an Unknown ranch packet " + BitConverter.ToString(packet).Replace("-", " "));
            }
        }
        public static void OnChatPacket(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent chat packet when not logged in.");
                return;
            }

            if (packet.Length < 4)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent an invalid chat packet.");
                return;
            }


            string packetStr = Encoding.UTF8.GetString(packet);

            Chat.ChatChannel channel = (Chat.ChatChannel)packet[1];
            string message = packetStr.Substring(2, packetStr.Length - 4);

            if (Chat.ProcessCommand(sender.LoggedinUser, message))
            {
                Logger.DebugPrint(sender.LoggedinUser.Username + " Attempting to run command '" + message + "' in channel: " + channel.ToString());
                return;
            }
           

            Logger.DebugPrint(sender.LoggedinUser.Username + " Attempting to say '" + message + "' in channel: " + channel.ToString());

            string nameTo = null;
            if (channel == Chat.ChatChannel.Dm)
            {
                nameTo = Chat.GetDmRecipiant(message);
                message = Chat.GetDmMessage(message);
            }

            if (message == "")
                return;

            Object violationReason = Chat.FilterMessage(message);
            if (violationReason != null)
            {
                sender.LoggedinUser.ChatViolations += 1;
                string chatViolationMessage = Messages.FormatGlobalChatViolationMessage((Chat.Reason)violationReason);
                byte[] chatViolationPacket = PacketBuilder.CreateChat(chatViolationMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                sender.SendPacket(chatViolationPacket);
                return;
            }

            byte chatSide = Chat.GetSide(channel);
            message = Chat.DoCorrections(message);
            message = Chat.EscapeMessage(message);


            string failedReason = Chat.NonViolationChecks(sender.LoggedinUser, message);
            if (failedReason != null)
            {
                byte[] failedMessage = PacketBuilder.CreateChat(failedReason, PacketBuilder.CHAT_BOTTOM_RIGHT);
                sender.SendPacket(failedMessage);
                return;
            }

            GameClient[] recipiants = Chat.GetRecipiants(sender.LoggedinUser, channel, nameTo);

            // Finally send chat message.
            string formattedMessage = Chat.FormatChatForOthers(sender.LoggedinUser, channel, message);
            string formattedMessageSender = Chat.FormatChatForSender(sender.LoggedinUser, channel, message, nameTo);
            byte[] chatPacketOthers = PacketBuilder.CreateChat(formattedMessage, chatSide);
            byte[] chatPacketSender = PacketBuilder.CreateChat(formattedMessageSender, chatSide);
            byte[] playDmSound = PacketBuilder.CreatePlaysoundPacket(Chat.PrivateMessageSound);
            // Send to clients ...
            foreach (GameClient recipiant in recipiants)
            {
                recipiant.SendPacket(chatPacketOthers);
                if (channel == Chat.ChatChannel.Dm)
                    recipiant.SendPacket(playDmSound);
            }

            // Send to sender
            sender.SendPacket(chatPacketSender);
        }
        public static void OnClickPacket(GameClient sender, byte[] packet)
        {

            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Send click packet when not logged in.");
                return;
            }
            if (packet.Length < 6)
            {
                Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid Click Packet");
                return;
            }
            
            string packetStr = Encoding.UTF8.GetString(packet);
            if(packetStr.Contains("|"))
            {
                string packetContents = packetStr.Substring(1, packetStr.Length - 3);
                string[] xy = packetContents.Split('|');
                int x = 0;
                int y = 0;

                try
                {
                    x = int.Parse(xy[0])+4;
                    y = int.Parse(xy[1])+1;
                }
                catch(FormatException)
                {
                    Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent a click packet with non-string xy value.");
                    return;
                }

                Logger.DebugPrint(sender.LoggedinUser.Username + " Clicked on tile: " + Map.GetTileId(x, y, false).ToString() + "(overlay: " + Map.GetTileId(x, y, true).ToString() + ") at " + x.ToString() + "," + y.ToString());


                // Get description of tile 
                string returnedMsg = Messages.NothingInterestingHere;
                if(World.InSpecialTile(x, y))
                {
                    World.SpecialTile tile = World.GetSpecialTile(x, y);
                    if (tile.Title != null)
                        returnedMsg = tile.Title;
                }
                if(Ranch.IsRanchHere(x, y))
                {
                    Ranch ranch = Ranch.GetRanchAt(x, y);
                    if(ranch.OwnerId == -1)
                    {
                        returnedMsg = Messages.RanchUnownedRanchClicked;
                    }
                    else
                    {
                        string title = ranch.Title;
                        if (title == null || title == "")
                            title = Messages.RanchDefaultRanchTitle;
                        returnedMsg = Messages.FormatRanchClickMessage(Database.GetUsername(ranch.OwnerId), title);
                    }
                }

                byte[] tileInfoPacket = PacketBuilder.CreateClickTileInfoPacket(returnedMsg);
                sender.SendPacket(tileInfoPacket);
            }
        }
        public static void OnItemInteraction(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent object interaction packet when not logged in.");
                return;
            }
            if (packet.Length < 3)
            {
                Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid object interaction packet.");
                return;
            }

            byte action = packet[1];
            switch(action)
            {
                case PacketBuilder.ITEM_PICKUP_ALL:
                    string chatMsg = Messages.GrabAllItemsMessage;
                    DroppedItems.DroppedItem[] droppedItems = DroppedItems.GetItemsAt(sender.LoggedinUser.X, sender.LoggedinUser.Y);

                    foreach (DroppedItems.DroppedItem item in droppedItems)
                    {
                        try
                        {
                            sender.LoggedinUser.Inventory.Add(item.Instance);
                            DroppedItems.RemoveDroppedItem(item);
                        }
                        catch (InventoryException)
                        {
                            chatMsg = Messages.GrabbedAllItemsButInventoryFull;
                        }
                    }

                    UpdateAreaForAll(sender.LoggedinUser.X, sender.LoggedinUser.Y);

                    byte[] chatMessage = PacketBuilder.CreateChat(chatMsg, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    sender.SendPacket(chatMessage);

                    break;
                case PacketBuilder.ITEM_PICKUP:
                    string packetStr = Encoding.UTF8.GetString(packet);
                    string randomIdStr = packetStr.Substring(2, packet.Length - 4);
                    int randomId = 0;

                    try
                    {
                        randomId = Int32.Parse(randomIdStr);
                    }
                    catch(FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid object interaction packet.");
                        return;
                    }

                    try
                    {
                        DroppedItems.DroppedItem item = DroppedItems.GetDroppedItemById(randomId);
                        try
                        {
                            sender.LoggedinUser.Inventory.Add(item.Instance);
                        }
                        catch (InventoryException)
                        {
                            byte[] inventoryFullMessage = PacketBuilder.CreateChat(Messages.GrabbedItemButInventoryFull, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(inventoryFullMessage);
                            break;
                        }

                        
                        DroppedItems.RemoveDroppedItem(item);

                        UpdateAreaForAll(sender.LoggedinUser.X, sender.LoggedinUser.Y);

                        chatMessage = PacketBuilder.CreateChat(Messages.GrabbedItemMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(chatMessage);
                    }
                    catch(KeyNotFoundException)
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to grab a non existing object.");
                        return;
                    }

                    break;
                case PacketBuilder.ITEM_REMOVE:
                    char toRemove = (char)packet[2];
                    switch(toRemove)
                    {
                        case '1':
                            if(sender.LoggedinUser.EquipedCompetitionGear.Head != null)
                            {
                                ItemInstance itemInstance = new ItemInstance(sender.LoggedinUser.EquipedCompetitionGear.Head.Id);
                                sender.LoggedinUser.Inventory.AddIgnoringFull(itemInstance);
                                sender.LoggedinUser.EquipedCompetitionGear.Head = null;
                            }
                            else
                            {
                                Logger.HackerPrint(sender.LoggedinUser.Username + " Attempted to remove competition gear when none was equipped.");
                            }
                            break;
                        case '2':
                            if (sender.LoggedinUser.EquipedCompetitionGear.Body != null)
                            {
                                ItemInstance itemInstance = new ItemInstance(sender.LoggedinUser.EquipedCompetitionGear.Body.Id);
                                sender.LoggedinUser.Inventory.AddIgnoringFull(itemInstance);
                                sender.LoggedinUser.EquipedCompetitionGear.Body = null;
                            }
                            else
                            {
                                Logger.HackerPrint(sender.LoggedinUser.Username + " Attempted to remove competition gear when none was equipped.");
                            }
                            break;
                        case '3':
                            if (sender.LoggedinUser.EquipedCompetitionGear.Legs != null)
                            {
                                ItemInstance itemInstance = new ItemInstance(sender.LoggedinUser.EquipedCompetitionGear.Legs.Id);
                                sender.LoggedinUser.Inventory.AddIgnoringFull(itemInstance);
                                sender.LoggedinUser.EquipedCompetitionGear.Legs = null;
                            }
                            else
                            {
                                Logger.HackerPrint(sender.LoggedinUser.Username + " Attempted to remove competition gear when none was equipped.");
                            }
                            break;
                        case '4':
                            if (sender.LoggedinUser.EquipedCompetitionGear.Feet != null)
                            {
                                ItemInstance itemInstance = new ItemInstance(sender.LoggedinUser.EquipedCompetitionGear.Feet.Id);
                                sender.LoggedinUser.Inventory.AddIgnoringFull(itemInstance);
                                sender.LoggedinUser.EquipedCompetitionGear.Feet = null;
                            }
                            else
                            {
                                Logger.HackerPrint(sender.LoggedinUser.Username + " Attempted to remove competition gear when none was equipped.");
                            }
                            break;
                        case '5':
                            if (sender.LoggedinUser.EquipedJewelry.Slot1 != null)
                            {
                                ItemInstance itemInstance = new ItemInstance(sender.LoggedinUser.EquipedJewelry.Slot1.Id);
                                sender.LoggedinUser.Inventory.AddIgnoringFull(itemInstance);
                                sender.LoggedinUser.EquipedJewelry.Slot1 = null;
                            }
                            else
                            {
                                Logger.HackerPrint(sender.LoggedinUser.Username + " Attempted to remove jewery when none was equipped.");
                            }
                            break;
                        case '6':
                            if (sender.LoggedinUser.EquipedJewelry.Slot2 != null)
                            {
                                ItemInstance itemInstance = new ItemInstance(sender.LoggedinUser.EquipedJewelry.Slot2.Id);
                                sender.LoggedinUser.Inventory.AddIgnoringFull(itemInstance);
                                sender.LoggedinUser.EquipedJewelry.Slot2 = null;
                            }
                            else
                            {
                                Logger.HackerPrint(sender.LoggedinUser.Username + " Attempted to remove jewery when none was equipped.");
                            }
                            break;
                        case '7':
                            if (sender.LoggedinUser.EquipedJewelry.Slot3 != null)
                            {
                                ItemInstance itemInstance = new ItemInstance(sender.LoggedinUser.EquipedJewelry.Slot3.Id);
                                sender.LoggedinUser.Inventory.AddIgnoringFull(itemInstance);
                                sender.LoggedinUser.EquipedJewelry.Slot3 = null;
                            }
                            else
                            {
                                Logger.HackerPrint(sender.LoggedinUser.Username + " Attempted to remove jewery when none was equipped.");
                            }
                            break;
                        case '8':
                            if (sender.LoggedinUser.EquipedJewelry.Slot4 != null)
                            {
                                ItemInstance itemInstance = new ItemInstance(sender.LoggedinUser.EquipedJewelry.Slot4.Id);
                                sender.LoggedinUser.Inventory.AddIgnoringFull(itemInstance);
                                sender.LoggedinUser.EquipedJewelry.Slot4 = null;
                            }
                            else
                            {
                                Logger.HackerPrint(sender.LoggedinUser.Username + " Attempted to remove jewery when none was equipped.");
                            }
                            break;
                        default:
                            Logger.InfoPrint(sender.LoggedinUser.Username + "Unimplemented  \"remove worn item\" ItemInteraction packet: " + BitConverter.ToString(packet).Replace("-", " "));
                            break;
                    }

                    UpdateStats(sender);
                    if(toRemove >= '1' && toRemove <= '4')
                    {
                        byte[] itemRemovedMessage = PacketBuilder.CreateChat(Messages.RemoveCompetitionGear, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(itemRemovedMessage);
                    }
                    else if (toRemove >= '5' && toRemove <= '8')
                    {
                        byte[] itemRemovedMessage = PacketBuilder.CreateChat(Messages.RemoveJewelry, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(itemRemovedMessage);
                    }
                    
                    break;
                case PacketBuilder.ITEM_USE:
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packet.Length - 2);
                    randomId = 0;

                    try
                    {
                        randomId = Int32.Parse(randomIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid object interaction packet.");
                        return;
                    }
                    if (sender.LoggedinUser.Inventory.HasItem(randomId))
                    {
                        InventoryItem itm = sender.LoggedinUser.Inventory.GetItemByRandomid(randomId);
                        if(itm.ItemId == Item.DorothyShoes)
                        {
                            if(World.InIsle(sender.LoggedinUser.X, sender.LoggedinUser.Y))
                            {
                                World.Isle isle = World.GetIsle(sender.LoggedinUser.X, sender.LoggedinUser.Y);
                                if(isle.Name == "Prison Isle")
                                {
                                    byte[] dontWorkHere = PacketBuilder.CreateChat(Messages.RanchDorothyShoesPrisonIsleMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(dontWorkHere);
                                    break;
                                }
                            }

                            if(sender.LoggedinUser.OwnedRanch == null) // How????
                            {
                                Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to use Dorothy Shoes when they did *NOT* own a ranch.");
                                sender.LoggedinUser.Inventory.Remove(itm.ItemInstances[0]);
                                break;
                            }
                            byte[] noPlaceLIke127001 = PacketBuilder.CreateChat(Messages.RanchDorothyShoesMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(noPlaceLIke127001);

                            sender.LoggedinUser.Teleport(sender.LoggedinUser.OwnedRanch.X, sender.LoggedinUser.OwnedRanch.Y);
                        }
                        else
                        {
                            Logger.ErrorPrint(sender.LoggedinUser.Username + "Tried to use item with undefined action- ID: " + itm.ItemId);
                        }
                    }
                    break;
                case PacketBuilder.ITEM_WEAR:
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packet.Length - 2);
                    randomId = 0;

                    try
                    {
                        randomId = Int32.Parse(randomIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid object interaction packet.");
                        return;
                    }
                    if (sender.LoggedinUser.Inventory.HasItem(randomId))
                    {
                        InventoryItem itm = sender.LoggedinUser.Inventory.GetItemByRandomid(randomId);
                        ItemInstance instance = itm.ItemInstances[0];
                     
                        Item.ItemInformation itemInf = instance.GetItemInfo();
                        if(itemInf.Type == "CLOTHES")
                        {
                            switch (itemInf.GetMiscFlag(0))
                            {
                                case CompetitionGear.MISC_FLAG_HEAD:
                                    if (sender.LoggedinUser.EquipedCompetitionGear.Head == null)
                                        sender.LoggedinUser.EquipedCompetitionGear.Head = itemInf;
                                    else
                                    {
                                        ItemInstance itemInstance = new ItemInstance(sender.LoggedinUser.EquipedCompetitionGear.Head.Id);
                                        sender.LoggedinUser.Inventory.AddIgnoringFull(itemInstance);
                                        sender.LoggedinUser.EquipedCompetitionGear.Head = itemInf;
                                    }
                                    break;
                                case CompetitionGear.MISC_FLAG_BODY:
                                    if (sender.LoggedinUser.EquipedCompetitionGear.Body == null)
                                        sender.LoggedinUser.EquipedCompetitionGear.Body = itemInf;
                                    else
                                    {
                                        ItemInstance itemInstance = new ItemInstance(sender.LoggedinUser.EquipedCompetitionGear.Body.Id);
                                        sender.LoggedinUser.Inventory.AddIgnoringFull(itemInstance);
                                        sender.LoggedinUser.EquipedCompetitionGear.Body = itemInf;
                                    }
                                    break;
                                case CompetitionGear.MISC_FLAG_LEGS:
                                    if (sender.LoggedinUser.EquipedCompetitionGear.Legs == null)
                                        sender.LoggedinUser.EquipedCompetitionGear.Legs = itemInf;
                                    else
                                    {
                                        ItemInstance itemInstance = new ItemInstance(sender.LoggedinUser.EquipedCompetitionGear.Legs.Id);
                                        sender.LoggedinUser.Inventory.AddIgnoringFull(itemInstance);
                                        sender.LoggedinUser.EquipedCompetitionGear.Legs = itemInf;
                                    }
                                    break;
                                case CompetitionGear.MISC_FLAG_FEET:
                                    if (sender.LoggedinUser.EquipedCompetitionGear.Feet == null)
                                        sender.LoggedinUser.EquipedCompetitionGear.Feet = itemInf;
                                    else
                                    {
                                        ItemInstance itemInstance = new ItemInstance(sender.LoggedinUser.EquipedCompetitionGear.Feet.Id);
                                        sender.LoggedinUser.Inventory.AddIgnoringFull(itemInstance);
                                        sender.LoggedinUser.EquipedCompetitionGear.Feet = itemInf;
                                    }
                                    break;
                                default: 
                                    Logger.ErrorPrint(itemInf.Name + " Has unknown misc flags.");
                                    return;
                            }
                            sender.LoggedinUser.Inventory.Remove(instance);
                            byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatEquipCompetitionGearMessage(itemInf.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(chatPacket);
                        }
                        else if(itemInf.Type == "JEWELRY")
                        {
                            bool addedJewelry = false;
                            if (sender.LoggedinUser.EquipedJewelry.Slot1 == null)
                            {
                                sender.LoggedinUser.EquipedJewelry.Slot1 = itemInf;
                                addedJewelry = true;
                            }
                            else if (sender.LoggedinUser.EquipedJewelry.Slot2 == null)
                            {
                                sender.LoggedinUser.EquipedJewelry.Slot2 = itemInf;
                                addedJewelry = true;
                            }
                            else if (sender.LoggedinUser.EquipedJewelry.Slot3 == null)
                            {
                                sender.LoggedinUser.EquipedJewelry.Slot3 = itemInf;
                                addedJewelry = true;
                            }
                            else if (sender.LoggedinUser.EquipedJewelry.Slot4 == null)
                            {
                                sender.LoggedinUser.EquipedJewelry.Slot4 = itemInf;
                                addedJewelry = true;
                            }

                            if(addedJewelry)
                            {
                                sender.LoggedinUser.Inventory.Remove(instance);
                                byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatJewerlyEquipMessage(itemInf.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(chatPacket);
                            }
                            else
                            {
                                byte[] chatPacket = PacketBuilder.CreateChat(Messages.MaxJewelryMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(chatPacket);
                            }
                        }

                        UpdateInventory(sender);
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to wear an item they did not have.");
                    }
                    break;
                case PacketBuilder.ITEM_DRINK:
                    packetStr = Encoding.UTF8.GetString(packet);
                    string idStr = packetStr.Substring(2, packet.Length - 4);
                    if(idStr == "NaN") // Fountain
                    {
                        string msg = Messages.FountainDrankYourFull;
                        bool looseMoney = RandomNumberGenerator.Next(0, 20) == 18;
                        if(looseMoney)
                        {
                            int looseAmount = RandomNumberGenerator.Next(0, 100);
                            if (looseAmount > sender.LoggedinUser.Money)
                                looseAmount = sender.LoggedinUser.Money;
                            sender.LoggedinUser.Money -= looseAmount;
                            msg = Messages.FormatDroppedMoneyMessage(looseAmount);
                        }

                        sender.LoggedinUser.Thirst = 1000;
                        byte[] drankFromFountainMessage = PacketBuilder.CreateChat(msg, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(drankFromFountainMessage);
                    }
                    else
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + "Sent unknown ITEM_DRINK command id: " + idStr);
                    }
                    break;
                case PacketBuilder.ITEM_CONSUME:
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packet.Length - 3);
                    randomId = 0;

                    try
                    {
                        randomId = Int32.Parse(randomIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid object interaction packet.");
                        return;
                    }

                    if (sender.LoggedinUser.Inventory.HasItem(randomId))
                    {
                        InventoryItem itm = sender.LoggedinUser.Inventory.GetItemByRandomid(randomId);
                        ItemInstance instance = itm.ItemInstances[0];
                        sender.LoggedinUser.Inventory.Remove(instance);
                        Item.ItemInformation itmInfo = instance.GetItemInfo();
                        bool toMuch = Item.ConsumeItem(sender.LoggedinUser, itmInfo);

                        byte[] chatPacket = PacketBuilder.CreateChat(Messages.FormatConsumeItemMessaege(itmInfo.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(chatPacket);
                        if (toMuch)
                        {
                            chatPacket = PacketBuilder.CreateChat(Messages.ConsumedButMaxReached, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(chatPacket);
                        }

                        UpdateInventory(sender);
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to consume an item they did not have.");
                    }
                    break;
                case PacketBuilder.ITEM_DROP:
                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packet.Length - 2);
                    randomId = 0;

                    try
                    {
                        randomId = Int32.Parse(randomIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid object interaction packet.");
                        return;
                    }

                    if(sender.LoggedinUser.Inventory.HasItem(randomId))
                    {
                        InventoryItem itm = sender.LoggedinUser.Inventory.GetItemByRandomid(randomId);
                        ItemInstance instance = itm.ItemInstances[0];
                        if(DroppedItems.GetItemsAt(sender.LoggedinUser.X, sender.LoggedinUser.Y).Length > 25)
                        {
                            byte[] tileIsFullPacket = PacketBuilder.CreateChat(Messages.DroppedItemTileIsFull, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(tileIsFullPacket);
                            break;
                        }
                        DroppedItems.AddItem(instance, sender.LoggedinUser.X, sender.LoggedinUser.Y);
                        sender.LoggedinUser.Inventory.Remove(instance);
                        byte[] chatPacket = PacketBuilder.CreateChat(Messages.DroppedAnItemMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(chatPacket);
                        UpdateInventory(sender);
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to drop an item they did not have.");
                    }
                    break;
                case PacketBuilder.ITEM_SHOVEL:
                    if (packet[2] != 0x14)
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Used ITEM_SHOVEL with 3rd byte not 0x14.");
                    if (!Quest.UseTool(sender.LoggedinUser, Quest.Shovel, sender.LoggedinUser.X, sender.LoggedinUser.Y))
                    {
                        byte[] ChatPacket = PacketBuilder.CreateChat(Messages.ShovelNothing, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(ChatPacket);
                    }
                    break;
                case PacketBuilder.ITEM_RAKE:
                    if (packet[2] != 0x14)
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Used ITEM_RAKE with 3rd byte not 0x14.");
                    if (!Quest.UseTool(sender.LoggedinUser, Quest.Rake, sender.LoggedinUser.X, sender.LoggedinUser.Y))
                    {
                        byte[] ChatPacket = PacketBuilder.CreateChat(Messages.RakeNothing, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(ChatPacket);
                    }
                    break;
                case PacketBuilder.ITEM_MAGNIFYING:
                    if (packet[2] != 0x14)
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Used ITEM_MAGNIFYING with 3rd byte not 0x14.");
                    if (!Quest.UseTool(sender.LoggedinUser, Quest.MagnifyingGlass, sender.LoggedinUser.X, sender.LoggedinUser.Y))
                    {
                        byte[] ChatPacket = PacketBuilder.CreateChat(Messages.MagnifyNothing, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(ChatPacket);
                    }
                    break;
                case PacketBuilder.ITEM_BINOCULARS:
                    if (packet[2] != 0x14)
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Used ITEM_BINOCULARS with 3rd byte not 0x14.");
                    if(!Quest.UseTool(sender.LoggedinUser, Quest.Binoculars, sender.LoggedinUser.X, sender.LoggedinUser.Y))
                    {
                        byte[] ChatPacket = PacketBuilder.CreateChat(Messages.BinocularsNothing, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(ChatPacket);
                    }
                    break;
                case PacketBuilder.ITEM_CRAFT:
                    packetStr = Encoding.UTF8.GetString(packet);
                    string craftIdStr = packetStr.Substring(2, packet.Length - 2);
                    int craftId = 0;
                    // Prevent crashing on non-int string.
                    try
                    {
                        craftId = Int32.Parse(craftIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " tried to craft using craft id NaN.");
                        return;
                    }
                    if(Workshop.CraftIdExists(craftId))
                    {
                        Workshop.CraftableItem itm = Workshop.GetCraftId(craftId);
                        if(itm.MoneyCost <= sender.LoggedinUser.Money) // Check money
                        {
                            foreach(Workshop.RequiredItem reqItem in itm.RequiredItems)
                            {
                                if (sender.LoggedinUser.Inventory.HasItemId(reqItem.RequiredItemId))
                                {
                                    if (sender.LoggedinUser.Inventory.GetItemByItemId(reqItem.RequiredItemId).ItemInstances.Count < reqItem.RequiredItemCount)
                                        goto failMissingItem;
                                }
                                else
                                    goto failMissingItem;
                            }

                            // Finally create the items
                            try
                            {
                                sender.LoggedinUser.Inventory.Add(new ItemInstance(itm.GiveItemId));
                            }
                            catch(InventoryException)
                            {
                                byte[] inventoryFullMessage = PacketBuilder.CreateChat(Messages.WorkshopNoRoomInInventory, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(inventoryFullMessage);
                                break;
                            }
                            sender.LoggedinUser.Money -= itm.MoneyCost;

                            // Remove the required items..
                            foreach(Workshop.RequiredItem reqItem in itm.RequiredItems) 
                                for(int i = 0; i < reqItem.RequiredItemCount; i++)
                                    sender.LoggedinUser.Inventory.Remove(sender.LoggedinUser.Inventory.GetItemByItemId(reqItem.RequiredItemId).ItemInstances[0]);

                            sender.LoggedinUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.Crafting).Count++;

                            if (sender.LoggedinUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.Crafting).Count >= 100)
                                sender.LoggedinUser.Awards.AddAward(Award.GetAwardById(22)); // Craftiness
                            if (sender.LoggedinUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.Crafting).Count >= 1000)
                                sender.LoggedinUser.Awards.AddAward(Award.GetAwardById(23)); // Workmanship

                            byte[] itemCraftSuccess = PacketBuilder.CreateChat(Messages.WorkshopCraftingSuccess, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(itemCraftSuccess);
                            break;
                            
                        }
                        else
                        {
                            byte[] cantAffordMessage = PacketBuilder.CreateChat(Messages.WorkshopCannotAfford, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(cantAffordMessage);
                            break;
                        }

                        failMissingItem:
                        {
                            byte[] missingItemMessage = PacketBuilder.CreateChat(Messages.WorkshopMissingRequiredItem, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(missingItemMessage);
                            break;
                        }
                    }

                    break;
                case PacketBuilder.ITEM_SELL: // Handles selling an item.
                    int totalSold = 1;
                    int message = 1;

                    packetStr = Encoding.UTF8.GetString(packet);
                    randomIdStr = packetStr.Substring(2, packet.Length - 2);
                    randomId = 0;
                    // Prevent crashing on non-int string.
                    try
                    {
                        randomId = Int32.Parse(randomIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid object sell packet.");
                        return;
                    }

                    if (!sender.LoggedinUser.Inventory.HasItem(randomId))
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to sell a item that they doesnt have in there inventory");
                        return;
                    }

                    InventoryItem invItem = sender.LoggedinUser.Inventory.GetItemByRandomid(randomId);
                    int itemId = invItem.ItemId;
                    goto doSell;
                case PacketBuilder.ITEM_SELL_ALL:
                    packetStr = Encoding.UTF8.GetString(packet);
                    string itemIdStr = packetStr.Substring(2, packet.Length - 2);
                    itemId = 0;
                    // Prevent crashing on non-int string.
                    try
                    {
                        itemId = Int32.Parse(itemIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid object sell packet.");
                        return;
                    }

                    if (!sender.LoggedinUser.Inventory.HasItemId(itemId))
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to sell a item that they doesnt have in there inventory");
                        return;
                    }
                    invItem = sender.LoggedinUser.Inventory.GetItemByItemId(itemId);

                    totalSold = invItem.ItemInstances.Count;
                    message = 2;
                    goto doSell;
                doSell:;

                    Item.ItemInformation itemInfo = Item.GetItemById(itemId);
                    Shop shop = sender.LoggedinUser.LastShoppedAt;
                    if (shop != null)
                    {
                        int sellPrice = shop.CalculateSellCost(itemInfo) * totalSold;
                        if (shop.CanSell(itemInfo))
                        {
                            for(int i = 0; i < totalSold; i++)
                            {
                                ItemInstance itemInstance = invItem.ItemInstances[0];
                                sender.LoggedinUser.Inventory.Remove(itemInstance);
                                shop.Inventory.Add(itemInstance);
                            }

                            sender.LoggedinUser.Money += sellPrice;

                            UpdateAreaForAll(sender.LoggedinUser.X, sender.LoggedinUser.Y);
                            if(message == 1)
                            {
                                byte[] soldItemMessage = PacketBuilder.CreateChat(Messages.FormatSellMessage(itemInfo.Name, sellPrice), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(soldItemMessage);
                            }
                            if(message == 2)
                            {
                                string name = itemInfo.Name;
                                if (totalSold > 1)
                                    name = itemInfo.PluralName;

                                byte[] soldItemMessage = PacketBuilder.CreateChat(Messages.FormatSellAllMessage(name, sellPrice,totalSold), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(soldItemMessage);
                            }

                        }
                        else
                        {
                            Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to sell a item that was not avalible to be sold.");
                        }
                    }
                    break;

                case PacketBuilder.ITEM_BUY_AND_CONSUME:
                    packetStr = Encoding.UTF8.GetString(packet);
                    itemIdStr = packetStr.Substring(2, packet.Length - 3);
                    itemId = 0;
                    // Prevent crashing on non-int string.
                    try
                    {
                        itemId = Int32.Parse(itemIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid object buy and consume packet.");
                        return;
                    }
                    if (!Item.ItemIdExist(itemId))
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to buy an itemid that doesnt even exist.");
                        break;
                    }

                    Inn lastInn = sender.LoggedinUser.LastVisitedInn;
                    if (lastInn != null)
                    {
                        try
                        {
                            itemInfo = lastInn.GetStockedItem(itemId);
                            int price = lastInn.CalculateBuyCost(itemInfo);
                            if(sender.LoggedinUser.Money >= price)
                            {
                                sender.LoggedinUser.Money -= price;
                                bool toMuch = Item.ConsumeItem(sender.LoggedinUser, itemInfo);

                                string tooMuchMessage = Messages.ConsumedButMaxReached;
                                if (itemInfo.Effects.Length > 0)
                                    if (itemInfo.Effects[0].EffectsWhat == "TIREDNESS")
                                        tooMuchMessage = Messages.InnFullyRested;
                                if (itemInfo.Effects.Length > 1)
                                    if (itemInfo.Effects[1].EffectsWhat == "TIREDNESS")
                                        tooMuchMessage = Messages.InnFullyRested;

                                byte[] enjoyedServiceMessage = PacketBuilder.CreateChat(Messages.FormatInnEnjoyedServiceMessage(itemInfo.Name, price), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(enjoyedServiceMessage);

                                if(toMuch)
                                {
                                    byte[] toMuchMessage = PacketBuilder.CreateChat(tooMuchMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                    sender.SendPacket(toMuchMessage);
                                }

                                UpdateArea(sender);
                            }
                            else
                            {
                                byte[] cantAffordMessage = PacketBuilder.CreateChat(Messages.InnCannotAffordService, PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(cantAffordMessage);
                            }
                        }
                        catch(KeyNotFoundException)
                        {
                            Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to buy and consume an item not stocked by the inn there standing on.");
                        }
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to buy and consume item while not in a inn.");
                    }
                    break;

                case PacketBuilder.ITEM_BUY: // Handles buying an item.
                    message = 1;
                    int count = 1;
                    goto doPurchase;
                case PacketBuilder.ITEM_BUY_5:
                    message = 2;
                    count = 5;
                    goto doPurchase;
                case PacketBuilder.ITEM_BUY_25:
                    message = 3;
                    count = 25;
                doPurchase:;
                    packetStr = Encoding.UTF8.GetString(packet);
                    itemIdStr = packetStr.Substring(2, packet.Length - 3);
                    itemId = 0;
                    // Prevent crashing on non-int string.
                    try
                    {
                        itemId = Int32.Parse(itemIdStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid object buy packet.");
                        return;
                    }

                    if(!Item.ItemIdExist(itemId))
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to buy an itemid that doesnt even exist.");
                        break;
                    }

                    itemInfo = Item.GetItemById(itemId);
                    shop = sender.LoggedinUser.LastShoppedAt;
                    if (shop != null)
                    {
                        int buyCost = shop.CalculateBuyCost(itemInfo) * count;
                        if (sender.LoggedinUser.Money < buyCost)
                        {
                            byte[] cantAffordMessage = PacketBuilder.CreateChat(Messages.CantAfford1, PacketBuilder.CHAT_BOTTOM_RIGHT);
                            sender.SendPacket(cantAffordMessage);
                            return;
                        }
                        if (shop.Inventory.HasItemId(itemId))
                        {
                            if (shop.Inventory.GetItemByItemId(itemId).ItemInstances.Count < count)
                            {
                                Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to buy more of an item than is in stock.");
                                break;
                            }


                            // Check we wont overflow the inventory
                            if (sender.LoggedinUser.Inventory.HasItemId(itemId)) 
                            {
                                InventoryItem items = sender.LoggedinUser.Inventory.GetItemByItemId(itemId);
                                if (items.ItemInstances.Count + count > ConfigReader.MAX_STACK)
                                {
                                    goto showError;
                                }

                            }
                            else if(sender.LoggedinUser.Inventory.Count + 1 > sender.LoggedinUser.MaxItems)
                            {
                                goto showError;
                            }

                            for (int i = 0; i < count; i++)
                            {
                                ItemInstance itemInstance = shop.Inventory.GetItemByItemId(itemId).ItemInstances[0];
                                try
                                {
                                    sender.LoggedinUser.Inventory.Add(itemInstance);
                                }
                                catch (InventoryException)
                                {
                                    Logger.ErrorPrint("Failed to add: " + itemInfo.Name + " to " + sender.LoggedinUser.Username + " inventory.");
                                    break;
                                }
                                shop.Inventory.Remove(itemInstance);
                            }

                            sender.LoggedinUser.Money -= buyCost;


                            // Send chat message to client.
                            UpdateAreaForAll(sender.LoggedinUser.X, sender.LoggedinUser.Y);
                            if (message == 1)
                            {
                                byte[] broughtItemMessage = PacketBuilder.CreateChat(Messages.FormatBuyMessage(itemInfo.Name, buyCost), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(broughtItemMessage);
                            }
                            else if (message == 2)
                            {
                                byte[] broughtItemMessage = PacketBuilder.CreateChat(Messages.FormatBuy5Message(itemInfo.PluralName, buyCost), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(broughtItemMessage);
                            }
                            else if (message == 3)
                            {
                                byte[] broughtItemMessage = PacketBuilder.CreateChat(Messages.FormatBuy25Message(itemInfo.PluralName, buyCost), PacketBuilder.CHAT_BOTTOM_RIGHT);
                                sender.SendPacket(broughtItemMessage);
                            }
                        }
                        else
                        {
                            Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to buy a item that was not for sale.");
                        }
                    }
                    else
                    {
                        Logger.HackerPrint(sender.LoggedinUser.Username + " Tried to buy an item while not in a store.");
                    }


                    break;

                showError:;
                    if (message == 1)
                    {
                        byte[] inventoryFullMessage = PacketBuilder.CreateChat(Messages.Brought1ButInventoryFull, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(inventoryFullMessage);
                    }
                    else if (message == 2)
                    {

                        byte[] inventoryFullMessage = PacketBuilder.CreateChat(Messages.Brought5ButInventoryFull, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(inventoryFullMessage);
                    }
                    else if (message == 3)
                    {

                        byte[] inventoryFullMessage = PacketBuilder.CreateChat(Messages.Brought25ButInventoryFull, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        sender.SendPacket(inventoryFullMessage);
                    }
                    break;

                case PacketBuilder.PACKET_INFORMATION:
                    packetStr = Encoding.UTF8.GetString(packet);
                    string valueStr = packetStr.Substring(3, packet.Length - 3);
                    int value = 0;
                    try
                    {
                        value = Int32.Parse(valueStr);
                    }
                    catch (FormatException)
                    {
                        Logger.ErrorPrint(sender.LoggedinUser.Username + " Sent an invalid object interaction packet.");
                        return;
                    }
                    if (packet[2] == PacketBuilder.ITEM_INFORMATON)
                    {
                        itemId = -1;
                        if (sender.LoggedinUser.Inventory.HasItem(value))
                            itemId = sender.LoggedinUser.Inventory.GetItemByRandomid(value).ItemId;
                        else if (DroppedItems.IsDroppedItemExist(value))
                            itemId = DroppedItems.GetDroppedItemById(value).Instance.ItemId;
                        if (itemId == -1)
                        {
                            Logger.HackerPrint(sender.LoggedinUser.Username + " asked for details of non existiant item.");
                            return;
                        }
                        sender.LoggedinUser.MetaPriority = true;
                        Item.ItemInformation info = Item.GetItemById(itemId);
                        string infoMessage = Meta.BuildItemInfo(info);
                        byte[] metaPacket = PacketBuilder.CreateMetaPacket(infoMessage);
                        sender.SendPacket(metaPacket);
                    }
                    if (packet[2] == PacketBuilder.ITEM_INFORMATON_ID)
                    {
                        sender.LoggedinUser.MetaPriority = true;
                        if (!Item.ItemIdExist(value))
                        {
                            Logger.HackerPrint(sender.LoggedinUser.Username + " asked for details of non existiant item.");
                            return;
                        }

                        Item.ItemInformation info = Item.GetItemById(value);
                        string infoMessage = Meta.BuildItemInfo(info);
                        byte[] metaPacket = PacketBuilder.CreateMetaPacket(infoMessage);
                        sender.SendPacket(metaPacket);
                    }
                    else if(packet[2] == PacketBuilder.NPC_INFORMATION)
                    {
                        if(Npc.NpcExists(value))
                        {
                            sender.LoggedinUser.MetaPriority = true;
                            Npc.NpcEntry npc = Npc.GetNpcById(value);
                            string infoMessage = Meta.BuildNpcInfo(npc);
                            byte[] metaPacket = PacketBuilder.CreateMetaPacket(infoMessage);
                            sender.SendPacket(metaPacket);
                        }
                        else
                        {
                            Logger.HackerPrint(sender.LoggedinUser.Username + " asked for details of non existiant npc.");
                            return;
                        }
                    }

                    break;
                default:
                    Logger.WarnPrint(sender.LoggedinUser.Username + " Sent an unknown Item Interaction Packet type: " + action.ToString() + ", Packet Dump: " + BitConverter.ToString(packet).Replace('-', ' '));
                    break;
            }

        }
        public static void OnInventoryRequested(GameClient sender, byte[] packet)
        {
            if (!sender.LoggedIn)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent chat packet when not logged in.");
                return;
            }

            if (packet.Length < 2)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent an invalid inventory request packet.");
                return;
            }

            UpdateInventory(sender);
        }
        public static void OnLoginRequest(GameClient sender, byte[] packet)
        {
            Logger.DebugPrint("Login request received from: " + sender.RemoteIp);

            string loginRequestString = Encoding.UTF8.GetString(packet).Substring(1);

            if (!loginRequestString.Contains('|') || packet.Length < 3)
            {
                Logger.ErrorPrint(sender.RemoteIp + " Sent an invalid login request");
                return;
            }

            if (packet[1] != PacketBuilder.PACKET_CLIENT_TERMINATOR)
            {
                string[] loginParts = loginRequestString.Split('|');
                if (loginParts.Length < 3)
                {
                    Logger.ErrorPrint(sender.RemoteIp + " Sent a login request of invalid length. " + loginRequestString);
                    return;
                }

                int version = int.Parse(loginParts[0]);
                string encryptedUsername = loginParts[1];
                string encryptedPassword = loginParts[2];
                string username = Authentication.DecryptLogin(encryptedUsername);
                string password = Authentication.DecryptLogin(encryptedPassword);

                if (Authentication.CheckPassword(username, password))
                {
                    // Obtain user information
                    int userId = Database.GetUserid(username);

                    if(Database.IsUserBanned(userId))
                    {
                        Logger.DebugPrint(sender.RemoteIp + " Tried to login to : " + username + " but, the account was banned.");
                        byte[] userBannedPacket = PacketBuilder.CreateLoginPacket(false, Messages.LoginFailedReasonBanned);
                        sender.SendPacket(userBannedPacket);
                        return;
                    }

                    if(Database.IsIpBanned(sender.RemoteIp))
                    {
                        Logger.DebugPrint(sender.RemoteIp + " Tried to login to : " + username + " but, the IP was banned.");
                        byte[] ipBannedPacket = PacketBuilder.CreateLoginPacket(false, Messages.FormatIpBannedMessage(sender.RemoteIp));
                        sender.SendPacket(ipBannedPacket);
                        return;
                    }


                    sender.Login(userId);
                    sender.LoggedinUser.Password = password;

                    byte[] ResponsePacket = PacketBuilder.CreateLoginPacket(true);
                    sender.SendPacket(ResponsePacket);

                    Logger.DebugPrint(sender.RemoteIp + " Logged into : " + sender.LoggedinUser.Username + " (ADMIN: " + sender.LoggedinUser.Administrator + " MOD: " + sender.LoggedinUser.Moderator + ")");

                    // Send login message
                    byte[] loginMessageBytes = PacketBuilder.CreateChat(Messages.FormatLoginMessage(sender.LoggedinUser.Username), PacketBuilder.CHAT_BOTTOM_LEFT);
                    foreach (GameClient client in ConnectedClients)
                        if (client.LoggedIn)
                            if (!client.LoggedinUser.MuteLogins && !client.LoggedinUser.MuteAll)
                                if (client.LoggedinUser.Id != userId)
                                        client.SendPacket(loginMessageBytes);

                    UpdateUserInfo(sender.LoggedinUser);

                }
                else
                {
                    Logger.WarnPrint(sender.RemoteIp + " Attempted to login to: " + username + " with incorrect password " + password);
                    byte[] ResponsePacket = PacketBuilder.CreateLoginPacket(false);
                    sender.SendPacket(ResponsePacket);
                }
            }

        }

        public static void OnDisconnect(GameClient sender)
        {
            connectedClients.Remove(sender);
            if (sender.LoggedIn)
            {
                Database.SetPlayerLastLogin(Convert.ToInt32(new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()), sender.LoggedinUser.Id); // Set last login date

                Database.RemoveOnlineUser(sender.LoggedinUser.Id);
                // Send disconnect message
                byte[] logoutMessageBytes = PacketBuilder.CreateChat(Messages.FormatLogoutMessage(sender.LoggedinUser.Username), PacketBuilder.CHAT_BOTTOM_LEFT);
                foreach (GameClient client in ConnectedClients)
                    if (client.LoggedIn)
                        if (!client.LoggedinUser.MuteLogins && !client.LoggedinUser.MuteAll)
                            if (client.LoggedinUser.Id != sender.LoggedinUser.Id)
                                client.SendPacket(logoutMessageBytes);
                // Tell clients of diconnect (remove from chat)
                byte[] playerRemovePacket = PacketBuilder.CreatePlayerLeavePacket(sender.LoggedinUser.Username);
                foreach (GameClient client in ConnectedClients)
                    if (client.LoggedIn)
                        if (client.LoggedinUser.Id != sender.LoggedinUser.Id)
                            client.SendPacket(playerRemovePacket);
            }

        }

        /*
         *  Get(Some Information)
         */


        public static bool IsUserOnline(int id)
        {
            try
            {
                GetUserById(id);
                return true;
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
        }

        public static User[] GetUsersInTown(World.Town town, bool includeStealth = false, bool includeMuted = false)
        {
            List<User> usersInTown = new List<User>();
            foreach (GameClient client in ConnectedClients)
                if (client.LoggedIn)
                {
                    if (!includeStealth && client.LoggedinUser.Stealth)
                        continue;
                    if (!includeMuted && client.LoggedinUser.MuteIsland)
                        continue;
                    if (World.InTown(client.LoggedinUser.X, client.LoggedinUser.Y))
                        if (World.GetIsle(client.LoggedinUser.X, client.LoggedinUser.Y).Name == town.Name)
                            usersInTown.Add(client.LoggedinUser);
                }

            return usersInTown.ToArray();
        }
        public static User[] GetUsersInIsle(World.Isle isle, bool includeStealth = false, bool includeMuted = false)
        {
            List<User> usersInIsle = new List<User>();
            foreach (GameClient client in ConnectedClients)
                if (client.LoggedIn)
                {
                    if (!includeStealth && client.LoggedinUser.Stealth)
                        continue;
                    if (!includeMuted && client.LoggedinUser.MuteIsland)
                        continue;
                    if (World.InIsle(client.LoggedinUser.X, client.LoggedinUser.Y))
                        if (World.GetIsle(client.LoggedinUser.X, client.LoggedinUser.Y).Name == isle.Name)
                            usersInIsle.Add(client.LoggedinUser);
                }

            return usersInIsle.ToArray();
        }

        public static User[] GetUsersOnSpecialTileCode(string code)
        {
            List<User> userList = new List<User>();

            foreach (GameClient client in connectedClients)
            {
                if (client.LoggedIn)
                {

                    if (World.InSpecialTile(client.LoggedinUser.X, client.LoggedinUser.Y))
                    {
                        World.SpecialTile tile = World.GetSpecialTile(client.LoggedinUser.X, client.LoggedinUser.Y);

                        if (tile.Code == code)
                        {
                            userList.Add(client.LoggedinUser);
                        }
                    }
                }
            }
            return userList.ToArray();
        }
        public static User[] GetUsersAt(int x, int y, bool includeStealth = false, bool includeMuted = false)
        {
            List<User> usersHere = new List<User>();
            foreach(GameClient client in ConnectedClients)
            {
                if(client.LoggedIn)
                {
                    if (!includeStealth && client.LoggedinUser.Stealth)
                        continue;
                    if (!includeMuted && client.LoggedinUser.MuteNear)
                        continue;
                    if (client.LoggedinUser.X == x && client.LoggedinUser.Y == y)
                        usersHere.Add(client.LoggedinUser);
                }
            }
            return usersHere.ToArray();
        }
        public static User GetUserByName(string username)
        {
            foreach(GameClient client in ConnectedClients)
            {
                if(client.LoggedIn)
                {
                    if (client.LoggedinUser.Username == username)
                        return client.LoggedinUser;
                }
            }
            throw new KeyNotFoundException("User was not found.");
        }

        public static User GetUserById(int id)
        {
            foreach(GameClient client in ConnectedClients)
            {
                if (client.LoggedIn)
                    if (client.LoggedinUser.Id == id)
                        return client.LoggedinUser;
            }

            throw new KeyNotFoundException("User not found (not online?)");
        }
        public static User[] GetNearbyUsers(int x, int y, bool includeStealth=false, bool includeMuted=false)
        {
            int startX = x - 15;
            int endX = x + 15;
            int startY = y - 19;
            int endY = y + 19;
            List<User> usersNearby = new List<User>();

            foreach (GameClient client in ConnectedClients)
                if (client.LoggedIn)
                {
                    if (!includeStealth && client.LoggedinUser.Stealth)
                        continue;
                    if (!includeMuted && client.LoggedinUser.MuteNear)
                        continue;
                    if (startX <= client.LoggedinUser.X && endX >= client.LoggedinUser.X && startY <= client.LoggedinUser.Y && endY >= client.LoggedinUser.Y)
                        usersNearby.Add(client.LoggedinUser);
                }

            return usersNearby.ToArray();
        }
        public static int GetNumberOfPlayers(bool includeStealth=false)
        {
            int count = 0;
            foreach(GameClient client in ConnectedClients)
                if (client.LoggedIn)
                {
                    if (!includeStealth && client.LoggedinUser.Stealth)
                        continue;
                    if (!client.LoggedinUser.Stealth)
                        count++;
                }
            
            return count;
        }

        public static Point[] GetAllBuddyLocations(User caller)
        {
            List<Point> allLocations = new List<Point>();

            foreach (GameClient client in ConnectedClients)
            {
                if (client.LoggedIn)
                {

                    if (!caller.Friends.List.Contains(client.LoggedinUser.Id))
                        continue;
                    

                    if (!client.LoggedinUser.Stealth)
                        allLocations.Add(new Point(client.LoggedinUser.X, client.LoggedinUser.Y));

                }
            }

            return allLocations.ToArray();
        }

        public static Point[] GetAllPlayerLocations(User caller)
        {
            List<Point> allLocations = new List<Point>();
            
            foreach (GameClient client in ConnectedClients)
            {
                if (client.LoggedIn)
                {

                    if (client.LoggedinUser.Id == caller.Id) 
                        continue;
                    
                    if (!client.LoggedinUser.Stealth)
                        allLocations.Add(new Point(client.LoggedinUser.X, client.LoggedinUser.Y));
                    
                }

                        
            }
            return allLocations.ToArray();
        }
        public static int GetNumberOfPlayersListeningToAdsChat()
        {
            int count = 0;
            foreach (GameClient client in ConnectedClients)
            {
                if (client.LoggedIn)
                    if (!client.LoggedinUser.MuteAds)
                        count++;
            }
            return count;
        }

        public static int GetNumberOfModsOnline()
        {
            int count = 0;
            foreach (GameClient client in ConnectedClients)
            {
                if (client.LoggedIn)
                    if(client.LoggedinUser.Moderator)
                        count++;
            }
            return count;
        }
        public static int GetNumberOfAdminsOnline()
        {
            int count = 0;
            foreach (GameClient client in ConnectedClients)
            {
                if (client.LoggedIn)
                    if (client.LoggedinUser.Administrator)
                        count++;
            }
            return count;
        }

        /*
         *  Update game state functions.
         */

        public static void Update(GameClient client)
        {
            UpdateArea(client);
            foreach (User nearbyUser in GameServer.GetNearbyUsers(client.LoggedinUser.X, client.LoggedinUser.Y, false, false))
                if (nearbyUser.Id != client.LoggedinUser.Id)
                    if(!nearbyUser.MetaPriority)
                        UpdateArea(nearbyUser.LoggedinClient);

            UpdateWeather(client);
            UpdateUserInfo(client.LoggedinUser);
        }

        public static void UpdateDrawingForAll(GameClient sender, string drawing, bool includingSender=false)
        {

            UpdateAreaForAll(sender.LoggedinUser.X, sender.LoggedinUser.Y);
            User[] usersHere = GetUsersAt(sender.LoggedinUser.X, sender.LoggedinUser.Y, true, true);
            foreach (User user in usersHere)
            {
                if(!includingSender)
                    if (user.Id == sender.LoggedinUser.Id)
                        continue;
                

                byte[] patchDrawing = PacketBuilder.CreateDrawingUpdatePacket(drawing);
                user.LoggedinClient.SendPacket(patchDrawing);

            }
        }
        public static void UpdateHorseMenu(GameClient forClient, HorseInstance horseInst)
        {
            int TileID = Map.GetTileId(forClient.LoggedinUser.X, forClient.LoggedinUser.Y, false);
            string type = Map.TerrainTiles[TileID - 1].Type;
    
            if(horseInst.Owner == forClient.LoggedinUser.Id)
                forClient.LoggedinUser.LastViewedHorse = horseInst;

            forClient.LoggedinUser.MetaPriority = true;
            byte[] metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildHorseInformation(horseInst, forClient.LoggedinUser));
            forClient.SendPacket(metaPacket);

            string loadSwf = HorseInfo.BreedViewerSwf(horseInst, type);
            byte[] swfPacket = PacketBuilder.CreateSwfModulePacket(loadSwf, PacketBuilder.PACKET_SWF_MODULE_FORCE);
            forClient.SendPacket(swfPacket);
        }
        public static void UpdateInventory(GameClient forClient)
        {
            if (!forClient.LoggedIn)
                return;
            forClient.LoggedinUser.MetaPriority = true;
            byte[] metaPacket = PacketBuilder.CreateMetaPacket(Meta.BuildInventoryInfo(forClient.LoggedinUser.Inventory));
            forClient.SendPacket(metaPacket);
        }

        public static void UpdateWeather(GameClient forClient)
        {
            if (!forClient.LoggedIn)
            {
                Logger.ErrorPrint(forClient.RemoteIp + "tried to update weather information when not logged in.");
                return;
            }

            string lastWeather = forClient.LoggedinUser.LastSeenWeather;
            string weather = forClient.LoggedinUser.GetWeatherSeen();
            if (lastWeather != weather)
            {
                byte[] WeatherUpdate = PacketBuilder.CreateWeatherUpdatePacket(weather);
                forClient.SendPacket(WeatherUpdate);
            }
        }
        public static void UpdateWorld(GameClient forClient)
        {
            if (!forClient.LoggedIn)
            {
                Logger.ErrorPrint(forClient.RemoteIp + "tried to update world information when not logged in.");
                return;
            }

            byte[] WorldData = PacketBuilder.CreateWorldData(World.ServerTime.Minutes, World.ServerTime.Days, World.ServerTime.Years, forClient.LoggedinUser.GetWeatherSeen());
            forClient.SendPacket(WorldData);
        }
        public static void UpdatePlayer(GameClient forClient)
        {
            if (!forClient.LoggedIn)
            {
                Logger.ErrorPrint(forClient.RemoteIp + "tried to update player information when not logged in.");
                return;
            }
            byte[] PlayerData = PacketBuilder.CreatePlayerData(forClient.LoggedinUser.Money, GameServer.GetNumberOfPlayers(), forClient.LoggedinUser.MailBox.MailCount);
            forClient.SendPacket(PlayerData);
        }
        public static void UpdateUserInfo(User user)
        {
            byte[] playerInfoBytes = PacketBuilder.CreatePlayerInfoUpdateOrCreate(user.X, user.Y, user.Facing, user.CharacterId, user.Username);



            List<User> users = new List<User>();
            foreach (GameClient client in ConnectedClients)
                if (client.LoggedIn)
                {
                    if (client.LoggedinUser.Id != user.Id)
                        client.SendPacket(playerInfoBytes);
                }


        }
        public static void UpdateAreaForAll(int x, int y, bool ignoreMetaPrio=false)
        {
            foreach(GameClient client in ConnectedClients)
            {
                if (client.LoggedIn)
                    if (client.LoggedinUser.X == x && client.LoggedinUser.Y == y)
                        if(!client.LoggedinUser.MetaPriority || ignoreMetaPrio)
                            UpdateArea(client);
            }
        }
        
        public static void UpdateArea(GameClient forClient)
        {
            if (!forClient.LoggedIn)
            {
                Logger.ErrorPrint(forClient.RemoteIp + "tried to update tile information when not logged in.");
                return;
            }

            string LocationStr = "";
            if (!World.InSpecialTile(forClient.LoggedinUser.X, forClient.LoggedinUser.Y))
            {
                LocationStr = Meta.BuildMetaInfo(forClient.LoggedinUser, forClient.LoggedinUser.X, forClient.LoggedinUser.Y);
            }
            else
            {
                World.SpecialTile specialTile = World.GetSpecialTile(forClient.LoggedinUser.X, forClient.LoggedinUser.Y);
                if (specialTile.AutoplaySwf != null && specialTile.AutoplaySwf != "")
                {
                    byte[] swfModulePacket = PacketBuilder.CreateSwfModulePacket(specialTile.AutoplaySwf,PacketBuilder.PACKET_SWF_MODULE_GENTLE);
                    forClient.SendPacket(swfModulePacket);
                }
                if (specialTile.Code != null)
                    if (!ProcessMapCodeWithArg(forClient, specialTile))
                        return;
                LocationStr = Meta.BuildSpecialTileInfo(forClient.LoggedinUser, specialTile);
            }

            byte[] AreaMessage = PacketBuilder.CreateMetaPacket(LocationStr);
            forClient.SendPacket(AreaMessage);
            forClient.LoggedinUser.MetaPriority = false;

        }
        public static void UpdateStats(GameClient client)
        {
            if (!client.LoggedIn)
                return;

            client.LoggedinUser.MetaPriority = true;
            string metaWind = Meta.BuildStatsMenu(client.LoggedinUser);
            byte[] statsPacket = PacketBuilder.CreateMetaPacket(metaWind);
            client.SendPacket(statsPacket);

        }

        /*
         *   Other...
         */
        public static bool ProcessMapCodeWithArg(GameClient forClient, World.SpecialTile tile)
        {
            string mapCode = tile.Code;
            if(mapCode.Contains('-'))
            {
                string[] codeInfo = mapCode.Split('-');
                string command = codeInfo[0];
                string paramaters = codeInfo[1];

                if(command == "JUMP")
                {
                    if(paramaters.Contains(','))
                    {
                        string[] args = paramaters.Split(',');
                        try
                        {
                            int newX = int.Parse(args[0]);
                            int newY = int.Parse(args[1]);
                            forClient.LoggedinUser.Teleport(newX, newY);
                            if (World.InIsle(tile.X, tile.Y))
                            {
                                World.Isle isle = World.GetIsle(tile.X, tile.Y);
                                int tileset = isle.Tileset;
                                int overlay = Map.GetTileId(tile.X, tile.Y, true);
                                if (tileset == 6 && overlay == 249) // warp point
                                {
                                    byte[] swfPacket = PacketBuilder.CreateSwfModulePacket("warpcutscene", PacketBuilder.PACKET_SWF_CUTSCENE);
                                    forClient.SendPacket(swfPacket);
                                }
                            }
                            return false;
                        }
                        catch(Exception)
                        {
                            return true;
                        }
                    }
                }
            }
            return true;
        }
        public static void StartServer()
        {
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress hostIP = IPAddress.Parse(ConfigReader.BindIP);
            IPEndPoint ep = new IPEndPoint(hostIP, ConfigReader.Port);
            ServerSocket.Bind(ep);
            Logger.InfoPrint("Binding to ip: " + ConfigReader.BindIP + " On port: " + ConfigReader.Port.ToString());
            ServerSocket.Listen(10000);
            gameTimer = new Timer(new TimerCallback(onGameTick), null, gameTickSpeed, gameTickSpeed);
            minuteTimer = new Timer(new TimerCallback(onMinuteTick), null, oneMinute, oneMinute);
            while (true)
            {
                Logger.InfoPrint("Waiting for new connections...");

                Socket cientSocket = ServerSocket.Accept();
                GameClient client = new GameClient(cientSocket);
                connectedClients.Add(client);
            }
        }

        
        

    }
}
