using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Isle_Server
{
    class Messages
    {
        // Announcements
        public static string NewUserMessage;
        public static string LoginFormat;
        public static string MotdFormat;
        

        // Records
        public static string ProfileSavedMessage;

        // Chat
        public static string GlobalChatFormat;
        public static string AdsChatFormat;

        public static string GlobalChatFormatForModerators;

        public static string HereChatFormatForSender;
        public static string IsleChatFormatForSender;
        public static string NearChatFormatForSender;
        public static string BuddyChatFormatForSender;
        public static string DirectChatFormatForSender;

        public static string ChatViolationMessageFormat;
        public static int RequiredChatViolations;

        // Meta
        public static string IsleFormat;
        public static string TownFormat;
        public static string AreaFormat;
        public static string LocationFormat;

        public static string TileFormat;
        public static string NothingMessage;
        public static string Seperator;

        public static string FormatGlobalChatViolationMessage(Chat.Reason violationReason)
        {
            return ChatViolationMessageFormat.Replace("%AMOUNT%", RequiredChatViolations.ToString()).Replace("%REASON%", violationReason.Message);
        }
        public static string FormatGlobalChatMessage(User sender, string message)
        {
            if (sender.Moderator)
                return GlobalChatFormatForModerators.Replace("%USERNAME%", sender.Username).Replace("%MESSAGE%", message);
            else
                return GlobalChatFormat.Replace("%USERNAME%", sender.Username).Replace("%MESSAGE%", message);

        }
        public static string FormatMOTD()
        {
            return MotdFormat.Replace("%MOTD%", ConfigReader.Motd);
        }
        public static string FormatLoginMessage(string username)
        {
            return LoginFormat.Replace("%USERNAME%", username);
        }



        public static string FormatLocationData(int x, int y)
        {
            string locationString = "";
            string message = "";
            if(World.InArea(x,y))
                locationString += AreaFormat.Replace("%AREA%", World.GetArea(x, y).Name);
            if (World.InTown(x, y))
                locationString += TownFormat.Replace("%TOWN%", World.GetTown(x, y).Name);
            if (World.InIsle(x, y))
                locationString += IsleFormat.Replace("%ISLE%", World.GetIsle(x, y).Name);

            if(locationString != "")
                message += LocationFormat.Replace("%META%", locationString);

            int[] itemIds = World.GetDroppedItems(x, y);
            if (itemIds.Length == 0)
                message += NothingMessage;

            return message;
        }
    }
}
