using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.Manager.CLI.Commands.Fileshare
{
    [Command(CommandVerb.New, "SharedDirectory", "FileShare")]
    class NewSharedDirectory : FileShareCommand
    {
        [Parameter("LocalPath", Position=2)]
        public string LocalPath { get; set; }

        [Parameter("VirtualPath", Position=3)]
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

            client.AddSharedDirectoryAsync(LocalPath, VirtualPath);
        }
    }
}
