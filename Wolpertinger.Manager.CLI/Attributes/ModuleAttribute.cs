using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.Manager.CLI
{
    class ModuleAttribute : Attribute
    {

        public string ModuleName { get; set; }



        public ModuleAttribute(string name)
        {
            this.ModuleName = name;
        }


    }
}
