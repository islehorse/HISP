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
        public static string BuddyChatFormat;
        public static string DirectChatFormat;
        public static string ModChatFormat;
        public static string AdminChatFormat;

        public static string GlobalChatFormatForModerators;
        public static string DirectChatFormatForModerators;

        public static string HereChatFormatForSender;
        public static string IsleChatFormatForSender;
        public static string NearChatFormatForSender;
        public static string BuddyChatFormatForSender;
        public static string DirectChatFormatForSender;
        public static string AdminChatFormatForSender;
        public static string ModChatFormatForSender;

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

        // For all
        public static string FormatGlobalChatMessage(string username, string message)
        {
            return GlobalChatFormat.Replace("%USERNAME%", username).Replace("%MESSAGE%", message);
        }

        public static string FormatBuddyChatMessage(string username, string message)
        {
            return BuddyChatFormat.Replace("%USERNAME%", username).Replace("%MESSAGE%", message);
        }

        public static string FormatDirectMessage(string username, string message)
        {
            return DirectChatFormat.Replace("%USERNAME%", username).Replace("%MESSAGE%", message);
        }
        public static string FormatDirectMessageForMod(string username, string message)
        {
            return DirectChatFormatForModerators.Replace("%USERNAME%", username).Replace("%MESSAGE%", message);
        }
        
        public static string FormatGlobalChatMessageForMod(string username, string message)
        {
            return GlobalChatFormatForModerators.Replace("%USERNAME%", username).Replace("%MESSAGE%", message);
        }

        public static string FormatAdsChatMessage(string username, string message)
        {
            return AdsChatFormat.Replace("%USERNAME%", username).Replace("%MESSAGE%", message);
        }

        public static string FormatModChatMessage(string username, string message)
        {
            return ModChatFormat.Replace("%USERNAME%", username).Replace("%MESSAGE%", message);
        }

        public static string FormatAdminChatMessage(string username, string message)
        {
            return AdminChatFormat.Replace("%USERNAME%", username).Replace("%MESSAGE%", message);
        }


        // For Sender
        public static string FormatBuddyChatMessageForSender(int numbBuddies, string username, string message)
        {
            return BuddyChatFormatForSender.Replace("%USERNAME%", username).Replace("%MESSAGE%", message).Replace("%AMOUNT%", numbBuddies.ToString());
        }

        public static string FormatAdminChatForSender(int numbAdmins, string username, string message)
        {
            return AdminChatFormatForSender.Replace("%USERNAME%", username).Replace("%MESSAGE%", message).Replace("%AMOUNT%", numbAdmins.ToString());
        }

        public static string FormatModChatForSender(int numbMods, string username, string message)
        {
            return ModChatFormatForSender.Replace("%USERNAME%", username).Replace("%MESSAGE%", message).Replace("%AMOUNT%", numbMods.ToString());
        }
        public static string FormatDirectChatMessageForSender(string username,string toUsername, string message)
        {
            return DirectChatFormatForSender.Replace("%FROMUSER%", username).Replace("%TOUSER%", toUsername).Replace(" %MESSAGE%", message);
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
