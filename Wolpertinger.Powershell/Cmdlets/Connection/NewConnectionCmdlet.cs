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
using System.Management.Automation;
using System.Text;
using Wolpertinger.Core;


namespace Wolpertinger.Powershell.Cmdlets.Connection
{
	[Cmdlet(VerbsCommon.New, Nouns.Connection)]
	public class NewConnectionCmdlet : PSCmdlet
	{

		[Parameter(Mandatory=true, Position=1)]
		public string Target { get; set; }



		protected override void ProcessRecord()
		{
			if (Program.ConnectionManager.GetClientConnection(Target) != null)
			{
				var errorRecord = new ErrorRecord(new ArgumentException(), "ConnectionAlreadyExists", ErrorCategory.InvalidArgument, null);
                WriteError(errorRecord);
			}
			else
			{
				var connection = Program.ConnectionManager.AddClientConnection(this.Target);
				connect(connection);
				WriteObject(connection);
			}
		}



		private void connect(IClientConnection connection)
		{
			connection.WtlpClient.MessagingClient.Connect();

			var authComponent = new AuthenticationComponent() { ClientConnection = connection };

			connection.WtlpClient.MessagingClient.MyResource = "Wolpertinger_Main";

			if (authComponent.EstablishConnectionAsync().Result)
			{
				WriteVerbose("Exchanging keys");

				authComponent.KeyExchangeAsync().Wait();

				WriteVerbose("Key exchange completed");
				WriteVerbose("Initiating Cluster Authentication");

				string token = authComponent.ClusterAuthGetTokenAsync().Result;

				if (authComponent.ClusterAuthVerifyAsync(token).Result)
				{
					WriteVerbose("Verified my Cluster Membership");
					if (authComponent.ClusterAuthRequestVerificationAsync().Result)
					{
						WriteVerbose("Verified Cluster Membership of target client");

						//we're finished
						WriteVerbose("Successfully connected to target");

					}
					else
					{
						WriteVerbose("Cluster Authentication of target client failed");
					}
				}
				else
				{
					WriteVerbose("Cluster Authentication failed");
				}

			}
		}

	}
}
