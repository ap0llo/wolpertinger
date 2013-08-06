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
using System.Security;
using Nerdcave.Common;
using Nerdcave.Common.Extensions;
using Wolpertinger.Core;
using System.Management.Automation;

namespace Wolpertinger.Powershell.Cmdlets.Connection

{
	[Cmdlet("Authenticate", Nouns.User)]
	public class AuthenticateUserCmdlet : PSCmdlet
	{
		[Parameter(Mandatory = true, Position = 1)]
		public IClientConnection Connection { get; set; }

		[Parameter(Mandatory=true, Position = 2)]
		public string Username { get; set; }


		[Parameter(Mandatory=true, Position = 3)]
		public SecureString Password { get; set; }


		protected override void ProcessRecord()
		{
			var authComponent = new AuthenticationComponent() { ClientConnection = Connection };

			//get a new token for the authentication
			string token = authComponent.UserAuthGetTokenAsync().Result;

			//authenticate the user
			var authTask = authComponent.UserAuthVerifyAsync(Username, token, Password);


			if(authTask.Result)
			{
				WriteVerbose("Authentication successful");
			}
			else
			{
				WriteError(new ErrorRecord(new Exception("User authentication failed"), "AuthenticationFailed", ErrorCategory.AuthenticationError, null));
			}

								  
		}       
	}
}
