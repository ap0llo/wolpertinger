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
using Nerdcave.Common;
using Nerdcave.Common.Extensions;
using Slf;

namespace Wolpertinger.Core
{
    /// <summary>
    /// Combined server- and client-implementation of the XmppLoggingConfigurator component
    /// See Wolpertinger API Documentation for details on the component.
    /// </summary>
    [Component(ComponentNamesExtended.XmppLoggingConfigurator, ComponentType.ClientServer)]
    public class XmppLoggingConfiguratorComponent : ClientComponent
    {

        private ILogger logger = LoggerService.GetLogger("XmppLoggingConfigurator");


        #region Events

        /// <summary>
        /// Occurs when a response to a GetEnable request is received
        /// </summary>
        public event EventHandler<ObjectEventArgs<bool>> GetEnableCompleted;
        /// <summary>
        /// Occurs when a response to a GetRecipient request is received
        /// </summary>
        public event EventHandler<ObjectEventArgs<string>> GetRecipientCompleted;
        /// <summary>
        /// Occurs when a response to a GetLogLebel request is received
        /// </summary>
        public event EventHandler<ObjectEventArgs> GetLogLevelCompleted;
        /// <summary>
        /// Occurs when a response to a GetEnableDebugLogging request is received
        /// </summary>
        public event EventHandler<ObjectEventArgs<bool>> GetEnableDebugLoggingCompleted;

        #endregion


    
        #region Client Implementation

        /// <summary>
        /// Asynchronously calls the SetEnable RemoteMethod on the target client
        /// </summary>
        /// <param name="isEnabled">The new enabled-state to be set</param>
        public void SetEnableAsync(bool isEnabled)
        {
            callRemoteMethodAsync(XmppLoggingConfiguratorMethods.SetEnable, false, isEnabled);
        }

        /// <summary>
        /// Asynchrounously calls the GetEnable RemoteMethod on the target client and
        /// raises the GetEnableCompleted event when a response is received.
        /// </summary>
        public void GetEnableAsync()
        {
            callRemoteMethodAsync(XmppLoggingConfiguratorMethods.GetEnable, true);
        }

        /// <summary>
        /// Synchronously calls the GetEnable RemoteMethod on the target client and
        /// waits until a response is received
        /// </summary>
        /// <returns>Returns the response value once it is received (Blocking)</returns>
        public bool GetEnable()
        {
            return (bool)callRemoteMethod(XmppLoggingConfiguratorMethods.GetEnable);
        }

        /// <summary>
        /// Asynchronously calls the SetRecipient RemoteMehod on the target client
        /// </summary>
        /// <param name="recipient">The new value to set</param>
        public void SetRecipientAsync(string recipient)
        {
            callRemoteMethodAsync(XmppLoggingConfiguratorMethods.SetRecipient, false, recipient);
        }

        /// <summary>
        /// Asynchronously calls the GetRecipient RemoteMethod on the target client and
        /// raises the GetRecipientCompleted event when a response is received
        /// </summary>
        public void GetRecipientAsync()
        {
            callRemoteMethodAsync(XmppLoggingConfiguratorMethods.GetRecipient, true);
        }

        /// <summary>
        /// Synchronously calls the GetRecipient RemoteMethod on the target client and
        /// returns the response value when a response is received.
        /// </summary>
        /// <returns>Returns the reponse value once it is received (Blocking)</returns>
        public string GetRecipient()
        {
            return (string)callRemoteMethod(XmppLoggingConfiguratorMethods.GetRecipient);
        }

        /// <summary>
        /// Asynchronously calls the SetLogLevel RemoteMethod on the target client.
        /// </summary>
        /// <param name="level">The new log-level to set</param>
        public void SetLogLevelAsync(LogLevel level)
        {
            callRemoteMethodAsync(XmppLoggingConfiguratorMethods.SetLogLevel, false, level.ToString());
        }

        /// <summary>
        /// Asynchronously calls the GetLogLevel RemoteMethod on the target client and 
        /// raises the GetLogLevelCompleted event when a response is received
        /// </summary>
        public void GetLogLevelAsync()
        {
            callRemoteMethodAsync(XmppLoggingConfiguratorMethods.GetLogLevel, true);
        }

