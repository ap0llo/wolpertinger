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
using System.Threading;
using System.Reflection;
using Slf;
using System.Xml;
using Nerdcave.Common;
using Nerdcave.Common.Extensions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Wolpertinger.Core
{
    /// <summary>
    /// Default implementation of <see cref="Wolpertinger.Core.IClientConnection" />
    /// </summary>
    public class DefaultClientConnection : IClientConnection
    {
        private static ILogger logger = LoggerService.GetLogger("DefaultClientConnection");

        private IWtlpClient _wtlpClient;

        private System.Timers.Timer timeoutTimer = new System.Timers.Timer(30000);
        private Dictionary<string, IComponent> clientComponents = new Dictionary<string, IComponent>();
        private Dictionary<string, IComponent> serverComponents = new Dictionary<string, IComponent>();

        private Dictionary<Guid, RemoteMethodCall> unrepliedCalls = new Dictionary<Guid, RemoteMethodCall>();

        private Dictionary<Guid, EventWaitHandle> synchronousCalls_WaitHandles = new Dictionary<Guid, EventWaitHandle>();
        private Dictionary<Guid, object> synchonousCalls_ValueCache = new Dictionary<Guid,object>();

        private int expectedResponseCount = 0;



        /// <summary>
        /// Event that will be raised when the connection to the target client has timed out
        /// </summary>
        public event EventHandler ConnectionTimedOut;

        /// <summary>
        /// Event that will be raised when the connection has been reset (either by this client or the target client)
        /// </summary>
        public event EventHandler ConnectionReset;

        /// <summary>
        /// Event that will be raised if a remote error has been received
        /// </summary>
        public event EventHandler<ObjectEventArgs<RemoteError>> RemoteErrorOccurred;


        /// <summary>
        /// Specifies whether incoming connection request should be accepted
        /// </summary>
        /// <remarks>
        /// Default value is true
        /// </remarks>
        public bool AcceptConnections { get; set; }


        /// <summary>
        /// The address of the target-client
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// The level of trust the target client has
        /// </summary>
        public int TrustLevel { get; set; }

        /// <summary>
        /// The level of trust in this client has on the target client
        /// </summary>
        public int MyTrustLevel { get; set; }

        /// <summary>
        /// Indicates whether the connection to the target client is open
        /// </summary>
        public bool Connected { get; set; }

        /// <summary>
        /// The connection Manager used to send and receive messages
        /// </summary>
        public IConnectionManager ConnectionManager { get; set; }

        /// <summary>
        /// Gets or sets the WtlpClient used by the connection for communication
        /// </summary>
        public IWtlpClient WtlpClient 
        {
            get 
            {
                lock (this)
                {
                    return _wtlpClient; 
                }
            }
            set
            {
                lock (this)
                {
                    if (_wtlpClient != null)
                    {
                        _wtlpClient.MessageReceived -= wtlpClient_MessageReceived;
                        _wtlpClient.MessageFragmentReceived -= _wtlpClient_MessageFragmentReceived;
                    }

                    _wtlpClient = value;

                    if (_wtlpClient != null)
                    {
                        _wtlpClient.MessageReceived += wtlpClient_MessageReceived;
                        _wtlpClient.MessageFragmentReceived += _wtlpClient_MessageFragmentReceived;
                    }
                }
            }
        }        

        /// <summary>
        /// Gets or sets the ComponentFactory used by the ClientConnection to instantiate new components
        /// </summary>
        public IComponentFactory ComponentFactory { get; set; }
        


        /// <summary>
        /// Initializes a new instance of DefaultClientConnection
        /// </summary>
        public DefaultClientConnection()
        {
            this.ComponentFactory = new DefaultComponentFactory();
            this.AcceptConnections = true;
            this.timeoutTimer.Elapsed += timeoutTimer_Elapsed;
            
        }








        /// <summary>
        /// Tells the client connection the target client is still sending data and prevents the connection from timing out
        /// </summary>
        public void Hearbeat()
        {
            //Reset the timeout-timer
            timeoutTimer.Stop();
            timeoutTimer.Start();
        }

        /// <summary>
        /// Resets the client connection
        /// </summary>
        /// <param name="sendNotice">Indicates whether the target client should be notified about the connection reset</param>
        public void ResetConnection(bool sendNotice=false)
        {

            //if specified, notify the target client that the connection has been reset
            if (sendNotice)
            {
                var task = (new CoreClientComponent() { ClientConnection = this }).SendResetNoticeAsync();
                task.Wait();
            }

            //reset TrustLevel and encryption keys
            TrustLevel = 0;
            MyTrustLevel = 0;
            WtlpClient.EncryptMessages = false;
            WtlpClient.EncryptionKey = null;
            WtlpClient.EncryptionIV = null;

            WtlpClient.Detach();
            WtlpClient = null;

            expectedResponseCount = 0;

            Connected = false;            

            //Raise the ConnectionReset event
            onConnectionReset();
        }


        /// <summary>
        /// Gets the connection's server component that matches the given name
        /// </summary>
        /// <param name="name">The component-name to look for</param>
        /// <returns>
        /// Returns the matching server component or null if component could not be found
        /// </returns>
        public IComponent GetServerComponent(string name)
        {
            lock (this)
            {
                //check if a matching component has already been initialized
                if (!serverComponents.ContainsKey(name.ToLower()))
                {
                    //get a new component
                    IComponent component = ComponentFactory.GetServerComponent(name);
                
                    //check if a component was found
                    if (component == null)
                        return null;

                    serverComponents.Add(name.ToLower(), component);
                }

                serverComponents[name.ToLower()].ClientConnection = this;

                //return the requested component
                return serverComponents[name.ToLower()];
            }
        }




        /// <summary>
        /// Gets a <see cref="Wolpertinger.Core.ClientInfo" /> about the connection's target client
        /// </summary>
        /// <returns>
        /// Returns a new ClientInfo object with information about the target client
        /// </returns>
        public ClientInfo GetClientInfo()
        {
            return new ClientInfo() 
            { 
                    JId = this.Target, 
                    Profiles = new List<Profile>(), 
                    ProtocolVersion = 0, 
                    TrustLevel = this.TrustLevel 
            }; 
        }


        /// <summary>
        /// Calls the specified remote method
        /// </summary>
        /// <param name="component">The remote-method's component name</param>
        /// <param name="name">The name of the method to call</param>
        /// <param name="args">The parameters to pass to the method</param>
        public void CallRemoteAction(string component, string name, params object[] args)
        {
            invokeRemoteMethod(component, name, args, false);
        }

        /// <summary>
        /// Calls the specified remote method and returns it's return value
        /// </summary>
        /// <param name="component">The remote-method's component name</param>
        /// <param name="name">The name of the method to call</param>
        /// <param name="args">The parameters to pass to the method</param>
        /// <returns>Returns the value returned by the remote method</returns>
        public object CallRemoteFunction(string component, string name, params object[] args)
        {
            return invokeRemoteMethod(component, name, args, true);
        }


        


        #region Event Handlers

        private void wtlpClient_MessageReceived(object sender, ObjectEventArgs<ParsingResult> e)
        {
            if (!e.Handled)
            {
                processMessage(e.Value);
                e.Handled = true;
            }
        }

        private void _wtlpClient_MessageFragmentReceived(object sender, EventArgs e)
        {
            Hearbeat();
        }

        private void timeoutTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //do nothing if no responses are expected
            if (expectedResponseCount == 0)
            {
                return;
            }

            timeoutTimer.Stop();
            //release all threads waiting for a response (they probably won't get one)
            //the "response value" is a TimeoutException (will cause the "GetReponseValueBlocking" method to throw that exception
            foreach (Guid item in synchronousCalls_WaitHandles.Keys)
            {
                synchonousCalls_ValueCache.Add(item, new TimeoutException());
                synchronousCalls_WaitHandles[item].Set();
            }

            //clear all wait handles
            synchronousCalls_WaitHandles.Clear();

            //raise the ConnectionTimedOut event
            onConnectionTimedOut();
        }

        #endregion Event Handlers


        #region Event Raisers

        /// <summary>
        /// Raises the ConnectionTimedOut event
        /// </summary>
        protected virtual void onConnectionTimedOut()
        {
            if (this.ConnectionTimedOut != null)
            {
                this.ConnectionTimedOut(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Raises the ConnectionReset event
        /// </summary>
        protected virtual void onConnectionReset()
        {
            if (this.ConnectionReset != null)
            {
                this.ConnectionReset(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Raises the RemoteErrorOccurred event
        /// </summary>
        /// <param name="err"></param>
        protected virtual void onRemoteErrorOccurred(RemoteError err)
        {
            if (this.RemoteErrorOccurred != null)
                this.RemoteErrorOccurred(this, err);
        }

        #endregion Event Raisers



        protected object invokeRemoteMethod(string component, string name, object[] args, bool responseExpected)
        {
            var call = new RemoteMethodCall() { ComponentName = component, MethodName = name, Parameters = args.ToList<object>(), ResponseExpected = responseExpected };

            //send the RemoteMethodCall
            sendMessage(call);

            //if a response is expected, wait for the recipient to send a response
            if (responseExpected)
            {
                //wait for the response using a EventWaitHandle
                if (!synchronousCalls_WaitHandles.ContainsKey(call.CallId))
                {
                    synchronousCalls_WaitHandles.Add(call.CallId, new EventWaitHandle(false, EventResetMode.ManualReset));
                }

                synchronousCalls_WaitHandles[call.CallId].WaitOne();

                //get the returned value from the cache
                object value = synchonousCalls_ValueCache[call.CallId];

                //remove the value from value cache
                synchonousCalls_ValueCache.Remove(call.CallId);

                if (value is TimeoutException)
                {
                    throw (value as TimeoutException);
                }
                else if (value is RemoteError)
                {
                    throw new RemoteErrorException(value as RemoteError);
                }
                else
                {
                    return value;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Send a message to the target client
        /// </summary>
        /// <param name="msg">The message to process and send</param>
        protected virtual void sendMessage(RpcMessage msg)
        {
            //if message is a RemoteMethodCall, it will be cached to be able to process response messages
            if (msg is RemoteMethodCall)
            {
                RemoteMethodCall call = msg as RemoteMethodCall;
                if (call.ResponseExpected)
                {
                    unrepliedCalls.Add(call.CallId, call);
                    expectedResponseCount++;
                    timeoutTimer.Start();
                }
            }

            //process the message using the MessageProcessor and pass the result to the ConnectionManager to send
            try
            {
                this.WtlpClient.Send(msg.Serialize().ToString().GetBytesUTF8());
            }
            catch (WtlpException ex)
            {
                if (ex.Error == Result.Timeout)
                    throw new TimeoutException();
                else
                   throw new RemoteErrorException();
            }
        }


        /// <summary>
        /// Processes an incoming message (using the MessageProcessor for de-serialization)
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void processMessage(ParsingResult message)
        {
            //parse the message
            XElement xmlMessage;
            RpcMessage msg = null;
            var body = message.Payload.ToStringUTF8();            
            try
            {
                //try to parse it as XML
                xmlMessage = XElement.Parse(body);
                                     
                switch (xmlMessage.Name.LocalName.ToLower())
                {
                    case "remotemethodcall":
                        msg = new RemoteMethodCall();
                        break;
                    case "remotemethodresponse":
                        msg = new RemoteMethodResponse();
                        break;
                    case "remoteerror":
                        msg = new RemoteError();
                        break;
                }

                if (!msg.Validate(xmlMessage))
                {
                    logger.Error("Message from {0} could not be validated", this.Target);
                    var error = new RemoteError(RemoteErrorCode.InvalidXmlError);
                    sendMessage(error);
                    return;
                }

                msg.Deserialize(xmlMessage);

            }
            //Catch XmlException and wrap it into a format exception (makes catching errors in the caller easier)
            catch (XmlException)
            {
                logger.Error("Could not parse message received from {0}", this.Target);
                RemoteError error = new RemoteError(RemoteErrorCode.InvalidXmlError);
                sendMessage(error);
                return;
            }

            

            
            //encryption is mandatory if TrustLevel is 2 or higher
            //if message was not encrypted it will result in a RemoteError and the connection will be reset
            if (this.TrustLevel >= 2 && !message.WasEncrypted && !(msg is RemoteMethodCall && (msg as RemoteMethodCall).MethodName == CoreMethods.SendResetNotice))
            {
                RemoteError error = new RemoteError(RemoteErrorCode.EncryptionError);
                if (msg.CallId != Guid.Empty)
                    error.CallId = msg.CallId;
                error.ComponentName = "Authentication";

                //The RemoteError will not be encrypted
                WtlpClient.EncryptMessages = false;
                sendMessage(error);
                ResetConnection();
                return;
            }



            if (msg is RemoteMethodCall)
            {
                //get the component responsible for handling the message
                string componentName = (msg as RpcMessage).ComponentName;
                var component = GetServerComponent(componentName);

                if (component == null)
                {
                    //no component to handle the request was found => send a RemoteError as response and return
                    RemoteError error = new RemoteError(RemoteErrorCode.ComponentNotFoundError);
                    error.CallId = (msg as RemoteMethodCall).CallId;
                    sendMessage(error);
                    return;
                }

                var processingTask = new Task(delegate
                    {
                        processRemoteMethodCall(msg as RemoteMethodCall, component);
                    });

                processingTask.Start();

                var heartBeatTask = new Task(delegate
                    {
                        var coreComponent = new CoreClientComponent();
                        coreComponent.ClientConnection = this;
                        while (!processingTask.IsCompleted)
                        {
                            Thread.Sleep(25000);
                            if (!processingTask.IsCompleted)
                                coreComponent.HeartbeatAsync();
                        }
                    });

                heartBeatTask.Start();
            }
            else if (msg is RemoteMethodResponse)
            {
                processRemoteMethodResponse(msg as RemoteMethodResponse);
            }
            else if (msg is RemoteError)
            {
                processRemoteError(msg as RemoteError);
            }
            else
            {
                logger.Error("ProcessMessage() encountered an unknown type of Message");
                sendMessage(new RemoteError(RemoteErrorCode.UnknownMessage));
            }
        }

        /// <summary>
        /// Tries to retrieve a <see cref="CallResult"/> from the specified component for the specified RemoteMethodCall using Reflection
        /// </summary>
        /// <param name="call">The RemoteMethodCall that needs to be answered</param>
        /// <param name="component">The component the RemoteMethodCall targeted</param>
        /// <returns>Returns the <see cref="CallResult"/> for the specified RemoteMethodCall</returns>
        protected virtual CallResult getCallResult(RemoteMethodCall call, IComponent component)
        {
            //search the component for a method that can handle the call
            Dictionary<string, MethodInfo> callHandlers = new Dictionary<string, MethodInfo>();
            foreach (MethodInfo mi in component.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public))
            {
                object[] attributes = mi.GetCustomAttributes(typeof(MethodCallHandlerAttribute), true);
                foreach (MethodCallHandlerAttribute attr in attributes)
                {
                    if (!callHandlers.ContainsKey(attr.MethodName))
                    {
                        callHandlers.Add(attr.MethodName, mi);
                    }
                }
            }    


            if (callHandlers.ContainsKey(call.MethodName))
            {                
                var attributes = callHandlers[call.MethodName].GetCustomAttributes(typeof(TrustLevelAttribute), true);                

                //get the trust level from the method's TrustLevel attribute
                TrustLevelAttribute trustLevel;
                if (attributes.Any())                
                    trustLevel = (TrustLevelAttribute)attributes.First();
                else                
                    //if no attribute could be found, fall back to the default (highest possible trust-level)
                    trustLevel = TrustLevelAttribute.MAX;

                //check if the sender is authorized to make that method call
                if (trustLevel.RequiredTrustLevel > this.TrustLevel)
                {
                    return new ErrorResult(RemoteErrorCode.NotAuthorizedError);
                }
                else
                {
                    //invoke the call handler method and return it's result
                    try
                    {
                        return (CallResult)callHandlers[call.MethodName].Invoke(component, call.Parameters.ToArray<object>());
                    }
                    catch (Exception e)
                    {
                        if (e.GetType() == typeof(ArgumentException) || e.GetType() == typeof(TargetParameterCountException))
                        {
                            return new ErrorResult(RemoteErrorCode.InvalidParametersError);
                        }
                        else { throw; }
                    }
                }
            }
            //Invoked method could not be found => return ErrorResult
            else
            {
                return new ErrorResult(RemoteErrorCode.MethodNotFoundError);
                
            }
        }

        /// <summary>
        /// Processes an incoming RemoteMethodCall
        /// </summary>
        /// <param name="call">The RemoteMethodCall that is to be processed</param>
        /// <param name="component">The component that will handle the call</param>
        protected virtual void processRemoteMethodCall(RemoteMethodCall call, IComponent component)
        {
            //get the call's result
            CallResult callResult = getCallResult(call, component);

            if (callResult is ErrorResult)
            {
                //result was error => send RemoteError
                sendMessage(
                    new RemoteError((callResult as ErrorResult).ErrorCode)
                        {
                            CallId = call.CallId,
                            ComponentName = call.ComponentName
                        }
                    );
            }
            else if (callResult is ResponseResult)
            {
                //result was a response => send RemoteMethodResponse
                RemoteMethodResponse response = new RemoteMethodResponse();
                response.CallId = call.CallId;
                response.ComponentName = call.ComponentName;
                response.ResponseValue = (callResult as ResponseResult).ResponseValue;

                sendMessage(response);
            }
            else if (callResult is VoidResult)
            {
                //result was VoidResult => do nothing
            }
            else
            {
                //should not happen, ResponseResult, ErrorResult and VoidResult are the only known subclasses of CallResult the
                logger.Error("ProcessMessage() encountered an unknown type of CallResult");
            }

            //If a post-processing action has been specified in the call, invoke it now
            if (callResult.PostProcessingAction != null)
            {
                callResult.PostProcessingAction.Invoke();
            }

        }

        /// <summary>
        /// Processes an incoming RemoteMethodResponse
        /// </summary>
        /// <param name="response">The RemoteMethodResponse that is to be processed</param>
        protected virtual void processRemoteMethodResponse(RemoteMethodResponse response)
        {
            //get the RemoteMethodCall that triggered the response from the cache
            RemoteMethodCall call = unrepliedCalls.ContainsKey(response.CallId) ? unrepliedCalls[response.CallId] : null;

            //response is only processed, when the associated method call could be found
            if (call != null)
            {
                //stop the connection from timing out as we just received a message
                timeoutTimer.Stop();

                //if call was synchronous => put the response value in the value cache and release wait handle
                if (synchronousCalls_WaitHandles.ContainsKey(call.CallId))
                {
                    synchonousCalls_ValueCache.Add(call.CallId, response.ResponseValue);
                    synchronousCalls_WaitHandles[call.CallId].Set();
                }
           
                //remove call from unreplied calls
                unrepliedCalls.Remove(response.CallId);

                //decrease the number of responses expected from the target client (connection will not time out when no responses are expected
                if (call.ResponseExpected)
                    this.expectedResponseCount--;

                //if there are still call that are unreplied, restart the timeout-timer
                if (unrepliedCalls.Any())
                {
                    timeoutTimer.Start();
                }
            }
        }

        /// <summary>
        /// Processes an RemoteError
        /// </summary>
        /// <param name="error">The RemoteError that is to be processed</param>
        protected virtual void processRemoteError(RemoteError error)
        {
            //if error was caused by synchronous method call, treat it as method response (thread will be blocked indefinitely otherwise)
            if (error.CallId != null && error.CallId != Guid.Empty && synchronousCalls_WaitHandles.ContainsKey(error.CallId))
            {
                synchonousCalls_ValueCache.Add(error.CallId, error);
                synchronousCalls_WaitHandles[error.CallId].Set();

                //remove original call from unreplied calls, if it exists
                if (unrepliedCalls.ContainsKey(error.CallId))
                {                    
                    //if response for the call was expected, decrease the number of expected responses to prevent the connection from timing out
                    if(unrepliedCalls[error.CallId].ResponseExpected)
                        expectedResponseCount--;

                    unrepliedCalls.Remove(error.CallId);
                }

            }

            //raise the RemoteErrorOccurred event
            onRemoteErrorOccurred(error);

        }
        

    }
}
