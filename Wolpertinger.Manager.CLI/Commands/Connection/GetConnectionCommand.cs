using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.Manager.CLI.Commands.Connection
{
    [Command(CommandVerb.Get, "Connection", "Connection")]
    class GetConnectionCommand : CommandBase
    {

        public override void Execute()
        {
            foreach (var connection in Context.ConnectionManager.GetClientConnections())
            {
                Context.WriteOutput(connection.Target);
            }
        }
    }
}
