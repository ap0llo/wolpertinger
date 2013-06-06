using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.Manager.CLI.Commands.Fileshare
{
    [Command(CommandVerb.Remove, "SharedDirectory", "FileShare")]
    class RemoveSharedDirectoryCommand : FileShareCommand
    {
        [Parameter("VirtualPath", Position=2)]
        public string VirtualPath { get; set; }


        public override void Execute()
        {
            var connection = getClientConnection();

            if (connection == null)
            {
                Context.WriteError("No active connection");
                return;
            }

            var client = getFileShareComponent();

            client.RemoveSharedDirectoryAsync(VirtualPath);

            //TODO: error checking
        }
    }

}
