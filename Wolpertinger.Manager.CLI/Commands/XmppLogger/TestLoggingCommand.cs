using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.Manager.CLI.Commands.XmppLogger
{
    [Command(CommandVerb.Test, "Logging", "XmppLogger")]
    class TestLoggingCommand : LoggerCommand
    {
        [Parameter("LogLevel", IsOptional=false, Position=1)]
        public Core.LogLevel LogLevel { get; set; }



        public override void Execute()
        {
            if (!checkConnection())
            {
                return;
            }

            var logger = getLoggerComponent();


            try
            {
                logger.TestLoggingAsync(LogLevel);
            }
            catch (TimeoutException)
            {
                Context.WriteError("Connection timed out");
            }

        }
    }
}
