using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wolpertinger.Core;

namespace Wolpertinger.Manager.CLI.Commands.ClientInfo
{
    [Command(CommandVerb.Get, "ClientInfo", "ClientInfo")]
    class GetClientInfoCommand : CommandBase
    {
        public override void Execute()
        {
            if (Context.ActiveConnection == null)
            {
                Context.WriteError("No active connection");
                return;
            }

            var clientInfoComponent = new ClientInfoClientComponent() { ClientConnection = Context.ActiveConnection };

            try
            {
                var result = clientInfoComponent.GetClientInfoAsync().Result;

                Context.WriteOutput("JId:             {0}", result.JId);                
                Context.WriteOutput("ProtocolVersion: {0}", result.ProtocolVersion);
                Context.WriteOutput("TrustLevel:      {0}", result.TrustLevel);
                Context.WriteOutput("Profiles:");
                foreach (var profile in result.Profiles)
                {
                    Context.WriteOutput("\t" + profile.ToString());
                }                
                
            }
            catch (TimeoutException)
            {
                Context.WriteError("Connection timed out");
            }
        }
    }
}
