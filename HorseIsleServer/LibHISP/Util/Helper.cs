using HISP.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace HISP.Util
{
    public class Helper
    {
        // Thanks Stackoverflow (https://stackoverflow.com/questions/321370/how-can-i-convert-a-hex-string-to-a-byte-array)
        private static int getHexVal(char hex)
        {
            int val = (int)hex;
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }
        public static byte[] StringToByteArray(string hex)
        {
            if (hex.Length % 2 == 1)
                throw new ArgumentException("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((getHexVal(hex[i << 1]) << 4) + (getHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }

        public static double PointsToDistance(int x1, int y1, int x2, int y2)
        {
            return Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
        }
        public static string CapitalizeFirstLetter(string str)
        {
            char firstChar = char.ToUpper(str[0]);
            return firstChar + str.Substring(1);
        }
        public static int GetMonthsBetweenTwoDateTimes(DateTime from, DateTime to)
        {
            if (from > to) return GetMonthsBetweenTwoDateTimes(to, from);
            int monthDiff = Math.Abs((to.Year * 12 + (to.Month)) - (from.Year * 12 + (from.Month)));
            return monthDiff;

        }
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
            return dtDateTime;
        }

        public static bool ByteArrayStartsWith(byte[] byteArray, byte[] searchValue)
        {
            if (byteArray.Length < searchValue.Length) return false;

            byte[] buffer = new byte[searchValue.Length];
            Array.ConstrainedCopy(byteArray, 0, buffer, 0, searchValue.Length);

            return buffer.SequenceEqual(searchValue);
        }

        public static bool ByteArrayEndsWith(byte[] byteArray, byte[] searchValue)
        {
            if (searchValue.Length > byteArray.Length) return false;

            byte[] buffer = new byte[searchValue.Length];
            Array.ConstrainedCopy(byteArray, (byteArray.Length - searchValue.Length), buffer, 0, searchValue.Length);

            return buffer.SequenceEqual(searchValue);
        }

        public static void ByteArrayToByteList(byte[] byteArray, List<byte> byteList)
        {
            byteList.AddRange(byteArray.ToList());
        }

        public static string RandomString(string allowedCharacters)
        {
            int length = GameServer.RandomNumberGenerator.Next(7, 16);
            string str = "";
            for (int i = 0; i < length; i++)
                str += allowedCharacters[GameServer.RandomNumberGenerator.Next(0, allowedCharacters.Length - 1)];

            return str;
        }
        public static string ReverseString(string str)
        {
            char[] charArray = str.ToCharArray();
            Array.Reverse(charArray);
            string newStr = new string(charArray);

            return newStr;
        }

        public static string GetIp(EndPoint ep)
        {
            string endPointIp = ep.ToString();
            if (endPointIp.Contains(":"))
                endPointIp = endPointIp.Substring(0, endPointIp.IndexOf(":"));
            return endPointIp;
        }
    }
}
