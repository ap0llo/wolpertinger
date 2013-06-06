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
            var logger = getLoggerComponent();

            var loglevel = logger.GetLoglevelAsync().Result;

            Context.WriteOutput(loglevel.ToString());
        }
    }
}
