using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wolpertinger.Powershell.Init
{
    class AppDataInit
    {
        public static void Initialize()
        {          
            if (!Directory.Exists(Program.APPDATAFOLDER))
            {
                Directory.CreateDirectory(Program.APPDATAFOLDER);
            }
        }
    }
}
