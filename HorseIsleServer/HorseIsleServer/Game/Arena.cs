using HISP.Game.Horse;
using HISP.Player;
using HISP.Security;
using HISP.Server;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace HISP.Game
{
    public class Arena
    {

        private static List<Arena> arenas = new List<Arena>();
        private List<ArenaEntry> entries;
        private Timer arenaTimeout;

        public int Id;
        public string Type;
        public int EntryCost;
        public int RandomId;
        public int RaceEvery;
        public int Slots;
        public string Mode;
        public int Timeout;
        public static Arena[] Arenas
        {
            get
            {
                return arenas.ToArray();
            }
        }

        public ArenaEntry[] Entries
        {
            get
            {
                return entries.ToArray();
            }
        }

        public class ArenaEntry
        {
            public User EnteredUser;
            public HorseInstance EnteredHorse;
            public int SubmitScore = 0;
            public bool Done = false;
        }
        public Arena(int id, string type, int entryCost, int raceEvery, int slots, int timeOut)
        {
            RandomId = RandomID.NextRandomId();
            Mode = "TAKINGENTRIES";
            Id = id;
            Type = type;
            EntryCost = entryCost;
            RaceEvery = raceEvery;
            Slots = slots;
            Timeout = timeOut;
            arenas.Add(this);
            entries = new List<ArenaEntry>();
        }

        public bool HaveAllPlayersCompleted()
        {
            int playersCompleted = 0;
            foreach(ArenaEntry entry in Entries)
            {
                if (entry.Done)
                    playersCompleted++;
            }
            if (playersCompleted >= Entries.Length)
                return true;
            else
                return false;
        }
        public void SubmitScore(User user, int score)
        {
            foreach(ArenaEntry entry in Entries)
            {
                if(entry.EnteredUser.Id == user.Id)
                {
                    entry.SubmitScore = score;
                    entry.Done = true;
                    break;
                }
            }

            if (HaveAllPlayersCompleted())
                End();
        }
        private string getSwf(ArenaEntry entry)
        {
            HorseInfo.StatCalculator speedCalculator = new HorseInfo.StatCalculator(entry.EnteredHorse, HorseInfo.StatType.SPEED, entry.EnteredUser);
            HorseInfo.StatCalculator strengthCalculator = new HorseInfo.StatCalculator(entry.EnteredHorse, HorseInfo.StatType.STRENGTH, entry.EnteredUser);
            HorseInfo.StatCalculator enduranceCalculator = new HorseInfo.StatCalculator(entry.EnteredHorse, HorseInfo.StatType.ENDURANCE, entry.EnteredUser);
            HorseInfo.StatCalculator conformationCalculator = new HorseInfo.StatCalculator(entry.EnteredHorse, HorseInfo.StatType.CONFORMATION, entry.EnteredUser);

            switch (Type)
            {
                case "BEGINNERJUMPING":
                    int bigJumps = Convert.ToInt32(Math.Round((double)strengthCalculator.Total / 100.0))+1;
                    return "jumpingarena1.swf?BIGJUMPS="+bigJumps+"&SPEEEDMAX="+speedCalculator.Total+"&JUNK=";
                case "JUMPING":
                    int bonus = entry.EnteredHorse.BasicStats.Health + entry.EnteredHorse.BasicStats.Hunger + entry.EnteredHorse.BasicStats.Thirst + entry.EnteredHorse.BasicStats.Shoes;
                    return "jumpingarena2.swf?BONUS=" + bonus + "&STRENGTH=" + strengthCalculator.Total + "&SPEED=" + speedCalculator.Total + "&ENDURANCE=" + enduranceCalculator.Total + "&JUNK=";
                case "CONFORMATION":
                    int baseScore = conformationCalculator.Total + ((entry.EnteredHorse.BasicStats.Groom > 750) ? 1000 : 500);
                    string swf = "dressagearena.swf?BASESCORE=" + baseScore;
                    int i = 1;
                    foreach (ArenaEntry ent in Entries.ToArray())
                    {
                        swf += "&HN" + i.ToString() + "=" + ent.EnteredUser.Username;
                        if (ent.EnteredUser.Id == entry.EnteredUser.Id)
                            swf += "&POS=" + i.ToString();
                        i++;
                    }
                    swf += "&JUNK=";
                    return swf;
                case "DRAFT":
                    int draftAbility = Convert.ToInt32(Math.Round((((double)entry.EnteredHorse.BasicStats.Health * 2.0 + (double)entry.EnteredHorse.BasicStats.Shoes * 2.0) + (double)entry.EnteredHorse.BasicStats.Hunger + (double)entry.EnteredHorse.BasicStats.Thirst) / 6.0 + (double)strengthCalculator.Total + ((double)enduranceCalculator.Total / 2.0)));
                    swf = "draftarena.swf?DRAFTABILITY=" + draftAbility;
                    i = 1;
                    foreach (ArenaEntry ent in Entries.ToArray())
                    {
                        swf += "&HN" + i.ToString() + "=" + ent.EnteredUser.Username;
                        if (ent.EnteredUser.Id == entry.EnteredUser.Id)
                            swf += "&POS=" + i.ToString();
                        i++;
                    }
                    swf += "&J=";
                    return swf;
                case "RACING":
                    int baseSpeed = Convert.ToInt32(Math.Round((((double)entry.EnteredHorse.BasicStats.Health * 2.0 + (double)entry.EnteredHorse.BasicStats.Shoes * 2.0) + (double)entry.EnteredHorse.BasicStats.Hunger + (double)entry.EnteredHorse.BasicStats.Thirst) / 6.0 + (double)speedCalculator.Total));
                    swf = "racingarena.swf?BASESPEED=" + baseSpeed + "&ENDURANCE=" + enduranceCalculator.Total;
                    i = 1;
                    foreach (ArenaEntry ent in Entries.ToArray())
                    {
                        swf += "&HN" + i.ToString() + "=" + ent.EnteredUser.Username;
                        if (ent.EnteredUser.Id == entry.EnteredUser.Id)
                            swf += "&POS=" + i.ToString();
                        i++;
                    }
                    swf += "&JUNK=";
                    return swf;
                default:
                    return "test.swf";
            }

        }
        public void Start()
        {
            Mode = "COMPETING";
            if (Entries.Length <= 0)
            {
                reset();
                return;
            }
            foreach(ArenaEntry entry in Entries.ToArray())
            {
                string swf = getSwf(entry);
                string message = "";
                switch (Type)
                {
                    case "RACING":
                        entry.EnteredHorse.BasicStats.Hunger -= 200;
                        entry.EnteredHorse.BasicStats.Thirst -= 200;
                        entry.EnteredHorse.BasicStats.Tiredness -= 200;
                        entry.EnteredHorse.BasicStats.Shoes -= 100;
                        message = Messages.ArenaRacingStartup;
                        break;
                    case "DRAFT":
                        entry.EnteredHorse.BasicStats.Hunger -= 200;
                        entry.EnteredHorse.BasicStats.Thirst -= 200;
                        entry.EnteredHorse.BasicStats.Tiredness -= 200;
                        entry.EnteredHorse.BasicStats.Shoes -= 100;
                        message = Messages.ArenaDraftStartup;
                        break;
                    case "BEGINNERJUMPING":
                        entry.EnteredHorse.BasicStats.Hunger -= 200;
                        entry.EnteredHorse.BasicStats.Thirst -= 200;
                        entry.EnteredHorse.BasicStats.Tiredness -= 200;
                        entry.EnteredHorse.BasicStats.Shoes -= 100;
                        message = Messages.ArenaJumpingStartup;
                        break;
                    case "JUMPING":
                        entry.EnteredHorse.BasicStats.Hunger -= 300;
                        entry.EnteredHorse.BasicStats.Thirst -= 300;
                        entry.EnteredHorse.BasicStats.Tiredness -= 300;
                        entry.EnteredHorse.BasicStats.Shoes -= 100;
                        message = Messages.ArenaJumpingStartup;
                        break;
                    case "DRESSAGE":
                        entry.EnteredHorse.BasicStats.Mood -= 300;
                        entry.EnteredHorse.BasicStats.Tiredness -= 200;
                        message = Messages.ArenaConformationStartup;
                        break;
                }
                byte[] startingUpEventPacket = PacketBuilder.CreateChat(message, PacketBuilder.CHAT_BOTTOM_RIGHT);
                byte[] swfModulePacket = PacketBuilder.CreateSwfModulePacket(swf, PacketBuilder.PACKET_SWF_CUTSCENE);
                Logger.DebugPrint(entry.EnteredUser.Username + " Loading swf: " + swf);
                entry.EnteredUser.LoggedinClient.SendPacket(swfModulePacket);
                entry.EnteredUser.LoggedinClient.SendPacket(startingUpEventPacket);
            }

            arenaTimeout = new Timer(new TimerCallback(arenaTimedOut), null, Timeout * 60 * 1000, Timeout * 60 * 1000);

            updateWaitingPlayers();

        }

        private void updateWaitingPlayers()
        {
            foreach (World.SpecialTile tile in World.SpecialTiles)
            {
                if (tile.Code == null)
                    continue;
                if (tile.Code.StartsWith("ARENA-"))
                {
                    string arenaId = tile.Code.Split('-')[1];
                    int id = int.Parse(arenaId);
                    if (id == this.Id)
                        GameServer.UpdateAreaForAll(tile.X, tile.Y, true);
                }
            }
        }

        private void arenaTimedOut(object state)
        {
            End();
        }

        private void reset()
        {
            // Delete all entries
            entries.Clear();
            RandomId = RandomID.NextRandomId();
            Mode = "TAKINGENTRIES";
            if (arenaTimeout != null)
                arenaTimeout.Dispose();
            arenaTimeout = null;
        }
        public void End()
        {

            if(Mode == "COMPETING")
            {
                string chatMessage = Messages.ArenaResultsMessage;
                
                string[] avaliblePlacings = new string[6] { Messages.ArenaFirstPlace, Messages.ArenaSecondPlace, Messages.ArenaThirdPlace, Messages.ArenaFourthPlace, Messages.ArenaFifthPlace, Messages.ArenaSixthPlace };
                
                int[] expRewards = new int[Entries.Length];
                expRewards[0] = 1;
                int expAwardMul = 1;
                for(int i = 1; i < Entries.Length; i++)
                {
                    expRewards[i] = 2 * expAwardMul;

                    if (expAwardMul == 1)
                        expAwardMul = 2;
                    else
                        expAwardMul += 2;
                }

                expRewards = expRewards.ToArray().Reverse().ToArray();

                int place = 0;
                ArenaEntry[] winners = Entries.OrderByDescending(o => o.SubmitScore).ToArray();
                foreach (ArenaEntry entry in winners)
                {
                    string placing = avaliblePlacings[place % avaliblePlacings.Length];

                    chatMessage += Messages.FormatArenaPlacing(placing, entry.EnteredUser.Username, entry.SubmitScore);
                    place++;
                }
                place = 0;
                foreach(ArenaEntry entry in winners)
                {
                    try
                    {
                        byte[] arenaResults = PacketBuilder.CreateChat(chatMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
                        entry.EnteredUser.LoggedinClient.SendPacket(arenaResults);

                        int expReward = expRewards[place];

                        entry.EnteredHorse.BasicStats.Experience += expReward;
                        entry.EnteredUser.Experience += expReward;

                        if (place == 0) // WINNER!
                        {
                            int prize = EntryCost * Entries.Length;
                            entry.EnteredUser.AddMoney(prize);


                            byte[] youWinMessage = PacketBuilder.CreateChat(Messages.FormatArenaYouWinMessage(prize, expReward), PacketBuilder.CHAT_BOTTOM_RIGHT);
                            entry.EnteredUser.LoggedinClient.SendPacket(youWinMessage);

                            // Awards:

                            if (Entries.Length >= 2 && Type == "JUMPING")
                                entry.EnteredUser.Awards.AddAward(Award.GetAwardById(5)); // Good Jumper

                            if (Entries.Length >= 4 && Type == "JUMPING")
                                entry.EnteredUser.Awards.AddAward(Award.GetAwardById(6)); // Great Jumper


                            if (Entries.Length >= 2 && Type == "RACING")
                                entry.EnteredUser.Awards.AddAward(Award.GetAwardById(7)); // Good Racer

                            if (Entries.Length >= 4 && Type == "RACING")
                                entry.EnteredUser.Awards.AddAward(Award.GetAwardById(8)); // Great Racer


                            if (Entries.Length >= 2 && Type == "DRESSAGE")
                                entry.EnteredUser.Awards.AddAward(Award.GetAwardById(9)); // Good Dressage

                            if (Entries.Length >= 4 && Type == "DRESSAGE")
                                entry.EnteredUser.Awards.AddAward(Award.GetAwardById(10)); // Great Dressage

                            if (Entries.Length >= 2 && Type == "DRAFT")
                                entry.EnteredUser.Awards.AddAward(Award.GetAwardById(38)); // Strong Horse Award

                            if (Entries.Length >= 4 && Type == "DRAFT")
                                entry.EnteredUser.Awards.AddAward(Award.GetAwardById(39)); // Strongest Horse Award
                        }
                        else
                        {
                            entry.EnteredUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.ArenaLoss).Count++;

                            if(entry.EnteredUser.TrackedItems.GetTrackedItem(Tracking.TrackableItem.ArenaLoss).Count >= 100)
                                entry.EnteredUser.Awards.AddAward(Award.GetAwardById(32)); // Perseverance

                            byte[] youDONTWinMessage = PacketBuilder.CreateChat(Messages.FormatArenaOnlyWinnerWinsMessage(expReward), PacketBuilder.CHAT_BOTTOM_RIGHT);
                            entry.EnteredUser.LoggedinClient.SendPacket(youDONTWinMessage);
                        }
                        place++;
                    }
                    catch(Exception)
                    {
                        continue;
                    }

                }

            }
            reset();
            updateWaitingPlayers();
        }
        public void DeleteEntry(User user)
        {
            if (Mode == "COMPETING")
                return;

            foreach(ArenaEntry entry in Entries)
                if(entry.EnteredUser.Id == user.Id)
                {
                    entries.Remove(entry);
                    break;
                }
        }
        public void AddEntry(User user, HorseInstance horse)
        {
            if(!UserHasHorseEntered(user))
            {
                ArenaEntry arenaEntry = new ArenaEntry();
                arenaEntry.EnteredUser = user;
                arenaEntry.EnteredHorse = horse;
                entries.Add(arenaEntry);
            }
        }

        public static Arena GetArenaUserEnteredIn(User user)
        {
            foreach (Arena arena in Arenas)
                if (arena.UserHasHorseEntered(user))
                    return arena;
            throw new KeyNotFoundException("user was not entered in any arena.");
        }
        public bool UserHasHorseEntered(User user)
        {
            foreach (ArenaEntry entry in Entries.ToArray())
                if (entry.EnteredUser.Id == user.Id)
                    return true;
            return false;
        }

        public static void StartArenas(int minutes)
        {
            foreach(Arena arena in Arenas)
            {
                if (minutes % arena.RaceEvery == 0)
                    if(arena.Mode == "TAKINGENTRIES")
                       arena.Start();
            }

        }
        public static bool UserHasEnteredHorseInAnyArena(User user)
        {
            foreach (Arena arena in Arenas)
                if (arena.UserHasHorseEntered(user))
                    return true;
            return false;
        }
        public static Arena GetAreaById(int id)
        {
            foreach (Arena arena in Arenas)
                if (arena.Id == id)
                    return arena;
            throw new KeyNotFoundException("Arena with id " + id + " NOT FOUND!");
        }
    }
}
