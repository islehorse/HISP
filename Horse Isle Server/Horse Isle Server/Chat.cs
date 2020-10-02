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
            Dm = 0x16,
            Mod = 0x1c,
            Admin  = 0x1b
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
                        if (word.ToLower() == filter.FilteredWord.ToLower())
                            return filter.Reason;
                    }
                }
                else
                {
                    if (message.ToLower().Contains(filter.FilteredWord))
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
                case ChatChannel.Buddies:
                case ChatChannel.Admin:
                case ChatChannel.Mod:
                    return PacketBuilder.CHAT_BOTTOM_RIGHT;
                case ChatChannel.Dm:
                    return PacketBuilder.CHAT_BTMR_W_DM_SFX;
                default:
                    Logger.ErrorPrint("unknown channel: " + (byte)channel);
                    return PacketBuilder.CHAT_BOTTOM_LEFT;
            }

        }

        public static string GetDmRecipiant(string message)
        {
            if(message.Contains('|'))
            {
                string recipiantName = message.Split('|')[0];
                return recipiantName.Substring(2);
            }    
            else
            {
                return null;
            }
        }

        public static Client[] GetRecipiants(User user, ChatChannel channel, string to=null)
        {
            if (channel == ChatChannel.All)
            {
                List<Client> recipiants = new List<Client>();
                foreach (Client client in Server.ConnectedClients)
                {
                    if (client.LoggedIn)
                        if (!client.LoggedinUser.MuteGlobal)
                            if (client.LoggedinUser.Id != user.Id)
                                recipiants.Add(client);
                }
                return recipiants.ToArray();
            }
            
            if(channel == ChatChannel.Ads)
            { 
                List<Client> recipiants = new List<Client>();
                foreach (Client client in Server.ConnectedClients)
                {
                    if (client.LoggedIn)
                        if (!client.LoggedinUser.MuteAds)
                            if (client.LoggedinUser.Id != user.Id)
                                recipiants.Add(client);
                }
                return recipiants.ToArray();
            }

            if(channel == ChatChannel.Buddies)
            {
                List<Client> recipiants = new List<Client>();
                foreach (Client client in Server.ConnectedClients)
                {
                    if (client.LoggedIn)
                        if (!client.LoggedinUser.MuteBuddy)
                            if (client.LoggedinUser.Id != user.Id)
                                if (client.LoggedinUser.Friends.List.Contains(user.Id)) 
                                    recipiants.Add(client);
                }
                return recipiants.ToArray();
            }

            if (channel == ChatChannel.Mod)
            {
                if (!user.Moderator || !user.Administrator) // No mod chat for non-mods!
                {
                    Logger.WarnPrint(user.Username + " attempted to send in MOD chat, without being a MOD.");
                    return new Client[0];
                }

                List<Client> recipiants = new List<Client>();
                foreach (Client client in Server.ConnectedClients)
                {
                    if (client.LoggedIn)
                        if (client.LoggedinUser.Moderator)
                            if (client.LoggedinUser.Id != user.Id)
                                recipiants.Add(client);
                }
                return recipiants.ToArray();
            }

            if(channel == ChatChannel.Admin)
            {
                if (!user.Administrator) // No admin chat for non-admins!
                {
                    Logger.WarnPrint(user.Username + " attempted to send in ADMIN chat, without being an ADMIN.");
                    return new Client[0];
                }
                    

                List<Client> recipiants = new List<Client>();
                foreach (Client client in Server.ConnectedClients)
                {
                    if (client.LoggedIn)
                        if (client.LoggedinUser.Administrator)
                            if (client.LoggedinUser.Id != user.Id)
                                recipiants.Add(client);
                }
                return recipiants.ToArray();
            }

            if(channel == ChatChannel.Dm)
            {
                if (to != null)
                {
                    List<Client> recipiants = new List<Client>();
                    foreach (Client client in Server.ConnectedClients)
                    {
                        if (client.LoggedIn)
                            if (!client.LoggedinUser.MutePrivateMessage)
                                if (client.LoggedinUser.Username != to)
                                    recipiants.Add(client);
                    }
                    return recipiants.ToArray();
                }
                else
                {
                    Logger.ErrorPrint("Channel is " + channel + " (DM) BUT no 'to' Paramater was specfied");
                    return new Client[0];
                }
            }


            Logger.ErrorPrint(user.Username + " Sent message in unknown channel: " + (byte)channel);
            return new Client[0]; // No recipiants
        }

        public static string DoCorrections(string message)
        {
            if (!ConfigReader.DoCorrections)
                return message;

            foreach(Correction correct in CorrectedWords)
                message = message.Replace(correct.FilteredWord, correct.ReplacedWord);

            return message;
        }
        public static string EscapeMessage(string message)
        {
            return message.Replace("<", "&lt;");
        }

        public static string FormatChatForOthers(User user, ChatChannel channel, string message)
        {

            switch (channel)
            {
                case ChatChannel.All:
                    if (user.Moderator || user.Administrator)
                        return Messages.FormatGlobalChatMessageForMod(user.Username, message);
                    else
                        return Messages.FormatGlobalChatMessage(user.Username, message);
                case ChatChannel.Ads:
                    return Messages.FormatAdsChatMessage(user.Username, message);
                case ChatChannel.Buddies:
                    return Messages.FormatBuddyChatMessage(user.Username, message);
                case ChatChannel.Dm:
                    if (user.Moderator || user.Administrator)
                        return Messages.FormatDirectMessageForMod(user.Username, message);
                    else
                        return Messages.FormatDirectMessage(user.Username, message);
                case ChatChannel.Mod:
                    if (user.Moderator || user.Administrator)
                        return Messages.FormatModChatMessage(user.Username, message);
                    else
                        return "Hacker!";
                case ChatChannel.Admin:
                    if (user.Administrator)
                        return Messages.FormatAdminChatMessage(user.Username, message);
                    else
                        return "Hacker!";
                default:
                    Logger.ErrorPrint(user.Username + " is trying to end a message in unknown channel " + channel.ToString("X"));
                    return "not implemented yet :(";
            }
        }
        public static string FormatChatForSender(User user, ChatChannel channel, string message)
        {
            switch (channel)
            {
                case ChatChannel.All:
                    if (user.Moderator || user.Administrator)
                        return Messages.FormatGlobalChatMessageForMod(user.Username, message);
                    else
                        return Messages.FormatGlobalChatMessage(user.Username, message);
                case ChatChannel.Ads:
                    return Messages.FormatAdsChatMessage(user.Username, message);
                case ChatChannel.Buddies:
                    return Messages.FormatBuddyChatMessageForSender(user.Friends.Count, user.Username, message);
                case ChatChannel.Mod:
                    return Messages.FormatModChatForSender(Server.GetNumberOfModsOnline(), user.Username, message);
                case ChatChannel.Admin:
                    return Messages.FormatAdminChatForSender(Server.GetNumberOfAdminsOnline(),user.Username, message);
                default:
                    Logger.ErrorPrint(user.Username + " is trying to end a message in unknown channel " + channel.ToString("X"));
                    return "not implemented yet :(";
            }
        }

        public static string NonViolationChecks(User user, string message)
        {

            // Check if contains password.
            if (message.ToLower().Contains(user.Password.ToLower()))
            {
                return Messages.PasswordNotice;
            }


            // Check if ALL CAPS
            if (message.Contains(' ')) // hi1 apparently doesnt care about caps if its all 1 word?
            {
                string[] wordsSaid = message.Split(' ');
                foreach (string word in wordsSaid)
                {
                    if (word.ToUpper() == word)
                    {
                        return Messages.CapsNotice;
                    }
                }
            }

            return null;
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
