using HISP.Player;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace HISP.Game.Chat
{
    public class CommandRegister
    {
        private static List<CommandRegister> registeredComamnds = new List<CommandRegister>();
        public static CommandRegister[] RegisteredCommands
        {
            get
            {
                return registeredComamnds.ToArray();
            }
        }

        public char CmdLetter;
        public string CmdName;
        public string CmdUsage;
        public Func<string,string[],User, bool> CmdCallback;
        public CommandRegister(char cmdLetter, string cmdName, string cmdUsage, Func<string, string[], User, bool> cmdCallback)
        {
            CmdLetter = cmdLetter;
            CmdName = cmdName.ToUpper(CultureInfo.InvariantCulture);
            CmdCallback = cmdCallback;
            CmdUsage = cmdUsage;

            registeredComamnds.Add(this);
        }
    }
}
