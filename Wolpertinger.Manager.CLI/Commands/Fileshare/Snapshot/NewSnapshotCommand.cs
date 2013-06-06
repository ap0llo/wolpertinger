using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.Manager.CLI.Commands.Fileshare.Snapshot
{
    [Command(CommandVerb.New, "Snapshot", "FileShare")]    
    class NewSnapshotCommand : FileShareCommand
    {
        public override void Execute()
        {
            var client = getFileShareComponent();

            Context.WriteInfo("Creating Snapshot");
            var task = client.CreateSnapshotAsync();
            task.Wait();
            Context.WriteInfo("Snapshot created");
            Context.WriteOutput("Id: " + task.Result.ToString());
        }
    }
}
