using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using Wolpertinger.Core;

namespace Wolpertinger.Powershell.Cmdlets
{
    public class LoggerCmdletBase
        : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 1)]
        public IClientConnection Connection { get; set; }





        protected XmppLoggingConfiguratorComponent getLoggerComponent()
        {
            return new XmppLoggingConfiguratorComponent() { ClientConnection = this.Connection };
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
