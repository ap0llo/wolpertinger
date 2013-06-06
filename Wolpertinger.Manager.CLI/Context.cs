/*

Licensed under the new BSD-License

Copyright (c) 2011-2013, Andreas Grünwald 
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

    Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
    Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
    Neither the name of the Wolpertinger project nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.Manager.CLI
{
    public abstract class Context
    {

        protected Dictionary<string, CommandInfo> commands = new Dictionary<string, CommandInfo>();
        protected static Dictionary<string, string> aliases = new Dictionary<string, string>();

        public string Name { get; set; }


        public Context()
        {
            //commands.Add("clear-screen", new CommandInfo() { CheckConnection = false, ParameterCount = 0, CommandMethod = clearScreenCommand });
            //commands.Add("set-alias", new CommandInfo() { CheckConnection = false, ParameterCount = 2, CommandMethod = setAliasCommand });
            //commands.Add("remove-alias", new CommandInfo() { CheckConnection = false, ParameterCount = 1, CommandMethod = removeAliasCommand });

        }



        public void ParseCommands(IEnumerable<string> cmds)
        {

            //string name = cmds.First().ToLower();

            //if (aliases.ContainsKey(name))
            //    name = aliases[name.ToLower()].ToLower();

            //if (commands.ContainsKey(name) && (
            //        (commands[name].ParameterCount == cmds.Count() - 1)
            //    || (commands[name].ParameterCount < 0 && cmds.Count() >= Math.Abs(commands[name].ParameterCount))))
            //{
            //    if ((commands[name].CheckConnection && checkConnection()) || !commands[name].CheckConnection)
            //    {
            //        commands[name].CommandMethod.Invoke(cmds.Skip(1));
            //        return;
            //    }
            //}

            //Program.UnknownCommand(this);

        }


        protected abstract bool checkConnection();

        //protected static Core.LogLevel? getLogLevel(string str)
        //{
        //    Core.LogLevel lvl;

        //    if (Enum.TryParse<Core.LogLevel>(str, true, out lvl))
        //        return lvl;
        //    else
        //        return null;
        //}

        protected static bool? getBool(string str)
        {
            bool value;

            if (bool.TryParse(str, out value))
                return value;
            else
                return null;
        }










        protected virtual void clearScreenCommand(IEnumerable<string> cmds)
        {
            Console.Clear();
        }

        protected virtual void setAliasCommand(IEnumerable<string> cmds)
        {
            //if (!aliases.ContainsKey(cmds.First().ToLower()))
            //    aliases.Add(cmds.First().ToLower(), cmds.Last());
            //else
            //    Program.ErrorLine(this, "Alias already exists");

        }

        protected virtual void removeAliasCommand(IEnumerable<string> cmds)
        {
            //if (aliases.ContainsKey(cmds.First().ToLower()))
            //{
            //    aliases.Remove(cmds.First().ToLower());
            //}
            //else
            //{
            //    Program.ErrorLine(this, "Alias not found");
            //}
        }
    }


    //public class CommandInfo
    //{
    //    public Action<IEnumerable<string>> CommandMethod;
    //    public int ParameterCount;
    //    public bool CheckConnection = true;
    //}
}
