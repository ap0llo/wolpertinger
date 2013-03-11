/*

Licensed under the new BSD-License
 
Copyright (c) 2011-2013, Andreas Grünwald 
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

    Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
    Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
    Neither the name of the Wolpertinger project nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nerdcave.Common.Extensions;

namespace Wolpertinger.Core
{
    /// <summary>
    /// Logger that writes all messages to the Console
    /// </summary>
    public class ConsoleLogger : CustomLogger
    {
        /// <summary>
        /// Initializes a new instance of ConsoleLogger
        /// </summary>
        public ConsoleLogger()
            : base()
        { }

        /// <summary>
        /// Initializes a new instance of ConsoleLogger
        /// </summary>
        /// <param name="name">The logger's name</param>
        public ConsoleLogger(string name)
            : base(name)
        { }


        /// <summary>
        /// Writes the specified message to the log (in this case: System.Console)
        /// </summary>
        /// <param name="message">The message to be written to the log</param>
        protected override void send(string message)
        {
            if (!Name.IsNullOrEmpty())
                message = Name + " | " + message;

            Console.WriteLine("{0} | {1}", DateTime.Now.ToString("hh:mm:ss"), message);
        }

        /// <summary>
        /// Writes a debug-level message to the log
        /// </summary>
        /// <param name="message">The message to write to the log</param>
        protected override void debug(string message)
        {            
            send("DEBUG | " + message);            
        }

        /// <summary>
        /// Writes a info-level message to the log
        /// </summary>
        /// <param name="message">The message to write to the log</param>
        protected override void info(string message)
        {            
            send("INFO | " + message);
        }

        /// <summary>
        /// Writes a warn-level message to the log
        /// </summary>
        /// <param name="message">The message to write to the log</param>
        protected override void warn(string message)
        {
            send("WARN | " + message);
        }

        /// <summary>
        /// Writes a error-level message to the log
        /// </summary>
        /// <param name="message">The message to write to the log</param>
        protected override void error(string message)
        {
            send("ERROR | " + message);
        }

        /// <summary>
        /// Writes a fatal-level message to the log
        /// </summary>
        /// <param name="message">The message to write to the log</param>
        protected override void fatal(string message)
        {            
            send("FATAL | " + message);
        }



    }
}
