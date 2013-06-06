using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.Manager.CLI.Commands.Fileshare
{
    [Command(CommandVerb.Remove, "Permission", "FileShare")]
    class RemovePermissionCommand : FileShareCommand
    {
        [Parameter("Path", Position=2)]
        public string Path { get; set; }


        public override void Execute()
        {
            var connection = getClientConnection(); ;


            if (connection == null)
            {
                Context.WriteError("No active connection");
                return;
            }


            var client = getFileShareComponent();


            client.RemovePermissionAsync(Path);
        }

    }
}
