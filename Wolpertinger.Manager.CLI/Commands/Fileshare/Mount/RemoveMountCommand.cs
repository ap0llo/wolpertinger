using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.Manager.CLI.Commands.Fileshare
{
    [Command(CommandVerb.Remove, "Mount", "FileShare")]
    class RemoveMountCommand : FileShareCommand
    {
        [Parameter("VirtualPath", Position=2)]
        public string VirtualPath { get; set; }


        public override void Execute()
        {
            var client = getFileShareComponent();

            client.RemoveSharedDirectoryAsync(VirtualPath);
        }
    }

}
