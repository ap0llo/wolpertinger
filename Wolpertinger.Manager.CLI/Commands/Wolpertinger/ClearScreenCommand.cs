using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.Manager.CLI.Commands.Wolpertinger
{
    [Command(CommandVerb.Clear, "Screen", "Wolpertinger")]
    class ClearScreenCommand : CommandBase
    {


        public override void Execute()
        {
            Console.Clear();
        }
    }
}
