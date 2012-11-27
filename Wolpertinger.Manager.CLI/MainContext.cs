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
    public class MainContext : Context
    {

        public MainContext()
        {
            this.Name = "";

            commands.Add("new-session", new CommandInfo() { ParameterCount = 1, CommandMethod = newSessionCommand });
            commands.Add("remove-session", new CommandInfo() { ParameterCount = 1, CommandMethod = removeSessionCommand });
            commands.Add("get-sessions", new CommandInfo() { ParameterCount = 0, CommandMethod = getSessionsCommand });
            commands.Add("enter-session", new CommandInfo() { ParameterCount = 1, CommandMethod = enterSessionCommand });
        }



        protected void newSessionCommand(IEnumerable<string> cmds)
        {
            string target = cmds.First();


            if (Program.sessions.ContainsKey(target))
            {
                Program.ErrorLine(this, "Session already exists");
            }
            else
            {
                Session s = new Session(target, Program.connectionManager);
                Program.sessions.Add(target.ToLower(), s);
                Program.activeContext = s;
            }
        }

        protected void removeSessionCommand(IEnumerable<string> cmds)
        {

            Session s = Program.getSession(cmds.First());

            if (s == null)
            {
                Program.UnknownCommand(this);
            }
            else
            {
                s.disconnectCommand(new List<string>());

                Program.sessions.Remove(s.Target.ToLower());
            }
        }

        protected void getSessionsCommand(IEnumerable<string> cmds)
        {
            int column1Width = Math.Max(Program.sessions.Count.ToString().Length, "Index".Length);
            int column2Width = Math.Max((Program.sessions.Keys.Any() ? Program.sessions.Keys.Max(x => x.Length) : 0), "Target".Length);


            string formatString = " {0, -" + column1Width + "} | {1,-" + column2Width + "}";
            string hl = " " + new String('-', column1Width + column2Width + 3);

            Program.OutputLine(this, hl);
            Program.OutputLine(this, formatString, "Index", "Target");
            Program.OutputLine(this, hl);


            int index = 0;
            foreach (string target in Program.sessions.Keys)
            {
                Program.OutputLine(this, formatString, index, target);
                index++;
            }
            Program.OutputLine(this, hl);
        }

        protected void enterSessionCommand(IEnumerable<string> cmds)
        {
            Session s = Program.getSession(cmds.First());

            if (s != null)
                Program.activeContext = s;
            else
                Program.ErrorLine(this, "Session not found");
        }


        protected override bool checkConnection()
        {
            return true;
        }
    }
}
