using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Wolpertinger.Core;

namespace Wolpertinger.Manager.CLI.Commands.Wolpertinger
{
    [Command(CommandVerb.Get, "Version", "Wolpertinger")]
    class GetVersionCommand : CommandBase
    {
        public override void Execute()
        {
            Context.WriteOutput("Wolpertinger Manager {0}", Assembly.GetExecutingAssembly().GetName().Version.ToString());
            Context.WriteOutput("Wolpertinger.Core    {0}", Assembly.GetAssembly(typeof(DefaultConnectionManager)).GetName().Version.ToString());
        }
    }
}
