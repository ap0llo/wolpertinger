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
using Slf;


namespace Wolpertinger.Core
{
    /// <summary>
    /// Abstract implementation of ILogger. 
    /// Will take all log-messages, build a single line string from it and pass it to the corresponding abstact method.
    /// </summary>
    public abstract class CustomLogger : ILogger
    {
        /// <summary>
        /// Writes the given message to the output-stream
        /// </summary>
        /// <param name="message">The message to write to the log</param>
        protected abstract void send(string message);

        /// <summary>
        /// Handler for debug-Messages
        /// </summary>
        /// <param name="message">The message to write to the log</param>
        protected abstract void debug(string message);

        /// <summary>
        /// Handler for Info-Messages
        /// </summary>
        /// <param name="message">The message to write to the log</param>
        protected abstract void info(string message);

        /// <summary>
        /// Handler for Warn-Messages
        /// </summary>
        /// <param name="message">The message to write to the log</param>
        protected abstract void warn(string message);

        /// <summary>
        /// Handler for Error-Messages
        /// </summary>
        /// <param name="message">The message to write to the log</param>
        protected abstract void error(string message);

        /// <summary>
        /// Handler fpr Fatal-Messages
        /// </summary>
        /// <param name="message">The message to write to the log</param>
        protected abstract void fatal(string message);

        /// <summary>
        /// The logger's name
        /// </summary>
        protected string name;


        public CustomLogger()
            : this("")
        { }

        public CustomLogger(string name)
        {
            this.name = name;
        }




        #region ILogger Members

        public void Debug(Exception exception, string format, IFormatProvider provider, params object[] args)
        {
            debug(exception.Message + "|" + string.Format(provider, format, args));
        }

        public void Debug(Exception exception, string format, params object[] args)
        {
            debug(exception.Message + "|" + string.Format(format, args));
        }

        public void Debug(Exception exception, string message)
        {
            debug(exception.Message + "|" + message);
        }

        public void Debug(Exception exception)
        {
            debug(exception.Message);
        }

        public void Debug(IFormatProvider provider, string format, params object[] args)
        {
            debug(String.Format(format, provider, args));
        }

        public void Debug(string format, params object[] args)
        {
            debug(string.Format(format, args));
        }

        public void Debug(string message)
        {
            debug(message);
        }

        public void Debug(object obj)
        {
            debug(obj.ToString());
        }

        public void Error(Exception exception, string format, IFormatProvider provider, params object[] args)
        {
            error(exception.Message + "|" + String.Format(provider, format, args));
        }

        public void Error(Exception exception, string format, params object[] args)
        {
            error(exception.Message + "|" + String.Format(format, args));
        }

        public void Error(Exception exception, string message)
        {
            error(exception.Message + "|" + message);
        }

        public void Error(Exception exception)
        {
            error(exception.Message);
        }

        public void Error(IFormatProvider provider, string format, params object[] args)
        {
            error(String.Format(provider, format, args));
        }

        public void Error(string format, params object[] args)
        {
            error(String.Format(format, args));
        }

        public void Error(string message)
        {
            error(message);
        }

        public void Error(object obj)
        {
            error(obj.ToString());
        }

        public void Fatal(Exception exception, string format, IFormatProvider provider, params object[] args)
        {
            fatal(exception.Message + "|" + String.Format(provider, format, args));
        }

        public void Fatal(Exception exception, string format, params object[] args)
        {
            fatal(exception.Message + "|" + String.Format(format, args));
        }

        public void Fatal(Exception exception, string message)
        {
            fatal(exception.Message + "|" + message);
        }

        public void Fatal(Exception exception)
        {
            fatal(exception.Message);
        }

        public void Fatal(IFormatProvider provider, string format, params object[] args)
        {
            fatal(String.Format(provider, format, args));
        }

        public void Fatal(string format, params object[] args)
        {
            fatal(String.Format(format, args));
        }

        public void Fatal(string message)
        {
            fatal(message);
        }

        public void Fatal(object obj)
        {
            fatal(obj.ToString());
        }

        public void Info(Exception exception, string format, IFormatProvider provider, params object[] args)
        {
            info(exception.Message + "|" + String.Format(provider, format, args));
        }

        public void Info(Exception exception, string format, params object[] args)
        {
            info(exception.Message + "|" + String.Format(format, args));
        }

        public void Info(Exception exception, string message)
        {
            info(exception.Message + "|" + message);
        }

        public void Info(Exception exception)
        {
            info(exception.Message);
        }

        public void Info(IFormatProvider provider, string format, params object[] args)
        {
            info(String.Format(provider, format, args));
        }

        public void Info(string format, params object[] args)
        {
            info(String.Format(format, args));
        }

        public void Info(string message)
        {
            info(message);
        }

        public void Info(object obj)
        {
            info(obj.ToString());
        }

        public void Log(LogItem item)
        {
            switch (item.LogLevel)
            {
                case Slf.LogLevel.Debug:
                    debug(item.Message);
                    break;
                case Slf.LogLevel.Error:
                    error(item.Message);
                    break;
                case Slf.LogLevel.Fatal:
                    fatal(item.Message);
                    break;
                case Slf.LogLevel.Info:
                    info(item.Message);
                    break;
                case Slf.LogLevel.Undefined:
                    info("UNDEFINED:" + item.Message);
                    break;
                case Slf.LogLevel.Warn:
                    warn(item.Message);
                    break;
            }
        }

        public string Name
        {
            get { return name; }
        }

        public void Warn(Exception exception, string format, IFormatProvider provider, params object[] args)
        {
            warn(exception.Message + "|" + String.Format(provider, format, args));
        }

        public void Warn(Exception exception, string format, params object[] args)
        {
            warn(exception.Message + "|" + String.Format(format, args));
        }

        public void Warn(Exception exception, string message)
        {
            warn(exception.Message + "|" + message);
        }

        public void Warn(Exception exception)
        {
            warn(exception.Message);
        }

        public void Warn(IFormatProvider provider, string format, params object[] args)
        {
            warn(String.Format(provider, format, args));
        }

        public void Warn(string format, params object[] args)
        {
            warn(String.Format(format, args));
        }

        public void Warn(string message)
        {
            warn(message);
        }

        public void Warn(object obj)
        {
            warn(obj.ToString());
        }

        #endregion

    }
}
