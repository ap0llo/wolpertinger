using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.Manager.CLI.Commands.Connection
{
    [Command(CommandVerb.Get, "Connection", "Connection")]
    class GetConnectionCommand : CommandBase
    {

        [Parameter("ConnectionName", IsOptional = true, Position = 2)]
        public string ConnectionName { get; set; }


        public override void Execute()
        {

            var query = Context.ConnectionManager.GetClientConnections().Where(x => x.Target.ToLower() == ConnectionName.ToLower());

            int index = 1;
            if (ConnectionName == null)
            {
                foreach (var connection in Context.ConnectionManager.GetClientConnections())
                {
                    Context.WriteOutput("#{0} | {1}", index++, connection.Target);
                }    
            }
            else if (ConnectionName.StartsWith("#"))
            {
                int outValue;
                if (int.TryParse(ConnectionName.Substring(1), out outValue))
                {
                    if (outValue > 0 && Context.ConnectionManager.GetClientConnections().Count() >= outValue)
                    {
                        Context.WriteOutput(Context.ConnectionManager.GetClientConnections().Skip(outValue -1).First().Target);
                        return;
                    }
                }

                abort("Connection not found");
            }
            else if (query.Any())
            {
                Context.WriteOutput(query.First().Target);
            }
            else
            {
                abort("Connection not found");
            }
        }
    }
}
