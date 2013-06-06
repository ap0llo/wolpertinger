using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.Manager.CLI
{
    class CommandExecutionException : Exception
    {
        public CommandExecutionException(string message)
            : base(message)
        {
        }

        public CommandExecutionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

    }
}
