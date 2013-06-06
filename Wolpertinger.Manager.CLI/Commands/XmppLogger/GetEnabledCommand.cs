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
            if (!checkConnection())
            {
                return;
            }

            var logger = getLoggerComponent();

            try
            {
                bool enabled = logger.GetEnableAsync().Result;
                Context.WriteOutput(enabled.ToString());
            }
            catch (TimeoutException)
            {
                Context.WriteError("Connection timed out");
            }
        }

    }
}
