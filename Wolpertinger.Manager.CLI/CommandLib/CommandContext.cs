using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wolpertinger.Core;

namespace Wolpertinger.Manager.CLI
{
    class CommandContext
    {

        public IConnectionManager ConnectionManager { get; set; }


        public IClientConnection ActiveConnection { get; set; }


        public CommandParser CommadParser { get; set; }


        public void AddClientConnection(IClientConnection connection)
        {
            
        }





        public void WriteError(string message)
        {
            Program.ErrorLine(message);
        }

        public void WriteError(string format, params object[] args)
        {
            WriteError(String.Format(format, args));
        }

        public void WriteInfo(string message)
        {
            Program.StatusLine(message);     
        }


        public void WriteOutput(string output)
        {
            Program.OutputLine(output);
        }

        public void WriteOutput()
        {
            WriteOutput("");
        }


        public void WriteOutput(string format, params object[] args)
        {
            WriteOutput(String.Format(format, args));
        }
    }
}
