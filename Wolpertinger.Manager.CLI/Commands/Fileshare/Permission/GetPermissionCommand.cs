using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wolpertinger.FileShareCommon;

namespace Wolpertinger.Manager.CLI.Commands.Fileshare
{
    [Command(CommandVerb.Get, "Permission", "FileShare")]
    class GetPermissionCommand : FileShareCommand
    {

        public override void Execute()
        {
            var client = getFileShareComponent();

            IEnumerable<Permission> permissions = client.GetAddedPermissionsAsync().Result;

            foreach (Permission p in permissions)
            {
                Context.WriteOutput("Permission for " + p.Path);
                foreach (string str in p.PermittedClients)
                {
                    Context.WriteOutput("     " + p);
                }
                Context.WriteOutput("");
            }
        }
    }
}
