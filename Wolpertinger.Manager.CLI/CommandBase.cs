using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.Manager.CLI
{
    abstract class CommandBase
    {
        public CommandContext Context { get; set; }



        public abstract void Execute();

    }
}
