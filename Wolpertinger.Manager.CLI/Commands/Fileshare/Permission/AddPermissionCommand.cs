using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wolpertinger.FileShareCommon;

namespace Wolpertinger.Manager.CLI.Commands.Fileshare
{
    [Command(CommandVerb.New, "Permission", "FileShare")]
    class AddPermissionCommand : FileShareCommand
    {
        [Parameter("VirtualPath", IsOptional=false, Position=2)]
        public string VirtualPath { get; set; }

        [Parameter("Clients", IsOptional = false, Position = 3)]
        public string Clients { get; set; }




        public override void Execute()
        {
            var client = getFileShareComponent();

            var permission = new Permission() { Path = VirtualPath, PermittedClients = Clients.Split(';').ToList<string>() };


            client.AddPermissionAsync(permission);
        }


    }
}
