using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wolpertinger.Core;

namespace Wolpertinger.Manager.CLI.Commands.XmppLogger
{
    [Command(CommandVerb.Get, "Enabled", "XmppLogger")]
    class GetEnabledCommand : LoggerCommand
    {
        public override void Execute()
        {
            var logger = getLoggerComponent();

            bool enabled = logger.GetEnableAsync().Result;
         
            Context.WriteOutput(enabled.ToString());

        }
    }
}
