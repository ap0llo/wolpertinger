using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wolpertinger.FileShareCommon;

namespace Wolpertinger.Manager.CLI.Commands.Fileshare
{
    [Command(CommandVerb.Get, "Mount", "FileShare")]
    class GetMountCommand : FileShareCommand
    {
        public override void Execute()
        {
            var client = getFileShareComponent();

            var mounts = client.GetMountsAsync().Result;


            if (!mounts.Any())
            {
                Context.WriteOutput("No Mounts found");
                return;
            }

            int lineLength = Math.Min(mounts.Max(x => Math.Max(x.LocalPath.Length, x.MountPoint.Length)) + 12, Console.WindowWidth);
            string hl = new String('-', lineLength);

            foreach (MountInfo item in mounts)
            {
                Context.WriteOutput("LocalPath:  {0}", item.LocalPath);
                Context.WriteOutput("MountPoint: {0}", item.MountPoint);
                Context.WriteOutput(hl);
            }           
        }



    }
}
