using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.Manager.CLI.Commands.Fileshare.Snapshot
{
    [Command(CommandVerb.Remove, "Snapshot", "FileShare")]
    class RemoveSnapshotCommand : FileShareCommand
    {
        [Parameter("SnapshotId", Position = 2)]
        public Guid SnapshotId { get; set; }

        public override void Execute()
        {
            var client = getFileShareComponent();

            if (SnapshotId == Guid.Empty)
            {
                throw new CommandExecutionException("Invalid SnapshotId");
            }

            client.DelteSnapshotAsync(SnapshotId);
        }
    }
}
