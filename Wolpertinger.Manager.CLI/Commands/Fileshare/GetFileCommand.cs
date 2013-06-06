using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wolpertinger.Core;
using Wolpertinger.FileShareCommon;

namespace Wolpertinger.Manager.CLI.Commands.Fileshare
{
    [Command(CommandVerb.Get, "File", "FileShare")]
    class GetFileCommand : FileShareCommand
    {
        [Parameter("Path", IsOptional = false, Position = 2)]
        public string Path { get; set; }

        public override void Execute()
        {
            var connection = getClientConnection();
            if (connection == null)
            {
                Context.WriteError("No active connection");
                return;
            }


            var client = getFileShareComponent();

            try
            {
                FileObject file = client.GetFileInfoAsync(Path).Result;
                printFileObject(file);
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
