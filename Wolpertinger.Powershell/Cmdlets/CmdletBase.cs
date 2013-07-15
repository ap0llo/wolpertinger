using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using Wolpertinger.Core;

namespace Wolpertinger.Powershell.Cmdlets
{
    public abstract class CmdletBase
        : PSCmdlet
    {
        [Parameter(Position = 1, Mandatory = true)]
        public IClientConnection Connection{ get; set; }

    }
}
