using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using Nerdcave.Common;
using Nerdcave.Common.Extensions;
using Wolpertinger.Core;

namespace Wolpertinger.Manager.CLI.Commands.Connection
{
    [Command(CommandVerb.Authenticate, "User", "Connection")]
    class AuthenticateUserCommand : CommandBase
    {

        [Parameter("Username", IsOptional= false, Position =1)]
        public string Username { get; set; }


        public override void Execute()
        {
            if(Context.ActiveConnection == null)
            {
                abort("No active connection");                
            }

            var authComponent = new AuthenticationComponent() { ClientConnection = Context.ActiveConnection };

            //get a new token for the authentication
            string token = authComponent.UserAuthGetTokenAsync().Result;

            //get the password
            SecureString securePassword = ConsoleHelper.GetPassword().ToSecureString();

            //authenticate the user
            var authTask = authComponent.UserAuthVerifyAsync(Username, token, securePassword);            

            Context.WriteInfo(  authTask.Result
                                ? "User Authentication successful"
                                : "User Authentication failed");                        
        }
    }
}
