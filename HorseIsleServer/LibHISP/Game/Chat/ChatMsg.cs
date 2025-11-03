using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using HISP.Player;
using HISP.Server;

namespace HISP.Game.Chat
{
    public class ChatMsg
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

        private static List<Filter> filteredWords = new List<Filter>();
        private static List<Correction> correctedWords = new List<Correction>();
        private static List<Reason> reasons = new List<Reason>();

        public static void AddFilter(Filter filter)
        {
            filteredWords.Add(filter);
        }
        public static void AddCorrection(Correction correction)
        {
            correctedWords.Add(correction);
        }
        public static void AddReason(Reason reason)
        {
            reasons.Add(reason);
        }
        public static Filter[] FilteredWords
        {
            get
            {
                return filteredWords.ToArray();
            }
        }
        public static Correction[] CorrectedWords
        {
            get
            {
                return correctedWords.ToArray();
            }
        }
        public static Reason[] Reasons
        {
            get
            {
                return reasons.ToArray();
            }
        }

        public static bool ProcessCommand(User user, string message)
        {
            if (message.Length < 1)
                return false;

            string parsedMessage = message;

            parsedMessage = parsedMessage.Trim();
            char cLetter = parsedMessage[0];
            parsedMessage = parsedMessage.Substring(1).Trim();

            string messageToGive = parsedMessage;


            foreach (CommandRegister cmd in CommandRegister.RegisteredCommands)
            {
                if(cmd.CmdLetter == cLetter)
                {
                    if (parsedMessage.ToUpper(CultureInfo.InvariantCulture).StartsWith(cmd.CmdName))
                    {
                        string[] args = parsedMessage.Substring(cmd.CmdName.Length).Trim().Split(' ');
                        return cmd.Execute(messageToGive, args, user);
                    }
                }
            }
            return false;
        }
        public static Object FilterMessage(string message) // Handles chat filtering and violation stuffs
        {
            if (!ConfigReader.EnableSwearFilter) // Freedom of Speech Mode
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
                //recipiants.AddRange(User.OnlineUsers.Where(o => (o.MuteGlobal && !o.MuteAll) && 
                //                                                (o.Id != user.Id) && 
                //                                                (o.MutePlayer.IsUserMuted(user)))
                //    .Select(o => o.Client));

                foreach (User onlineUser in User.OnlineUsers)
                {
                    if (!onlineUser.MuteGlobal && !onlineUser.MuteAll)
                        if (onlineUser.Id != user.Id)
                            if(!onlineUser.MutePlayer.IsUserMuted(user))
                                recipiants.Add(user.Client);
                }
                return recipiants.ToArray();
            }
            
            if(channel == ChatChannel.Ads)
            { 
                List<GameClient> recipiants = new List<GameClient>();
                foreach (User onlineUser in User.OnlineUsers)
                {
                    if (!onlineUser.MuteAds && !onlineUser.MuteAll)
                        if (onlineUser.Id != user.Id)
                            if (!onlineUser.MutePlayer.IsUserMuted(user))
                                recipiants.Add(onlineUser.Client);
                }
                return recipiants.ToArray();
            }

            if(channel == ChatChannel.Buddies)
            {
                List<GameClient> recipiants = new List<GameClient>();
                foreach (User onlineUser in User.OnlineUsers)
                {
                    if (!onlineUser.MuteBuddy && !onlineUser.MuteAll)
                        if (onlineUser.Id != user.Id)
                            if (onlineUser.Friends.List.Contains(user.Id))
                                if (!onlineUser.MutePlayer.IsUserMuted(user))
                                    recipiants.Add(onlineUser.Client);
                }
                return recipiants.ToArray();
            }

