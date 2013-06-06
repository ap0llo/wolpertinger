using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wolpertinger.Core;
using Wolpertinger.FileShareCommon;

namespace Wolpertinger.Manager.CLI.Commands.Fileshare
{
    [Command(CommandVerb.Get, "Directory", "FileShare")]
    class GetDirectoryCommand : FileShareCommand
    {
        [Parameter("Path", IsOptional=false, Position=2)]
        public string Path { get; set; }

        [Parameter("SnapshotId", IsOptional = true, Position = 3)]
        public Guid SnapshotId { get; set; }


        public override void Execute()
        {
            var connection = getClientConnection();
            if(connection == null)
            {
                Context.WriteError("No active conncetion");
                return;
            }


            var client = getFileShareComponent();


            if (SnapshotId == default(Guid))
            {
                SnapshotId = Guid.Empty;
            }

            try
            {
                DirectoryObject dir = client.GetDirectoryInfoAsync(Path, 1, SnapshotId).Result;
                printDirectoryObject(dir);
            }
            catch (TimeoutException)
            {
                Context.WriteError("Connection timed out");
            }
            catch (RemoteErrorException ex)
            {
                Context.WriteError("Remote error occurred: " + ex.Error.ToString());
            }

        }
    }
}
