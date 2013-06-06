using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.Manager.CLI.Commands.Fileshare
{
    [Command(CommandVerb.Get, "RootDirectory", "FileShare")]
    class GetRootDirectoryCommand : FileShareCommand
    {
        public override void Execute()
        {
            var connection = getClientConnection();

            if (connection == null)
            {
                Context.WriteError("No active connection");
                return;
            }


            var client = getFileShareComponent();

            var root = client.GetRootDirectoryPathAsync().Result;

            Context.WriteOutput(root);
        }
    }
}