            if (channel == ChatChannel.Isle)
            {
                List<GameClient> recipiants = new List<GameClient>();
                if(World.InIsle(user.X,user.Y))
                {
                    User[] usersInIsle = User.GetUsersInIsle(World.GetIsle(user.X, user.Y), true, false);
                    foreach (User userInIsle in usersInIsle)
                    {
                        if (user.Id != userInIsle.Id)
                            if(!userInIsle.MuteAll && !userInIsle.MuteIsland)
                                if(!userInIsle.MutePlayer.IsUserMuted(user))
                                    recipiants.Add(userInIsle.Client);
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
                User[] usersHere = User.GetUsersAt(user.X, user.Y, true, false);
                foreach (User userHere in usersHere)
                {
                    if (user.Id != userHere.Id)
                        if (!userHere.MuteAll && !userHere.MuteHere)
                            if (!userHere.MutePlayer.IsUserMuted(user))
                                recipiants.Add(userHere.Client);
                }
                return recipiants.ToArray();
            }

            if (channel == ChatChannel.Near)
            {
                List<GameClient> recipiants = new List<GameClient>();
                User[] nearbyUsers = User.GetNearbyUsers(user.X, user.Y, true, false);
                foreach (User nearbyUser in nearbyUsers)
                {
                    if (user.Id != nearbyUser.Id)
                        if (!nearbyUser.MuteAll && !nearbyUser.MuteNear)
                            if (!nearbyUser.MutePlayer.IsUserMuted(user))
                                recipiants.Add(nearbyUser.Client);
                }
                return recipiants.ToArray();
            }

            if (channel == ChatChannel.Mod)
            {
                if (!user.Moderator && !user.Administrator) // No mod chat for non-mods!
                {
                    Logger.WarnPrint(user.Username + " attempted to send in MOD chat, without being a MOD.");
                    return new GameClient[0];
                }

                List<GameClient> recipiants = new List<GameClient>();
                foreach (GameClient client in GameClient.ConnectedClients)
                {
                    if (client.LoggedIn)
                        if (client.User.Moderator)
                            if (client.User.Id != user.Id)
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
                foreach (GameClient client in GameClient.ConnectedClients)
                {
                    if (client.LoggedIn)
                        if (client.User.Administrator)
                            if (client.User.Id != user.Id)
                                recipiants.Add(client);
                }
                return recipiants.ToArray();
            }

            if(channel == ChatChannel.Dm)
            {
                if (to != null && to != "")
                {
                    List<GameClient> recipiants = new List<GameClient>();
                    foreach (GameClient client in GameClient.ConnectedClients)
                    {
                        if (client.LoggedIn)
                        {
                            if (!client.User.MutePrivateMessage && !client.User.MuteAll)
                            {
                                if (client.User.Username.ToLower().StartsWith(to.ToLower()))
                                {
                                    recipiants.Add(client);
                                    break;
                                }

                            }
                        }
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
            if (!ConfigReader.EnableCorrections)
                return message;

            foreach(Correction correct in CorrectedWords)
                message = message.Replace(correct.FilteredWord, correct.ReplacedWord);

            return message.Trim();
        }
        public static string EscapeMessage(string message)
        {
            return message.Replace("<", "&lt;");
        }

        public static string FormatChatForOthers(User user, ChatChannel channel, string message, bool autoReply=false)
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
                    string badge = "";
                    if (user.Moderator || user.Administrator)
                        badge += Messages.DmModBadge;
                    if (autoReply)
                        badge += Messages.DmAutoResponse;
                    return Messages.FormatDirectMessage(user.Username, message, badge);
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
                        return "";
                    }
                case ChatChannel.Admin:
                    if (user.Administrator)
                        return Messages.FormatAdminChatMessage(user.Username, message);
                    else
                    {
                        Logger.HackerPrint(user.Username + " Tried to send in mod chat without being a moderator. (Hack/Code Attempt)");
                        return "";
                    }
                default:
                    Logger.ErrorPrint(user.Username + " is trying to end a message in unknown channel " + channel.ToString("X"));
                    return "This channel is either invalid or is not implemented yet :(";
            }
        }
        public static string FormatChatForSender(User user, ChatChannel channel, string message, string dmRecipiant=null, bool autoReply=false)
        {
            switch (channel)
            {
                case ChatChannel.All:
                    if (user.Moderator || user.Administrator)
                        return Messages.FormatGlobalChatMessageForMod(user.Username, message);
                    else
                        return Messages.FormatGlobalChatMessage(user.Username, message);
                case ChatChannel.Ads:
                    int numbListening = User.AdsListening;
                    return Messages.FormatAdsChatForSender(numbListening-1, user.Username, message);
                case ChatChannel.Buddies:
                    return Messages.FormatBuddyChatMessageForSender(user.GetNumberOfBuddiesOnline(), user.Username, message);
                case ChatChannel.Isle:
                    int inIsle = 0;
                    if (World.InIsle(user.X, user.Y))
                        inIsle = User.GetUsersInIsle(World.GetIsle(user.X, user.Y), false, false).Length -1;
                    return Messages.FormatIsleChatMessageForSender(inIsle, user.Username, message);
                case ChatChannel.Here:
                    int usersHere = User.GetUsersAt(user.X, user.Y, false, false).Length -1;
                    return Messages.FormatHereChatMessageForSender(usersHere, user.Username, message);
                case ChatChannel.Near:
                    int nearbyUsers = User.GetNearbyUsers(user.X, user.Y, false, false).Length -1;
                    return Messages.FormatNearChatMessageForSender(nearbyUsers, user.Username, message);
                case ChatChannel.Mod:
                    int modsOnline = User.ModsOnline - 1;
                    return Messages.FormatModChatForSender(modsOnline, user.Username, message);
                case ChatChannel.Admin:
                    int adminsOnline = User.AdminsOnline - 1;
                    return Messages.FormatAdminChatForSender(adminsOnline, user.Username, message);
                case ChatChannel.Dm:
                    string badge = "";
                    if (user.Moderator || user.Administrator)
                        badge += Messages.DmModBadge;
                    if (autoReply)
                        badge += Messages.DmAutoResponse;
                    return Messages.FormatDirectChatMessageForSender(user.Username, dmRecipiant, message, badge);
                default:
                    Logger.ErrorPrint(user.Username + " is trying to end a message in unknown channel " + channel.ToString("X"));
                    return "This channel is either invalid or is not implemented yet :(";
            }
        }

        public static string NonViolationChecks(User user, string message)
        {
            if(!ConfigReader.EnableNonViolations)
                return null;

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
                {
                    return Messages.CapsNotice;
                }
            }

            return null;
        }
        public static Reason GetReason(string name)
        {
            return Reasons.First(o => o.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

    }
}
