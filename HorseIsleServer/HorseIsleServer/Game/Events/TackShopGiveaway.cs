using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using HISP.Game.Services;
using HISP.Server;
using HISP.Game.Horse;
using HISP.Player;

namespace HISP.Game.Events
{
    public class TackShopGiveaway
    {
        public string ShopName;
        public World.SpecialTile Location;
        public HorseInstance HorseGiveaway;
        public World.Town Town;
        public bool Active = false;
        public const int TACKSHOP_TIMEOUT = 1;
        private Timer giveAwayTimer;
        private int timesTicked = 0;

        private void giveawayTick(object state)
        {
            timesTicked++;
            if (timesTicked >= 2)
            {
                EndEvent();
                return;
            }
            if (timesTicked >= 1)
            {
                byte[] giveAwayMessage = PacketBuilder.CreateChat(Messages.FormatEventTackShopGiveaway1Min(HorseGiveaway.Color, HorseGiveaway.Breed.Name, HorseGiveaway.Gender, ShopName, Town.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                foreach (GameClient client in GameClient.ConnectedClients)
                    if (client.LoggedIn)
                        client.SendPacket(giveAwayMessage);
            }
            giveAwayTimer.Change(TACKSHOP_TIMEOUT * 60 * 1000, TACKSHOP_TIMEOUT * 60 * 1000);
        }

        public TackShopGiveaway()
        {
            List<World.SpecialTile> specialTiles = new List<World.SpecialTile>();

            foreach (World.SpecialTile sTile in World.SpecialTiles)
            {
                if (sTile.Code != null)
                {
                    if (sTile.Code.StartsWith("STORE-"))
                    {

                        int storeId = int.Parse(sTile.Code.Split("-")[1]);
                        Shop shopData = Shop.GetShopById(storeId);

                        if (shopData.BuysItemTypes.Contains("TACK"))
                        {
                            Npc.NpcEntry[] npcShop = Npc.GetNpcByXAndY(sTile.X, sTile.Y);
                            if (npcShop.Length > 0)
                            {
                                specialTiles.Add(sTile);
                            }
                        }
                    }
                }
            }

            string npcName = "ERROR";
            string npcDesc = "OBTAINING NAME";

            int shpIdx = GameServer.RandomNumberGenerator.Next(0, specialTiles.Count);
            Location = specialTiles[shpIdx];
            Npc.NpcEntry[] npcShops = Npc.GetNpcByXAndY(Location.X, Location.Y);

            npcName = npcShops[0].Name.Split(" ")[0];
            if (npcShops[0].ShortDescription.ToLower().Contains("tack"))
            {
                npcDesc = npcShops[0].ShortDescription.Substring(npcShops[0].ShortDescription.ToLower().IndexOf("tack"));
                ShopName = npcName + "'s " + npcDesc;
            }
            else
            {
                ShopName = npcName + "'s Gear";
            }

 
            while(true)
            {
                int hrsIdx = GameServer.RandomNumberGenerator.Next(0, HorseInfo.Breeds.Length);
                HorseInfo.Breed breed = HorseInfo.Breeds[hrsIdx];
                if (breed.SpawnInArea == "none")
                    continue;

                HorseGiveaway = new HorseInstance(breed);
                HorseGiveaway.Name = "Tack Shop Giveaway";
                break;
            }

            if (World.InTown(Location.X, Location.Y))
                Town = World.GetTown(Location.X, Location.Y);
        }

        public void StartEvent()
        {
            giveAwayTimer = new Timer(new TimerCallback(giveawayTick), null, TACKSHOP_TIMEOUT * 60 * 1000, TACKSHOP_TIMEOUT * 60 * 1000);

            byte[] giveAwayMessage = PacketBuilder.CreateChat(Messages.FormatEventTackShopGiveawayStart(HorseGiveaway.Color, HorseGiveaway.Breed.Name, HorseGiveaway.Gender, ShopName, Town.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
            foreach (GameClient client in GameClient.ConnectedClients)
                if (client.LoggedIn)
                    client.SendPacket(giveAwayMessage);

            Active = true;

            GameServer.TackShopGiveawayEvent = this;
        }

        public void EndEvent()
        {
            giveAwayTimer.Dispose();

            Active = false;
            GameServer.TackShopGiveawayEvent = null;

            User[] usersHere = GameServer.GetUsersAt(Location.X, Location.Y, false, true);

            if(usersHere.Length > 0)
            {
                int winIndx = GameServer.RandomNumberGenerator.Next(0, usersHere.Length);
                User winner = usersHere[winIndx];

                winner.HorseInventory.AddHorse(HorseGiveaway);
                winner.TrackedItems.GetTrackedItem(Tracking.TrackableItem.TackShopGiveaway).Count++;

                byte[] horseWonMessage = PacketBuilder.CreateChat(Messages.FormatEventTackShopGiveawayWon(winner.Username, HorseGiveaway.Breed.Name, ShopName, Town.Name, usersHere.Length), PacketBuilder.CHAT_BOTTOM_RIGHT);
                foreach (GameClient client in GameClient.ConnectedClients)
                    if (client.LoggedIn)
                        client.SendPacket(horseWonMessage);

            }
            else
            {
                byte[] eventEndedMessage = PacketBuilder.CreateChat(Messages.FormatEventTackShopGiveawayEnd(ShopName, Town.Name), PacketBuilder.CHAT_BOTTOM_RIGHT);
                foreach (GameClient client in GameClient.ConnectedClients)
                    if (client.LoggedIn)
                        client.SendPacket(eventEndedMessage);
            }
        }






    }
}
