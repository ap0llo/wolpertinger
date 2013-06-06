using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nerdcave.Common.Extensions;

namespace Wolpertinger.Manager.CLI.Commands.Fileshare
{
    [Command(CommandVerb.Set, "RootDirectory", "FileShare")]
    class SetRootDirectoryCommand : FileShareCommand
    {
        [Parameter("Path", Position = 2)]
        public string Path { get; set; }


        public override void Execute()
        {
            if (Path.IsNullOrEmpty())
            {
                throw new CommandExecutionException("Path may not be null or empty");
            }                       

            var client = getFileShareComponent();

            client.SetRootDirectoryPathAsync(Path);            
        }


    }
}
