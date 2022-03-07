﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace HISP.Server
{
    public class Util
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
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
            return dtDateTime;
        }

        public static void ByteArrayToByteList(byte[] byteArray, List<byte> byteList)
        {
            byteList.AddRange(byteArray.ToList());
        }
    }
}
