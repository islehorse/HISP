using System;
using System.Collections.Generic;
using System.Linq;

namespace Horse_Isle_Server
{
    class Chat
    {
        public struct Correction
        {
            public string FilteredWord;
            public string ReplacedWord;
        }

        public struct Reason
        {
            public string Name;
            public string Message;
        }
        public struct Filter
        {
            public string FilteredWord;
            public Reason Reason;
            public bool MatchAll;
        }


        public enum ChatChannel
        {
            All = 0x14,
            Ads = 0x1D,
            Near = 0x15,
            Buddies = 0x17,
            Isle = 0x24,
            Mod,
            Admin
        }

        public static List<Filter> FilteredWords = new List<Filter>();
        public static List<Correction> CorrectedWords = new List<Correction>();
        public static List<Reason> Reasons = new List<Reason>();
        public static bool ProcessCommand(User user, string message)
        {
            if (message.Length < 1)
                return false;

            if (user.Administrator || user.Moderator)
                if (message[0] == '%')
                    return true;
            if (message[0] == '!')
                return true;
            return false;
 
        }
        public static Object FilterMessage(string message) // Handles chat filtering and violation stuffs returns
        {
            if (!ConfigReader.BadWords) // Freedom of Speech Mode
                return null;


            string[] wordsSaid;
            if (message.Contains(' '))
                wordsSaid = message.Split(' ');
            else
                wordsSaid = new string[] { message };


            foreach(Filter filter in FilteredWords)
            {
                if (filter.MatchAll)
                {
                    foreach (string word in wordsSaid)
                    {
                        if (word == filter.FilteredWord)
                            return filter.Reason;
                    }
                }
                else
                {
                    if (message.Contains(filter.FilteredWord))
                        return filter.Reason;
                }
            }

            return null;
        }

        public static byte GetSide(ChatChannel channel)
        {
            switch (channel)
            {
                case ChatChannel.All:
                case ChatChannel.Ads:
                case ChatChannel.Isle:
                    return PacketBuilder.CHAT_BOTTOM_LEFT;
                case ChatChannel.Admin:
                case ChatChannel.Mod:
                case ChatChannel.Buddies:
                    return PacketBuilder.CHAT_BOTTOM_RIGHT;
                default:
                    Logger.ErrorPrint("unknown channel: " + (byte)channel);
                    return PacketBuilder.CHAT_BOTTOM_LEFT;
            }

        }
        public static Client[] GetRecipiants(User user, ChatChannel channel)
        {
            if(channel == ChatChannel.All)
            {
                List<Client> recipiants = new List<Client>();
                foreach (Client client in Server.ConnectedClients)
                    if (client.LoggedIn)
                        if (!client.LoggedinUser.MuteGlobal)
                            recipiants.Add(client);
                return recipiants.ToArray();
            }

            Logger.ErrorPrint(user.Username + " Sent message in unknown channel: " + (byte)channel);
            return new Client[0]; // No recipiants
        }
        public static Reason GetReason(string name)
        {
            foreach (Reason reason in Reasons)
                if (reason.Name == name)
                    return reason;

            throw new KeyNotFoundException("Reason " + name + " not found.");
        }

    }
}
