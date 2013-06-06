using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wolpertinger.Core;

namespace Wolpertinger.Manager.CLI.Commands.Connection
{
    [Command(CommandVerb.Start, "Connection", "Connection")]
    class StartConnectionCommand : CommandBase
    {

        public override void Execute()
        {
            if (Context.ActiveConnection == null)
            {
                Context.WriteError("No connection active");
            }
            else
            {
                var connection = Context.ActiveConnection;

                connection.WtlpClient.MessagingClient.Connect();

                var authComponent = new AuthenticationComponent() { ClientConnection = connection };

                try
                {
                    connection.WtlpClient.MessagingClient.MyResource = "Wolpertinger_Main";

                    if (authComponent.EstablishConnectionAsync().Result)
                    {
                        Context.WriteInfo("Exchanging keys");

                        authComponent.KeyExchangeAsync().Wait();

                        Context.WriteInfo("Key exchange completed");
                        Context.WriteInfo("Initiating Cluster Authentication");

                        string token = authComponent.ClusterAuthGetTokenAsync().Result;

                        if (authComponent.ClusterAuthVerifyAsync(token).Result)
                        {
                            Context.WriteInfo("Verified my Cluster Membership");
                            if (authComponent.ClusterAuthRequestVerificationAsync().Result)
                            {
                                Context.WriteInfo("Verified Cluster Membership of target client");

                                //we're finished
                                Context.WriteInfo("Successfully connected to target");

                            }
                            else
                            {
                                Context.WriteInfo("Cluster Authentication of target client failed");
                            }
                        }
                        else
                        {
                            Context.WriteInfo("Cluster Authentication failed");
                        }

                    }
                }
                catch (TimeoutException)
                {
                }


            }
        }

    }
}
