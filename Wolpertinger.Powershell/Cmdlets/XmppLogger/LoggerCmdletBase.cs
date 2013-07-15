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
        : CmdletBase
    {
      


        protected XmppLoggingConfiguratorComponent getLoggerComponent()
        {
            return new XmppLoggingConfiguratorComponent() { ClientConnection = this.Connection };
        }



       
    }
}
