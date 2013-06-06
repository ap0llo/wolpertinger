using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wolpertinger.Core;

namespace Wolpertinger.Manager.CLI.Commands.XmppLogger
{
    [Command(CommandVerb.Get, "EnableDebugLogging", "XmppLogger")]
    class GetEnableDebugLoggingCommand : LoggerCommand
    {
        public override void Execute()
        {
            var logger = getLoggerComponent();

            bool enabled = logger.GetEnableDebugLoggingAsync().Result;
            Context.WriteOutput(enabled.ToString());
        }
    }
}
