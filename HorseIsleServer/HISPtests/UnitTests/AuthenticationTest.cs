using HISP.Security;
using HISP.Util;
using System.Collections.Generic;

namespace HISP.Tests.UnitTests
{
    public class AuthenticationTest
    {

        private const string ALLOWED_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";
        public static bool Test(string testName, object value, object valueComp)
        {
            bool result = value.Equals(valueComp);
            if (result)
                ResultLogger.LogTestStatus(true, "AUTH_TEST " + testName, "Success.");
            else
                ResultLogger.LogTestResult(false, "AUTH_TEST " + testName, value.ToString(), valueComp.ToString());

            return result;
        }

        public static bool RunAuthenticationTest()
        {
            List<bool> results = new List<bool>();

            // Test Login Encrypt/Decrypt
            for(int i = 0; i <= 100; i++)
            {
                string rngStr = Helper.RandomString(ALLOWED_CHARS);
                string cipherText = Authentication.EncryptLogin(rngStr);
                string plainText = Authentication.DecryptLogin(cipherText);

                results.Add(Test("LoginEncryptDecrypt" + i.ToString(), plainText, rngStr));
            
            }


            string username = Helper.RandomString(ALLOWED_CHARS);
            string password = Helper.RandomString(ALLOWED_CHARS);
            
            string wrongPassword = Helper.RandomString(ALLOWED_CHARS);
            string wrongUsername = Helper.RandomString(ALLOWED_CHARS);

            Authentication.CreateAccount(username, password, "DEMIGIRL", true, true);

            // Test Login function
            results.Add(Test("CorrectUsernameAndPassword", Authentication.CheckPassword(username, password), true));
            results.Add(Test("CorrectUsernameWrongPassword", Authentication.CheckPassword(username, wrongPassword), false));
            results.Add(Test("WrongPasswordAndUsername", Authentication.CheckPassword(wrongUsername, wrongPassword), false));
            results.Add(Test("WrongUsernameCorrectPassword", Authentication.CheckPassword(wrongUsername, password), false));


            foreach (bool result in results)
                if (!result)
                    return false;
            return true;
        }
    }
}
