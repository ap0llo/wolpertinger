using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.Manager.CLI.Commands.Fileshare
{
    [Command(CommandVerb.Test, "Permission", "FileShare")]
    class TestPermissionCommand : FileShareCommand
    {
        [Parameter("Path", Position=2)]
        public string Path{ get; set; }


        public override void Execute()
        {
            var client = getFileShareComponent();

            Context.WriteOutput(client.GetPermissionAsync(Path).Result.ToString());
        }

    }
}
