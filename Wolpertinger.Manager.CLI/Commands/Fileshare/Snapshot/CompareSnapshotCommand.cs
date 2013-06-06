using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.Manager.CLI.Commands.Fileshare.Snapshot
{
    [Command(CommandVerb.Compare, "Snapshot","FileShare")]
    class CompareSnapshotCommand : FileShareCommand
    {
        [Parameter("SnapshotIdLeft", Position = 2)]
        public Guid SnapshotIdLeft { get; set; }

        [Parameter("SnapshotIdRight", Position = 3)]
        public Guid SnapshotIdRight { get; set; }



        public override void Execute()
        {
            if (SnapshotIdLeft == SnapshotIdRight)
            {
                throw new CommandExecutionException("Tried to compare snapshot to itself");
            }

            var client = getFileShareComponent();


            var task = client.CompareSnapshots(SnapshotIdLeft, SnapshotIdRight);
            var diff = task.Result;

            printDirectoryObjectDiff(diff);
        }
    }
}
