/*

Licensed under the new BSD-License
 
Copyright (c) 2011-2013, Andreas Grünwald 
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

    Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
    Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer 
	in the documentation and/or other materials provided with the distribution.
    Neither the name of the Wolpertinger project nor the names of its contributors may be used to endorse or promote products 
	derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS 
BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, 
STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wolpertinger.Core;

namespace Wolpertinger.Manager.CLI.Commands.Connection
{
    [Command(CommandVerb.Start, "Connection", "Connection")]
    class StartConnectionCommand : ConnectionDependentCommand
    {

        public override void Execute()
        {

            var connection = getClientConnection();

            connection.WtlpClient.MessagingClient.Connect();

            var authComponent = new AuthenticationComponent() { ClientConnection = connection };

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

    }
}
