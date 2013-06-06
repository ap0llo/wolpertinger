using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.Manager.CLI.Commands.XmppLogger
{
    [Command(CommandVerb.Get, "LogLevel", "XmppLogger")]
    class GetLogLevelCommand : LoggerCommand
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
                string loglevel = logger.GetLoglevelAsync().Result.ToString();
                Context.WriteOutput(loglevel.ToString());
            }
            catch (TimeoutException)
            {
                Context.WriteError("Connection timed out");
            }
        }
    }
}
