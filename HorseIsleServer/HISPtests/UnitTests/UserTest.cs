using HISP.Game;
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
            loginPacket[loginPacket.Length - 1] = PacketBuilder.PACKET_TERMINATOR;

            Array.ConstrainedCopy(loginInformation, 0, loginPacket, 1, loginInformation.Length);

            return loginPacket;
        }

        private static byte[] createUserInfoPacket()
        {
            byte[] packet = new byte[] { PacketBuilder.PACKET_LOGIN, PacketBuilder.PACKET_CLIENT_TERMINATOR, PacketBuilder.PACKET_TERMINATOR};
            return packet;
        }

        public static bool Test(string testName, object value, object valueComp)
        {
            bool result = value.Equals(valueComp);
            if (result)
                ResultLogger.LogTestStatus(true, "USER_TEST "+testName, "Success.");
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
                User user = GameServer.GetUserById(userId);


                // Test Maximums

                user.SetMoney(Int32.MaxValue);
                user.AddMoney(1000);
                results.Add(Test("MoneyOverflow", user.Money, Int32.MaxValue));

                user.Hunger = 8000;
                results.Add(Test("HungerMaximumEnforcement", user.Hunger, 1000));
                user.Hunger = -8000;
                results.Add(Test("HungerMinimumEnforcement", user.Hunger, 0));

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


                foreach (bool result in results)
                    if (!result)
                        return false;
                
                return true;

            }


        }

    }
}

