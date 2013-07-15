using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Wolpertinger.Powershell.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "WolpertingerVersion")]
    public class GetWolpertingerVersionCmdlet 
        : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            WriteObject(FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion);
        }
    }
}
