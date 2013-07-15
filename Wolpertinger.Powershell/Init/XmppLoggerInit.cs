using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wolpertinger.Core;

namespace Wolpertinger.Powershell.Init
{
    class XmppLoggerInit
    {
        public static void Initialze()
        {
            XmppLogger.ConnectionManager = Program.ConnectionManager;
        }

    }
}
