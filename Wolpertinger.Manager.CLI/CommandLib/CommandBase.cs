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


        /// <summary>
        /// Aborts the execution of the command by throwing a CommandExecutionException with the specified message
        /// </summary>
        /// <param name="message">The message for the CommandExecutionException</param>
        protected void abort(string message)
        {
            throw new CommandExecutionException(message);
        }

    }
}
