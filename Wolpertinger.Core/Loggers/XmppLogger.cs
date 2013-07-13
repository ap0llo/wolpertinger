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
using Slf;
using System.Xml.Linq;
using System.IO;
using Nerdcave.Common.Xml;

namespace Wolpertinger.Core
{
    /// <summary>
    /// A ILogger implementation that send log messages to a Jabber recipient
    /// </summary>
    public class XmppLogger : CustomLogger
    {

        private static ILogger logger = LoggerService.GetLogger("XmppLogger");

        private static IMessagingClient xmppClient;

        private static IConnectionManager _connectionManager;
        private static string _recipient;
        private static bool _enable;
        private static bool _enableDebugLogging;
        private static LogLevel _logLevel;

        private static bool loaded = false;

        private static KeyValueStore settingsFile;


        /// <summary>
        /// The ConnnectionManager used to send messages
        /// </summary>
        public static IConnectionManager ConnectionManager 
        {
            get { return _connectionManager; }
            set
            {
                if (value != _connectionManager)
                {
                    _connectionManager = value;
                    if (_connectionManager == null)
                    {
                        xmppClient = null;
                        return;
                    }
                            
                    var clients = _connectionManager.GetMessagingClients();
                    if (clients.Any(x => x.ServiceName.ToLower() == "xmpp"))
                    {
                        xmppClient = clients.First(x => x.ServiceName.ToLower() == "xmpp");
                    }
                    else
                    { 
                        xmppClient = null;
                        logger.Error("Could not get Xmpp Client from ConnectionManager");
                    }
                }
            }
        }


        /// <summary>
        /// Specifies to where to send log-messages to
        /// </summary>
        public static string Recipient 
        {
            get { return _recipient == null ? "" :_recipient; } 
            set 
            {
                _recipient = value;
                if(loaded)
                    settingsFile.SaveItem("Recipient", Recipient);
            } 
        }

        /// <summary>
        /// Enables/Disables logging
        /// </summary>
        public static bool Enable 
        {
            get { return _enable; } 
            set 
            {
                _enable = value; 
                if(loaded)
                    settingsFile.SaveItem("Enable", Enable);
            }
        }

        /// <summary>
        /// Specifies the minimum log-level of a message that will be written to the log
        /// </summary>
        public static LogLevel LogLevel 
        { 
            get { return _logLevel; } 
            set 
            { 
                _logLevel = value; 
                //saveSettings(); 
                if(loaded)
                    settingsFile.SaveItem("LogLevel", LogLevel.ToString());
            } 
        }

        /// <summary>
        /// Specifies, whether "Debug" messages are written to the log
        /// </summary>
        public static bool EnableDebugLogging 
        {
            get { return _enableDebugLogging; } 
            set 
            {
                _enableDebugLogging = value; 
                if(loaded)
                    settingsFile.SaveItem("EnableDebugLogging", EnableDebugLogging);
            } 
        }




        #region Constructors

        public XmppLogger()
            : base()
        { }


        public XmppLogger(string name)
            : base(name)
        { }

        #endregion


        #region Static Methods


        /// <summary>
        /// Loads the logger's settings from the configuration file
        /// </summary>
        public static void LoadSettings()
        {
            settingsFile = new KeyValueStore(Path.Combine(DefaultConnectionManager.SettingsFolder, "XmppLoggerSettings.xml"));

            Enable = settingsFile.GetItem<bool>("Enable");
            LogLevel level;
            LogLevel = Enum.TryParse(settingsFile.GetItem<string>("LogLevel"), out level) ? level : LogLevel.Info;
            Recipient = settingsFile.GetItem<string>("Recipient");
            EnableDebugLogging = settingsFile.GetItem<bool>("EnableDebugLogging");

            loaded = true;

        }
        
        
        #endregion


        /// <summary>
        /// Sends the specified message to the jabber-account specified in Recipient
        /// </summary>
        /// <param name="message">The message to be sent</param>
        protected override void send(string message)
        {
            if (!Name.IsNullOrEmpty())
                message = Name + "|" + message;

            if (Enable && !Recipient.IsNullOrEmpty() && xmppClient != null)
                xmppClient.SendMessage(XmppLogger.Recipient, message);
        }


        /// <summary>
        /// Writes a info-level message to the log
        /// </summary>
        /// <param name="message">The message to write to the log</param>
        protected override void debug(string message)
        {
            if (EnableDebugLogging)
            {
                send("DEBUG|" + message);
            }
        }

        /// <summary>
        /// Writes a info-level message to the log
        /// </summary>
        /// <param name="message">The message to write to the log</param>
        protected override void info(string message)
        {
            if (LogLevel <= LogLevel.Info)
                send("INFO|" + message);           
        }

        /// <summary>
        /// Writes a warn-level message to the log
        /// </summary>
        /// <param name="message">The message to write to the log</param>
        protected override void warn(string message)
        {
            if (LogLevel <= LogLevel.Warn)
                send("WARN|" + message);
        }

        /// <summary>
        /// Writes a error-level message to the log
        /// </summary>
        /// <param name="message">The message to write to the log</param>
        protected override void error(string message)
        {
            if (LogLevel <= LogLevel.Error)
                send("ERROR|" + message);
        }

        /// <summary>
        /// Writes a fatal-level message to the log
        /// </summary>
        /// <param name="message">The message to write to the log</param>
        protected override void fatal(string message)
        {
            if (LogLevel <= LogLevel.Fatal)
                send("FATAL|" + message);
        }


        
    }

    /// <summary>
    /// List of possible LogLevels 
    /// </summary>
    public enum LogLevel    
    {
        None = 0,
        Info = 1,
        Warn = 2,
        Error = 3,
        Fatal = 4
    }
}
