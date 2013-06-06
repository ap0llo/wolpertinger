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
                Context.WriteError("No active connection");
                return;
            }


            SecureString securePassword = ConsoleHelper.GetPassword().ToSecureString();

            var authComponent = new AuthenticationComponent() { ClientConnection = Context.ActiveConnection };

            string token = authComponent.UserAuthGetTokenAsync().Result;

            var authTask = authComponent.UserAuthVerifyAsync(Username, token, securePassword);            

            Context.WriteInfo(  authTask.Result
                                ? "User Authentication successful"
                                : "User Authentication failed");                        
        }
    }
}
