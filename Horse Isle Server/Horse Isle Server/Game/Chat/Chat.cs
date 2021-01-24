using System;
using System.Collections.Generic;
using System.Linq;
using HISP.Player;
using HISP.Server;

namespace HISP.Game.Chat
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
            Here = 0x18,
            Isle = 0x1A,
            Dm = 0x16,
            Mod = 0x1c,
            Admin  = 0x1b
        }
        public static string PrivateMessageSound;

        public static List<Filter> FilteredWords = new List<Filter>();
        public static List<Correction> CorrectedWords = new List<Correction>();
        public static List<Reason> Reasons = new List<Reason>();
        public static bool ProcessCommand(User user, string message)
        {
            if (message.Length < 1)
                return false;

            string[] args = message.Split(' ').Skip(1).ToArray();

            if (user.Administrator || user.Moderator)
                if (message[0] == '%')
                {
                    if(message.StartsWith("%STICKBUG"))
                        return Command.Stickbug(message, args, user);
                    if (message.StartsWith("%GIVE"))
                        return Command.Give(message, args, user);
                    if (message.StartsWith("%GOTO"))
                        return Command.Goto(message, args, user);
                    if (message.StartsWith("%KICK"))
                        return Command.Kick(message, args, user);
                    if (message.StartsWith("%NOCLIP"))
                        return Command.NoClip(message, args, user);
                    return false;
                }
            if (message[0] == '!')
            {
                // Alias for !MUTE
                if (message.StartsWith("!MUTEALL"))
                    return Command.Mute(message, new string[] { "ALL" }, user);
                else if (message.StartsWith("!MUTEADS"))
                    return Command.Mute(message, new string[] { "ADS" }, user);
                else if (message.StartsWith("!MUTEGLOBAL"))
                    return Command.Mute(message, new string[] { "GLOBAL" }, user);
                else if (message.StartsWith("!MUTEISLAND"))
                    return Command.Mute(message, new string[] { "ISLAND" }, user);
                else if (message.StartsWith("!MUTENEAR"))
                    return Command.Mute(message, new string[] { "NEAR" }, user);
                else if (message.StartsWith("!MUTEHERE"))
                    return Command.Mute(message, new string[] { "HERE" }, user);
                else if (message.StartsWith("!MUTEBUDDY"))
                    return Command.Mute(message, new string[] { "BUDDY" }, user);
                else if (message.StartsWith("!MUTEPM"))
                    return Command.Mute(message, new string[] { "PM" }, user);
                else if (message.StartsWith("!MUTEPM"))
                    return Command.Mute(message, new string[] { "PM" }, user);
                else if (message.StartsWith("!MUTEBR"))
                    return Command.Mute(message, new string[] { "BR" }, user);
                else if (message.StartsWith("!MUTESOCIALS"))
                    return Command.Mute(message, new string[] { "SOCIALS" }, user);
                else if (message.StartsWith("!MUTELOGINS"))
                    return Command.Mute(message, new string[] { "LOGINS" }, user);

                else if (message.StartsWith("!MUTE"))
                    return Command.Mute(message, args, user);
            }
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
                if (!filter.MatchAll)
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
                case ChatChannel.Here:
                case ChatChannel.Admin:
                case ChatChannel.Mod:
                    return PacketBuilder.CHAT_BOTTOM_RIGHT;
                case ChatChannel.Dm:
                    return PacketBuilder.CHAT_DM_RIGHT;
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
                return recipiantName;
            }    
            else
            {
                return null;
            }
        }

        public static string GetDmMessage(string message)
        {
            if (message.Contains('|'))
            {
                string messageStr = message.Split('|')[1];
                return messageStr;
            }
            else
            {
                return message;
            }
        }

        public static GameClient[] GetRecipiants(User user, ChatChannel channel, string to=null)
        {
            if (channel == ChatChannel.All)
            {
                List<GameClient> recipiants = new List<GameClient>();
                foreach (GameClient client in GameServer.ConnectedClients)
                {
                    if (client.LoggedIn)
                        if (!client.LoggedinUser.MuteGlobal && !client.LoggedinUser.MuteAll)
                            if (client.LoggedinUser.Id != user.Id)
                                recipiants.Add(client);
                }
                return recipiants.ToArray();
            }
            
            if(channel == ChatChannel.Ads)
            { 
                List<GameClient> recipiants = new List<GameClient>();
                foreach (GameClient client in GameServer.ConnectedClients)
                {
                    if (client.LoggedIn)
                        if (!client.LoggedinUser.MuteAds && !client.LoggedinUser.MuteAll)
                            if (client.LoggedinUser.Id != user.Id)
                                recipiants.Add(client);
                }
                return recipiants.ToArray();
            }

            if(channel == ChatChannel.Buddies)
            {
                List<GameClient> recipiants = new List<GameClient>();
                foreach (GameClient client in GameServer.ConnectedClients)
                {
                    if (client.LoggedIn)
                        if (!client.LoggedinUser.MuteBuddy && !client.LoggedinUser.MuteAll)
                            if (client.LoggedinUser.Id != user.Id)
                                if (client.LoggedinUser.Friends.List.Contains(user.Id)) 
                                    recipiants.Add(client);
                }
                return recipiants.ToArray();
            }

            if (channel == ChatChannel.Isle)
            {
                List<GameClient> recipiants = new List<GameClient>();
                if(World.InIsle(user.X,user.Y))
                {
                    User[] usersInSile = GameServer.GetUsersUsersInIsle(World.GetIsle(user.X, user.Y), true, false);
                    foreach (User userInIsle in usersInSile)
                    {
                        if (user.Id != userInIsle.Id)
                            if(!user.MuteAll)
                                recipiants.Add(userInIsle.LoggedinClient);
                    }
                    return recipiants.ToArray();
                }
                else
                {
                    return new GameClient[0];
                }

            }

            if (channel == ChatChannel.Here)
            {
                List<GameClient> recipiants = new List<GameClient>();
                User[] usersHere = GameServer.GetUsersAt(user.X, user.Y, true, false);
                foreach (User userHere in usersHere)
                {
                    if (user.Id != userHere.Id)
                        if (!user.MuteAll)
                            recipiants.Add(userHere.LoggedinClient);
                }
                return recipiants.ToArray();
            }

            if (channel == ChatChannel.Near)
            {
                List<GameClient> recipiants = new List<GameClient>();
                User[] nearbyUsers = GameServer.GetNearbyUsers(user.X, user.Y, true, false);
                foreach (User nearbyUser in nearbyUsers)
                {
                    if (user.Id != nearbyUser.Id)
                        if (!user.MuteAll)
                            recipiants.Add(nearbyUser.LoggedinClient);
                }
                return recipiants.ToArray();
            }

            if (channel == ChatChannel.Mod)
            {
                if (!user.Moderator || !user.Administrator) // No mod chat for non-mods!
                {
                    Logger.WarnPrint(user.Username + " attempted to send in MOD chat, without being a MOD.");
                    return new GameClient[0];
                }

                List<GameClient> recipiants = new List<GameClient>();
                foreach (GameClient client in GameServer.ConnectedClients)
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
                    return new GameClient[0];
                }
                    

                List<GameClient> recipiants = new List<GameClient>();
                foreach (GameClient client in GameServer.ConnectedClients)
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
                    List<GameClient> recipiants = new List<GameClient>();
                    foreach (GameClient client in GameServer.ConnectedClients)
                    {
                        if (client.LoggedIn)
                            if (!client.LoggedinUser.MutePrivateMessage && !client.LoggedinUser.MuteAll)
                                if (client.LoggedinUser.Username == to)
                                    recipiants.Add(client);
                    }
                    return recipiants.ToArray();
                }
                else
                {
                    Logger.ErrorPrint("Channel is " + channel + " (DM) BUT no 'to' Paramater was specfied");
                    return new GameClient[0];
                }
            }


            Logger.ErrorPrint(user.Username + " Sent message in unknown channel: " + (byte)channel);
            return new GameClient[0]; // No recipiants
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
                case ChatChannel.Near:
                    return Messages.FormatNearbyChatMessage(user.Username, message);
                case ChatChannel.Isle:
                    return Messages.FormatIsleChatMessage(user.Username, message);
                case ChatChannel.Here:
                    return Messages.FormatHereChatMessage(user.Username, message);
                case ChatChannel.Mod:
                    if (user.Moderator || user.Administrator)
                        return Messages.FormatModChatMessage(user.Username, message);
                    else
                    {
                        Logger.HackerPrint(user.Username + " Tried to send in mod chat without being a moderator. (Hack/Code Attempt)");
                        return user.Username + " is a hacker! (Sent in mod channel without being a mod) Maybe ban?";
                    }
                case ChatChannel.Admin:
                    if (user.Administrator)
                        return Messages.FormatAdminChatMessage(user.Username, message);
                    else
                    {
                        Logger.HackerPrint(user.Username + " Tried to send in mod chat without being a moderator. (Hack/Code Attempt)");
                        return user.Username + " is a hacker! (Sent in admin channel without being a admin) Maybe ban?";
                    }
                default:
                    Logger.ErrorPrint(user.Username + " is trying to end a message in unknown channel " + channel.ToString("X"));
                    return "not implemented yet :(";
            }
        }
        public static string FormatChatForSender(User user, ChatChannel channel, string message, string dmRecipiant=null)
        {
            switch (channel)
            {
                case ChatChannel.All:
                    if (user.Moderator || user.Administrator)
                        return Messages.FormatGlobalChatMessageForMod(user.Username, message);
                    else
                        return Messages.FormatGlobalChatMessage(user.Username, message);
                case ChatChannel.Ads:
                    int numbListening = GameServer.GetNumberOfPlayersListeningToAdsChat(); // vry specific function ik
                    return Messages.FormatAdsChatForSender(numbListening-1, user.Username, message);
                case ChatChannel.Buddies:
                    return Messages.FormatBuddyChatMessageForSender(user.Friends.Count, user.Username, message);
                case ChatChannel.Isle:
                    int inIsle = 0;
                    if (World.InIsle(user.X, user.Y))
                        inIsle = GameServer.GetUsersUsersInIsle(World.GetIsle(user.X, user.Y), false, false).Length -1;
                    return Messages.FormatIsleChatMessageForSender(inIsle, user.Username, message);
                case ChatChannel.Here:
                    int usersHere = GameServer.GetUsersAt(user.X, user.Y, false, false).Length -1;
                    return Messages.FormatHereChatMessageForSender(usersHere, user.Username, message);
                case ChatChannel.Near:
                    int nearbyUsers = GameServer.GetNearbyUsers(user.X, user.Y, false, false).Length -1;
                    return Messages.FormatNearChatMessageForSender(nearbyUsers, user.Username, message);
                case ChatChannel.Mod:
                    int modsOnline = GameServer.GetNumberOfModsOnline() - 1;
                    return Messages.FormatModChatForSender(modsOnline, user.Username, message);
                case ChatChannel.Admin:
                    int adminsOnline = GameServer.GetNumberOfAdminsOnline() - 1;
                    return Messages.FormatAdminChatForSender(adminsOnline, user.Username, message);
                case ChatChannel.Dm:
                    return Messages.FormatDirectChatMessageForSender(user.Username, dmRecipiant, message);
                default:
                    Logger.ErrorPrint(user.Username + " is trying to end a message in unknown channel " + channel.ToString("X"));
                    return "not implemented yet :(";
            }
        }

        public static string NonViolationChecks(User user, string message)
        {

            // Check if contains password.
            if (message.ToLower().Contains(user.Password.ToLower()))
                return Messages.PasswordNotice;
 
            // Check if ALL CAPS
            string[] wordsSaid;
            if (message.Contains(' ')) 
                wordsSaid = message.Split(' ');
            else
                wordsSaid = new string[] { message };

            foreach (string word in wordsSaid)
            {
                string lettersOnly = "";
                foreach(char c in word)
                {
                    if((byte)c >= (byte)'A' && (byte)c <= (byte)'z') // is letter
                    {
                        lettersOnly += c;
                    }
                }
                if (lettersOnly.ToUpper() == lettersOnly && lettersOnly.Length >= 5)
                    return Messages.CapsNotice;
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
