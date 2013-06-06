using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.Manager.CLI.Commands.Connection
{
    [Command(CommandVerb.Close, "Connection", "Connection")]
    class CloseConnectionCommand : CommandBase
    {

        public override void Execute()
        {
            if (Context.ActiveConnection == null)
            {
                Context.WriteError("No connection active");
            }
            else
            {
                Context.ActiveConnection.ResetConnection(true);

                Context.ConnectionManager.RemoveClientConnection(Context.ActiveConnection.Target);

                Context.ActiveConnection = null;
            }


        }

    }
}
