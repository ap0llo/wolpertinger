using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wolpertinger.Core;

namespace Wolpertinger.Manager.CLI.Commands.XmppLogger
{
    [Command(CommandVerb.Set, "EnableDebugLogging", "XmppLogger")]
    class SetSetEnableDebugLoggingCommand : LoggerCommand
    {
        [Parameter("Value", IsOptional = false, Position = 1)]
        public bool Value { get; set; }

        public override void Execute()
        {
            if (!checkConnection())
            {
                return;
            }

            var logger = getLoggerComponent();

            try
            {
                logger.SetEnableDebugLoggingAsync(Value);
            }
            catch (TimeoutException)
            {
                Context.WriteError("Connection timed out");
            }
        }
    }
}
