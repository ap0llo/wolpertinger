using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.Manager.CLI.Commands.Connection
{
    [Command(CommandVerb.New, "Connection")]
    class NewConnectionCommand : CommandBase
    {

        [Parameter("Target", IsOptional= false, Position=1)]
        public string Target { get; set; }



        public override void Execute()
        {
            if (Context.ConnectionExists(this.Target))
            {
                Context.WriteError("Connection already exists");
            }
            else
            {
                var newConnection = Context.ConnectionManager.AddClientConnection(this.Target);
                Context.AddClientConnection(newConnection);
            }
        }



    }
}
