using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wolpertinger.Core;

namespace Wolpertinger.Manager.CLI.Commands.XmppLogger
{
    abstract class LoggerCommand : CommandBase
    {

        protected XmppLoggingConfiguratorComponent getLoggerComponent()
        {
            return new XmppLoggingConfiguratorComponent() { ClientConnection = Context.ActiveConnection };
        }

        protected bool checkConnection()
        {
            if (Context.ActiveConnection == null)
            {
                Context.WriteError("No active connection");
                return false;
            }
            else
            {
                return true;
            }
        }


        protected static Core.LogLevel? getLogLevel(string str)
        {
            Core.LogLevel lvl;

            if (Enum.TryParse<Core.LogLevel>(str, true, out lvl))
                return lvl;
            else
                return null;
        }

    }
}
