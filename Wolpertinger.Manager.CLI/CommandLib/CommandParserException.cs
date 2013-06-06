using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.Manager.CLI
{
    public class CommandParserException : Exception
    {
        public CommandParserException(string message)
            : base(message)
        {
        }


        public CommandParserException(string format, params object[] args)
            : this(String.Format(format, args))
        {
        }
    }
}
