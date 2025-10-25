using HISP.Player;
using HISP.Util;
using System;
using System.Globalization;

namespace HISP.Game.Chat
{
    public class CommandRegister
    {
        private static ThreadSafeList<CommandRegister> registeredComamnds = new ThreadSafeList<CommandRegister>();
        public static CommandRegister[] RegisteredCommands
        {
            get
            {
                return registeredComamnds.ToArray();
            }
        }
        private Func<string, string[], User, bool> commandCallback;

        public bool CmdRequiresAdmin;
        public bool CmdRequiresMod;

        public char CmdLetter;
        public string CmdName;
        public string CmdUsage;
        public CommandRegister(char cmdLetter, string cmdName, string cmdUsage, Func<string, string[], User, bool> cmdCallback, bool cmdRequiresAdmin, bool cmdRequiresMod)
        {
            this.CmdLetter = cmdLetter;
            this.CmdName = cmdName.ToUpper(CultureInfo.InvariantCulture).Trim();
            this.CmdUsage = cmdUsage;

            this.CmdRequiresMod = cmdRequiresMod;
            this.CmdRequiresAdmin = cmdRequiresAdmin;

            this.commandCallback = cmdCallback;

            registeredComamnds.Add(this);
        }

        public bool HasPermission(User user)
        {
            if (CmdRequiresAdmin && !(user.Administrator))
                return false;
            if (CmdRequiresMod && !(user.Moderator || user.Administrator))
                return false;

            return true;
        }

        public bool Execute(string message, string[] args, User user)
        {
            if(HasPermission(user))
                return commandCallback(message, args, user);
            return false;
        }
    }
}
