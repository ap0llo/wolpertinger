using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.Manager.CLI.Commands.Fileshare
{
    [Command(CommandVerb.New, "Mount", "FileShare")]
    class NewMount : FileShareCommand
    {
        [Parameter("LocalPath", Position=2)]
        public string LocalPath { get; set; }

        [Parameter("VirtualPath", Position=3)]
        public string VirtualPath { get; set; }


        public override void Execute()
        {
            var client = getFileShareComponent();

            client.AddSharedDirectoryAsync(LocalPath, VirtualPath);
        }
    }
}