        /// <summary>
        /// Synchronously calls the GetLogLevel RemoteMethod on the target client and 
        /// returns the response value when it is received.
        /// </summary>
        /// <returns>Returns the target client's loglevel setting once it is received (Blocking)</returns>
        public LogLevel GetLoglevel()
        {
            string level = (string)callRemoteMethod(XmppLoggingConfiguratorMethods.GetLogLevel);
            LogLevel logLevel;
            return Enum.TryParse<LogLevel>(level, out logLevel) ? logLevel : LogLevel.None;
        }

        /// <summary>
        /// Asynchronously calls the SetEnableDebugLogging RemoteMethod on the target client
        /// </summary>
        /// <param name="value">The new value to set</param>
        public void SetEnableDebugLoggingAsync(bool value)
        {
            callRemoteMethodAsync(XmppLoggingConfiguratorMethods.SetEnableDebugLogging, false, value);
        }

        /// <summary>
        /// Asynchronously calls the GetEnableDebugLogging RemoteMethod on the target client and
        /// raises the GetEnableDebugLoggingCompleted event when a response is received.
        /// </summary>
        public void GetEnableDebugLoggingAsync()
        {
            callRemoteMethodAsync(XmppLoggingConfiguratorMethods.GetEnableDebugLogging, true);
        }

        /// <summary>
        /// Synchronously calls the GetEnableDebugLogging RemoteMethod on the target client and 
        /// returns the response value when it is received.
        /// </summary>
        /// <returns>Returns the response value once it is received (Blocking)</returns>
        public bool GetEnableDebugLogging()
        {
            return (bool)callRemoteMethod(XmppLoggingConfiguratorMethods.GetEnableDebugLogging);
        }

        /// <summary>
        /// Asynchronously calls the TestLogging RemoteMethod on the target client
        /// </summary>
        /// <param name="level">The log-level of the test log-message</param>
        public void TestLoggingAsync(LogLevel level)
        {
            callRemoteMethodAsync(XmppLoggingConfiguratorMethods.TestLogging, false, level.ToString());
        }        


        /// <summary>
        /// Response handler for the GetEnable RemoteMethod
        /// </summary>
        /// <param name="value">The value returned by the target client</param>
        [ResponseHandler(XmppLoggingConfiguratorMethods.GetEnable)]
        protected void responseHandlerGetEnable(bool value)
        {
            if (this.GetEnableCompleted != null)
            {
                this.GetEnableCompleted(this, value);
            }
        }

        /// <summary>
        /// Response handler for the GetRecipient RemoteMethod 
        /// </summary>
        /// <param name="value">The recipient returned by the target client</param>
        [ResponseHandler(XmppLoggingConfiguratorMethods.GetRecipient)]
        protected void responseHandlerGetRecipient(string value)
        {
            if (!value.IsNullOrEmpty() && this.GetRecipientCompleted != null)
            {
                this.GetRecipientCompleted(this, value);
            }
        }

        /// <summary>
        /// Response handler for the GetLogLevel RemoteMethod
        /// </summary>
        /// <param name="value">The log-level returned by the target client</param>
        [ResponseHandler(XmppLoggingConfiguratorMethods.GetLogLevel)]
        protected void responseHandlerGetLogLevel(string value)
        {
            LogLevel level;
            if (!value.IsNullOrEmpty() && Enum.TryParse(value, out level) && this.GetLogLevelCompleted != null)
                this.GetLogLevelCompleted(this, new ObjectEventArgs(level));
            else 
                logger.Error("GetLogLevel(): Received invalid response");
        }

        /// <summary>
        /// Response handler for the GetEnableDebugLogging RemoteMethod
        /// </summary>
        /// <param name="value">The value returned by the target client</param>
        [ResponseHandler(XmppLoggingConfiguratorMethods.GetEnableDebugLogging)]
        protected void responseHandlerGetEnableDebugLogging(bool value)
        {
            if (this.GetEnableDebugLoggingCompleted != null)
                this.GetEnableDebugLoggingCompleted(this, value);
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
