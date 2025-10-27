using HISP.Game;
using HISP.Game.Horse;
using HISP.Game.Items;
using HISP.Player;
using HISP.Security;
using HISP.Server;
using HISP.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HISP.Tests.UnitTests
{
    public class UserTest
    {
        private const string ALLOWED_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";

        private static byte[] createLoginPacket(string username, string password)
        {
            byte[] loginInformation = Encoding.UTF8.GetBytes("91|" + Authentication.EncryptLogin(username) + "|" + Authentication.EncryptLogin(password) + "|");
            byte[] loginPacket = new byte[1 * 3 + loginInformation.Length];
            loginPacket[0] = PacketBuilder.PACKET_LOGIN;
            loginPacket[loginPacket.Length - 2] = PacketBuilder.PACKET_CLIENT_TERMINATOR;
            loginPacket[loginPacket.Length - 1] = 0;

            Array.ConstrainedCopy(loginInformation, 0, loginPacket, 1, loginInformation.Length);

            return loginPacket;
        }

        private static byte[] createUserInfoPacket()
        {
            byte[] packet = new byte[] { PacketBuilder.PACKET_LOGIN, PacketBuilder.PACKET_CLIENT_TERMINATOR, 0};
            return packet;
        }
        public static bool Test(string testName, object value, object valueComp)
        {
            bool result = value.Equals(valueComp);
            if (result)
                ResultLogger.LogTestStatus(true, "USER_TEST " + testName, "Success.");
            else
                ResultLogger.LogTestResult(false, "USER_TEST " + testName, value.ToString(), valueComp.ToString());

            return result;
        }


        private static async Task<byte[]> receiveAsync(Socket s)
        {
            byte[] buffer = new byte[s.Available];
            await s.ReceiveAsync(buffer);
            return buffer;
        }

        public static async Task<bool> RunUserTest()
        {
            List<bool> results = new List<bool>();

            string username = Helper.RandomString(ALLOWED_CHARS);
            string password = Helper.RandomString(ALLOWED_CHARS);

            int userId = Authentication.CreateAccount(username, password, "GENDERFLUID", true, true);

            // Connect to running server via TCP;
            using (Socket hispServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                // This is already a pretty good unit test; confirming we can actually login to the game,

                try
                {
                    await hispServer.ConnectAsync("127.0.0.1", ConfigReader.Port);

                    // Login to the game;
                    await hispServer.SendAsync(createLoginPacket(username, password));
                    await receiveAsync(hispServer);

                    await hispServer.SendAsync(createUserInfoPacket());
                    await receiveAsync(hispServer);


                    ResultLogger.LogTestStatus(true, "GAME_LOGIN_TEST", "Success.");
                }
                catch (Exception e)
                {
                    ResultLogger.LogTestStatus(false, "GAME_LOGIN_TEST", e.Message);
                    return false;
                }



                // While were here; lets do a bunch of tests on HISPs "User" object.
                User user = User.GetUserById(userId);


                // Test Maximums

                user.SetMoney(Int32.MaxValue);
                user.AddMoney(1000);
                results.Add(Test("MoneyOverflow", user.Money, Int32.MaxValue));

                user.Hunger = 8000;
                results.Add(Test("HungerMaximumEnforcement", user.Hunger, 1000));
                user.Hunger = -8000;
                results.Add(Test("HungerMinimumEnforcement", user.Hunger, 0));

                user.Thirst = 8000;
                results.Add(Test("ThirstMaximumEnforcement", user.Thirst, 1000));
                user.Thirst = -8000;
                results.Add(Test("ThirstMinimumEnforcement", user.Thirst, 0));

                user.Tiredness = 8000;
                results.Add(Test("TirednessMaximumEnforcement", user.Tiredness, 1000));
                user.Tiredness = -8000;
                results.Add(Test("TirednessMinimumEnforcement", user.Tiredness, 0));


                user.BankMoney = 9999999999.9999;
                user.BankMoney += 1000.0;
                results.Add(Test("BankMoneyMaximumEnforcement", user.BankMoney, 9999999999.9999));

                user.BankInterest = 9999999999.9999;
                user.BankInterest += 1000.0;
                results.Add(Test("BankInterestMaximumEnforcement", user.BankInterest, 9999999999.9999));

                // Test Gender Setting
                user.Gender = "GENDERFLUID";
                results.Add(Test("GenderedPronounsTestTheir", user.GetPronouns(true), Messages.PronounNeutralTheir));
                results.Add(Test("GenderedPronounsTestThey", user.GetPronouns(false), Messages.PronounNeutralThey));

                user.Gender = "MALE";
                results.Add(Test("GenderedPronounsTestHis", user.GetPronouns(true), Messages.PronounMaleHis));
                results.Add(Test("GenderedPronounsTestHe", user.GetPronouns(false), Messages.PronounMaleHe));

                user.Gender = "FEMALE";
                results.Add(Test("GenderedPronounsTestHer", user.GetPronouns(true), Messages.PronounFemaleHer));
                results.Add(Test("GenderedPronounsTestShe", user.GetPronouns(false), Messages.PronounFemaleShe));

                // Test icon

                results.Add(Test("IconTestAdmin", user.GetPlayerListIcon(), Messages.AdminIcon));
                user.Administrator = false;
                results.Add(Test("IconTestModerator", user.GetPlayerListIcon(), Messages.ModeratorIcon));
                user.Moderator = false;
                results.Add(Test("IconTestNew", user.GetPlayerListIcon(), Messages.NewUserIcon));
                user.NewPlayer = false;
                results.Add(Test("IconTestNone", user.GetPlayerListIcon(), -1));

                user.SubscribedUntil = DateTime.UtcNow.AddMonths(1);
                user.Subscribed = true;
                results.Add(Test("IconTest1Month", user.GetPlayerListIcon(), Messages.MonthSubscriptionIcon));
                user.SubscribedUntil = DateTime.UtcNow.AddMonths(3);
                results.Add(Test("IconTest3Month", user.GetPlayerListIcon(), Messages.ThreeMonthSubscripitionIcon));
                user.SubscribedUntil = DateTime.UtcNow.AddYears(1).AddMonths(3);
                results.Add(Test("IconTest1Year", user.GetPlayerListIcon(), Messages.YearSubscriptionIcon));

                // SecCode
                user.SecCodeSeeds[0] = 0x34;
                user.SecCodeSeeds[1] = 0x39;
                user.SecCodeSeeds[2] = 0x2a;
                user.SecCodeInc = 0x3e;

                byte[] expectedSecCodeResult = { 0x55, 0x3C, 0x4B, 0x58 };
                byte[] gotSecCode = user.GenerateSecCode();

                results.Add(Test("GenerateSecCode", gotSecCode.SequenceEqual(expectedSecCodeResult), true));

                // Check max horses count
                user.Subscribed = false;
                results.Add(Test("UnsubbedMaxHorses", user.MaxHorses, 7));

                user.Subscribed = true;
                results.Add(Test("SubbedMaxHorses", user.MaxHorses, 11));


                HorseInstance horse = new HorseInstance(HorseInfo.GetBreedById(170));
                horse.BasicStats.Thirst = 0;
                horse.BasicStats.Hunger = 0;
                int horseId = horse.RandomId;

                user.HorseInventory.AddHorse(horse);

                /*
                 * Test Ranches
                 */

                // delete this ranch
                Ranch.GetRanchById(37).OwnerId = -1;

                // Give player ranch id 37
                Ranch.GetRanchById(37).OwnerId = user.Id;

                // Check ranch is now owned by that player, and propagates to user object.
                results.Add(Test("GiveRanchTest", user.OwnedRanch.Id, 37));
                results.Add(Test("HaveDorothyShoes", user.Inventory.HasItemId(Item.DorothyShoes), true));

                // Ranch upgrade test
                user.OwnedRanch.UpgradedLevel = Ranch.RanchUpgrade.RanchUpgrades.Max(o => o.Id);

                // Test swf
                results.Add(Test("RanchSwfMine", user.OwnedRanch.GetSwf(true), "ranchviewer.swf?H=10&B1=&B2=&B3=&B4=&B5=&B6=&B7=&B8=&B9=&B10=&B11=&B12=&B13=&B14=&B15=&B16=&MINE=1"));
                results.Add(Test("RanchSwf", user.OwnedRanch.GetSwf(false), "ranchviewer.swf?H=10&B1=&B2=&B3=&B4=&B5=&B6=&B7=&B8=&B9=&B10=&B11=&B12=&B13=&B14=&B15=&B16="));

                // Test Ranch Building Functionality

                // Test Barn
                user.OwnedRanch.SetBuilding(0, Ranch.RanchBuilding.GetRanchBuildingById(1)); // Barn
                results.Add(Test("RanchBarnMaxHorses", user.MaxHorses, 11 + 4));

                // Test Water Well
                user.OwnedRanch.SetBuilding(0, Ranch.RanchBuilding.GetRanchBuildingById(2)); // Water Well
                user.Teleport(user.OwnedRanch.X, user.OwnedRanch.Y);
                results.Add(Test("RanchWaterWellWatering", user.HorseInventory.GetHorseById(horseId).BasicStats.Thirst, 1000));
                
                // Test Grain Silo
                user.OwnedRanch.SetBuilding(0, Ranch.RanchBuilding.GetRanchBuildingById(3)); // Grain Silo
                user.Teleport(user.OwnedRanch.X, user.OwnedRanch.Y);
                results.Add(Test("RanchGrainSiloFeeding", user.HorseInventory.GetHorseById(horseId).BasicStats.Hunger, 1000));


                // Set building id 0 to a big barn
                user.OwnedRanch.SetBuilding(0, Ranch.RanchBuilding.GetRanchBuildingById(10)); // Big Barn
                results.Add(Test("RanchBigBarnMaxHorses", user.MaxHorses, 11 + 8));

                // Set building id 0 to a gold barn
                user.OwnedRanch.SetBuilding(0, Ranch.RanchBuilding.GetRanchBuildingById(11)); // Gold Barn
                results.Add(Test("RanchGoldBarnMaxHorses", user.MaxHorses, 11 + 12));




                foreach (bool result in results)
                    if (!result)
                        return false;
                
                return true;

            }


        }

    }
}

