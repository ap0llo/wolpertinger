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
            return new XmppLoggingConfiguratorComponent() { ClientConnection = getClientConnection() };
        }

        protected IClientConnection getClientConnection()
        {
            if (Context.ActiveConnection == null)
            {
                throw new CommandExecutionException("No active connection");
            }
            else
            {
                return Context.ActiveConnection;
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
