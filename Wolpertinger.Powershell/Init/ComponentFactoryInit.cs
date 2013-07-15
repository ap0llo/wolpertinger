using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Wolpertinger.Core;

namespace Wolpertinger.Powershell.Init
{
    class ComponentFactoryInit
    {
        public static void Initialize()
        {
            //Register Wolpertinger.CoreComponents assembly
            DefaultComponentFactory.RegisterComponentAssembly(Assembly.GetAssembly(typeof(AuthenticationComponent)));                       

            //Register Wolpertinger.Powershell assembly
            DefaultComponentFactory.RegisterComponentAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
