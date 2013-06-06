using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.Manager.CLI.Commands.Connection
{
    [Command(CommandVerb.Exit, "Connection", "Connection")]
    class ExitConnectionCommand : CommandBase
    {

        public override void Execute()
        {
            if (Context.ActiveConnection == null)
            {
                Context.WriteError("No connection active");
            }
            else
            {
                Context.ActiveConnection = null;
            }
        }
    }
}
