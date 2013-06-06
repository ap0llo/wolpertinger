using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.Manager.CLI.Commands.Connection
{
    [Command(CommandVerb.Enter, "Connection", "Connection")]
    class EnterConnectionCommand : CommandBase
    {

        [Parameter("Target", IsOptional=false, Position=1)]
        public string Target { get; set; }


        public override void Execute()
        {
            var connection = Context.ConnectionManager.GetClientConnection(Target);

            if (connection == null)
            {
                abort("Connection not found");
            }
            else
            {
                Context.ActiveConnection = connection;
            }
        }

    }
}
