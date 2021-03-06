﻿/*

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
using System.Threading.Tasks;
using Nerdcave.Common;
using Nerdcave.Common.Extensions;
using Slf;

namespace Wolpertinger.Core
{
    /// <summary>
    /// Combined server- and client-implementation of the XmppLoggingConfigurator component
    /// See Wolpertinger API Documentation for details on the component.
    /// </summary>
    [Component(ComponentNamesExtended.XmppLoggingConfigurator)]
    public class XmppLoggingConfiguratorComponent : IComponent
    {


        public IClientConnection ClientConnection { get; set; }


        private ILogger logger = LoggerService.GetLogger("XmppLoggingConfigurator");




    
        #region Client Implementation

        /// <summary>
        /// Asynchronously calls the SetEnable RemoteMethod on the target client
        /// </summary>
        /// <param name="isEnabled">The new enabled-state to be set</param>
        public Task SetEnableAsync(bool isEnabled)
        {
            return Task.Factory.StartNew(delegate
            {
                ClientConnection.CallRemoteAction(
                    ComponentNamesExtended.XmppLoggingConfigurator,
                    XmppLoggingConfiguratorMethods.SetEnable, isEnabled);
            });
        }

        /// <summary>
        /// Asynchrounously calls the GetEnable RemoteMethod on the target client and
        /// raises the GetEnableCompleted event when a response is received.
        /// </summary>
        public Task<bool> GetEnableAsync()
        {
            return Task.Factory.StartNew<bool>(delegate
            {
                return (bool)ClientConnection.CallRemoteFunction(
                    ComponentNamesExtended.XmppLoggingConfigurator,
                    XmppLoggingConfiguratorMethods.GetEnable);
            });
        }


        /// <summary>
        /// Asynchronously calls the SetRecipient RemoteMehod on the target client
        /// </summary>
        /// <param name="recipient">The new value to set</param>
        public Task SetRecipientAsync(string recipient)
        {
            return Task.Factory.StartNew(delegate
            {
                ClientConnection.CallRemoteAction(
                    ComponentNamesExtended.XmppLoggingConfigurator,
                    XmppLoggingConfiguratorMethods.SetRecipient,
                    recipient);
            });
        }

        /// <summary>
        /// Asynchronously calls the GetRecipient RemoteMethod on the target client and
        /// raises the GetRecipientCompleted event when a response is received
        /// </summary>
        public Task<string> GetRecipientAsync()
        {
            return Task.Factory.StartNew<string>( delegate
            {
                return (string) ClientConnection.CallRemoteFunction(
                    ComponentNamesExtended.XmppLoggingConfigurator,
                    XmppLoggingConfiguratorMethods.GetRecipient);            
            });
        }


        /// <summary>
        /// Asynchronously calls the SetLogLevel RemoteMethod on the target client.
        /// </summary>
        /// <param name="level">The new log-level to set</param>
        public Task SetLogLevelAsync(LogLevel level)
        {
            return Task.Factory.StartNew(delegate
            {
                ClientConnection.CallRemoteAction(
                    ComponentNamesExtended.XmppLoggingConfigurator,
                    XmppLoggingConfiguratorMethods.SetLogLevel,
                    level.ToString());
            });
        }

        /// <summary>
        /// Asynchronously calls the GetLogLevel RemoteMethod on the target client and 
        /// raises the GetLogLevelCompleted event when a response is received
        /// </summary>
        public Task<LogLevel> GetLoglevelAsync()
        {
            return Task.Factory.StartNew<LogLevel>( delegate
            {
                string level = (string)ClientConnection.CallRemoteFunction(
                    ComponentNamesExtended.XmppLoggingConfigurator,
                    XmppLoggingConfiguratorMethods.GetLogLevel);
                LogLevel logLevel;
                return Enum.TryParse<LogLevel>(level, out logLevel) ? logLevel : LogLevel.None;
            });            
        }

        /// <summary>
        /// Asynchronously calls the SetEnableDebugLogging RemoteMethod on the target client
        /// </summary>
        /// <param name="value">The new value to set</param>
        public Task SetEnableDebugLoggingAsync(bool value)
        {
            return Task.Factory.StartNew( delegate 
            {
                ClientConnection.CallRemoteAction(
                    ComponentNamesExtended.XmppLoggingConfigurator,
                    XmppLoggingConfiguratorMethods.SetEnableDebugLogging,
                    value);
            });
        }

        /// <summary>
        /// Asynchronously calls the GetEnableDebugLogging RemoteMethod on the target client and
        /// raises the GetEnableDebugLoggingCompleted event when a response is received.
        /// </summary>
        public Task<bool> GetEnableDebugLoggingAsync()
        {
            return Task.Factory.StartNew<bool>( delegate 
            {
                return (bool) ClientConnection.CallRemoteFunction(
                    ComponentNamesExtended.XmppLoggingConfigurator,
                    XmppLoggingConfiguratorMethods.GetEnableDebugLogging);
            });

        }

        /// <summary>
        /// Asynchronously calls the TestLogging RemoteMethod on the target client
        /// </summary>
        /// <param name="level">The log-level of the test log-message</param>
        public Task TestLoggingAsync(LogLevel level)
        {
            return Task.Factory.StartNew( delegate
            {
                ClientConnection.CallRemoteAction(
                    ComponentNamesExtended.XmppLoggingConfigurator,
                    XmppLoggingConfiguratorMethods.TestLogging,
                    level.ToString());
            });
        }        


        #endregion Client Implementation





        #region Server Implementation

        /// <summary>
        /// Server implementation of the SetEnable RemoteMethod
        /// </summary>
        /// <param name="isEnabled">The new value to which the Enable property is to be set</param>
        [MethodCallHandler(XmppLoggingConfiguratorMethods.SetEnable), TrustLevel(4)]
        protected CallResult SetEnable_server(bool isEnabled)
        {
            XmppLogger.Enable = isEnabled;
            return new VoidResult();
        }

        /// <summary>
        /// Server implementation of the GetEnable RemoteMethod
        /// </summary>
        [MethodCallHandler(XmppLoggingConfiguratorMethods.GetEnable), TrustLevel(4)]
        protected CallResult GetEnable_server()
        {
           return new ResponseResult(XmppLogger.Enable);
        }

        /// <summary>
        /// Server implementation of the SetRecipient RemoteMethod
        /// </summary>
        /// <param name="recipient">The new recipient to be set</param>
        [MethodCallHandler(XmppLoggingConfiguratorMethods.SetRecipient), TrustLevel(4)]
        protected CallResult SetRecipient_server(string recipient)
        {
            if (recipient.IsNullOrEmpty())
            {
                return new ErrorResult(RemoteErrorCode.InvalidParametersError);
            }
            else
            {
                XmppLogger.Recipient = recipient;
                return new VoidResult();
            }
        }

        /// <summary>
        /// Server implementation of the GetRecipient RemoteMethod
        /// </summary>
        [MethodCallHandler(XmppLoggingConfiguratorMethods.GetRecipient), TrustLevel(4)]
        protected CallResult GetRecipient_server()
        {
            return new ResponseResult(XmppLogger.Recipient);
        }

        /// <summary>
        /// Server implementation of the SetLogLevel RemoteMethod
        /// </summary>
        /// <param name="loglevel">The new loglevel to be set</param>
        [MethodCallHandler(XmppLoggingConfiguratorMethods.SetLogLevel), TrustLevel(4)]
        protected CallResult SetLogLevel_server(string loglevel)
        {
            LogLevel lvl;

            if (Enum.TryParse<LogLevel>(loglevel, out lvl))
            {
                XmppLogger.LogLevel = lvl;
                return new VoidResult();
            }
            else
            {
                return new ErrorResult(RemoteErrorCode.InvalidParametersError);
            }
        }

        /// <summary>
        /// Server implemenation of the GetLogLevel RemoteMethod
        /// </summary>
        [MethodCallHandler(XmppLoggingConfiguratorMethods.GetLogLevel), TrustLevel(4)]
        protected CallResult GetLogLevel_server()
        {
            return new ResponseResult(XmppLogger.LogLevel.ToString());
        }

        /// <summary>
        /// Server implementation of the SetEnableDebugLogging RemoteMethod
        /// </summary>
        [MethodCallHandler(XmppLoggingConfiguratorMethods.SetEnableDebugLogging), TrustLevel(4)]
        protected CallResult SetEnableDebugLogging_server(bool value)
        {
            XmppLogger.EnableDebugLogging = value;
            return new VoidResult();
        }

        /// <summary>
        /// Server implementation of the GetEnableDebugLogging RemoteMethod
        /// </summary>
        [MethodCallHandler(XmppLoggingConfiguratorMethods.GetEnableDebugLogging), TrustLevel(4)]
        protected CallResult GetEnableDebugLogging_server()
        {
            return new ResponseResult(XmppLogger.EnableDebugLogging);
        }


        /// <summary>
        /// Server implementation of the TestLogging RemoteMethod
        /// </summary>
        /// <param name="level">The log-level of the test log-message</param>
        [MethodCallHandler(XmppLoggingConfiguratorMethods.TestLogging), TrustLevel(4)]
        protected CallResult TestLogging_server(string level)
        {
            LogLevel lvl = (LogLevel)Enum.Parse(typeof(LogLevel), level);

            switch (lvl)
            {
                case LogLevel.Fatal:
                    logger.Fatal("This is a test");
                    break;
                case LogLevel.Error:
                    logger.Error("This is a test");
                    break;
                case LogLevel.Warn:
                    logger.Warn("This is a test");
                    break;
                case LogLevel.Info:
                    logger.Info("This is a test");
                    break;
            }

            return new VoidResult();
        }


        #endregion Server Implementation



    }
}
