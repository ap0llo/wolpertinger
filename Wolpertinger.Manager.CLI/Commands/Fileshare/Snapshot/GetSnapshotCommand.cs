using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wolpertinger.FileShareCommon;

namespace Wolpertinger.Manager.CLI.Commands.Fileshare.Snapshot
{
    [Command(CommandVerb.Get, "Snapshot", "FileShare")]
    class GetSnapshotCommand : FileShareCommand
    {
        private const string SNAPSHOTFORMATSTRING = "{0, -36} | {1, -30}";


        public override void Execute()
        {           
            var client = getFileShareComponent();

            var snapshots = client.GetSnapshotsAsync().Result;            
        }



        private void printSnapshots(IEnumerable<SnapshotInfo> snapshots)
        {
            Context.WriteOutput("-----------------------------------------------------------");
            Context.WriteOutput(SNAPSHOTFORMATSTRING, "Id", "Time");
            Context.WriteOutput("-----------------------------------------------------------");

            foreach (var item in snapshots)
            {
                Context.WriteOutput(SNAPSHOTFORMATSTRING, item.Id, item.Time);
            }
        }
    }
}
