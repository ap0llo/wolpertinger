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
using Nerdcave.Common;
using System.Threading.Tasks;

namespace Wolpertinger.Core
{
    /// <summary>
    /// Default implementation of <see cref="Wolpertinger.Core.IClientConnection" />
    /// </summary>
    public class DefaultClientConnection : IClientConnection
    {
        
        private static ILogger logger = LoggerService.GetLogger("DefaultClientConnection");

        //Timer used for connection timeouts
        private System.Timers.Timer timeoutTimer = new System.Timers.Timer(30000);
        private Dictionary<string, IComponent> clientComponents = new Dictionary<string, IComponent>();
        private Dictionary<string, IComponent> serverComponents = new Dictionary<string, IComponent>();

        private Dictionary<Guid, RemoteMethodCall> unrepliedCalls = new Dictionary<Guid, RemoteMethodCall>();

        private Dictionary<Guid, EventWaitHandle> synchronousCalls_WaitHandles = new Dictionary<Guid, EventWaitHandle>();
        private Dictionary<Guid, object> synchonousCalls_ValueCache = new Dictionary<Guid,object>();

        private int expectedResponseCount = 0;

        private IMessageProcessor _messageProcessor;




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
        /// The adress of the target-client
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
        /// The MessageProcessor used to process outgoing and incoming messages
        /// </summary>
        public IMessageProcessor MessageProcessor
        {
            get
            {
                if (_messageProcessor == null)
                    _messageProcessor = ConnectionManager.ComponentFactory.GetMessageProcessor();

                return _messageProcessor;
            }
            set
            {
                _messageProcessor = value;
            }
        }



        /// <summary>
        /// Initializes a new instance of DefaultClientConnection
        /// </summary>
        public DefaultClientConnection()
        {
            this.AcceptConnections = true;
            this.timeoutTimer.Elapsed += timeoutTimer_Elapsed;            
        }



        /// <summary>
        /// Send a message to the target client
        /// </summary>
        /// <param name="msg">The message to process and send</param>
        public void SendMessage(Message msg)
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
            ConnectionManager.SendMessage(this.Target,this.MessageProcessor.ProcessOutgoingMessage(msg));
        }

        /// <summary>
        /// Processes an incoming message (using the MessageProcessor for deserialization)
        /// </summary>
        /// <param name="message">The message to be processed</param>
        public void ProcessMessage(string message)
        {            
            //try to parse the received message
            MessageProcessingResult result;
            try
            {
                result = MessageProcessor.ProcessIncomingMessage(message);
            }
            catch (FormatException)
            {
                //the message could not be deserialzed => send RemoteError to the target client
                RemoteError error = new RemoteError(RemoteErrorCode.InvalidXmlError);
                logger.Error("Could not parse message");
                SendMessage(error);
                return;
            }

            //encryption is mandatory if TrustLevel is 2 or higher
            //if message was not encrypted it will result in a RemoteError and the connection will be reset
            if (this.TrustLevel >= 2 && !result.WasEncrypted && !(result.Message is RemoteMethodCall && (result.Message as RemoteMethodCall).MethodName == CoreMethods.SendResetNotice))
            {
                RemoteError error = new RemoteError(RemoteErrorCode.EncryptionError);
                if (result.Message.CallId != Guid.Empty)
                    error.CallId = result.Message.CallId;
                    error.TargetName = "Authentication";

                //The RemoteError will not be encrypted
                MessageProcessor.EncryptMessages = false;
                SendMessage(error);
                ResetConnection();
                return;
            }

            //get the component responsible for handling the message
            string componentName = (result.Message as Message).TargetName;
            //RemoteMethodCalls are handled by a server-component, RemoteMethodResponses by client-components
            //RemoteErrors are handled by the ClientConnection itself
            IComponent component = (result.Message is RemoteMethodCall) 
                                    ? GetServerComponent(componentName)
                                    : GetClientComponent(componentName);

            if (component == null)
            {
                //no component to handle the request was found => send a RemoteError as response and return
                RemoteError error = new RemoteError(RemoteErrorCode.ComponentNotFoundError);
                error.CallId = (result.Message as RemoteMethodCall).CallId;
                SendMessage(error);
                return;
            }


            if (result.Message is RemoteMethodCall)
            {                
                //process method calls in a new thread to prevent method calls from blocking other threads
                //ThreadPool.QueueUserWorkItem(delegate
                // {
                //     processRemoteMethodCall(result.Message as RemoteMethodCall, component);
                // });
                var processingTask = new Task(delegate
                    {
                        processRemoteMethodCall(result.Message as RemoteMethodCall, component);
                    });

                processingTask.Start();

                var heartBeatTask = new Task(delegate
                    {
                        while (!processingTask.IsCompleted)
                        {
                            Thread.Sleep(25000);
                            if(!processingTask.IsCompleted)
                                (this.GetClientComponent(ComponentNames.Core) as CoreComponent).HeartbeatAsync();
                        }
                    });

                heartBeatTask.Start();



            }
            else if (result.Message is RemoteMethodResponse)
            {
                processRemoteMethodResponse(result.Message as RemoteMethodResponse, component);
            }
            else if (result.Message is RemoteError)
            {
                processRemoteError(result.Message as RemoteError);
            }
            else 
            {
                logger.Error("ProcessMessage() encoutered an unknown type of Message");
                SendMessage(new RemoteError(RemoteErrorCode.UnknownMessage));
            }
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
                (GetClientComponent(ComponentNames.Core) as CoreComponent).SendResetNoticeAsync();
            }

            //reset TrustLevel and encryption keys
            TrustLevel = 0;
            MyTrustLevel = 0;
            MessageProcessor.EncryptMessages = false;
            MessageProcessor.EncryptionKey = null;
            MessageProcessor.EncryptionIV = null;            

            Connected = false;            

            //Raise the ConnectionReset event
            onConnectionReset();
        }

        /// <summary>
        /// Gets the connection's client component that matches the given name
        /// </summary>
        /// <param name="name">The component-name to look for</param>
        /// <returns>
        /// Returns the matching client component or null if component could not be found
        /// </returns>
        public IComponent GetClientComponent(string name)
        {
            //check if a matching component has already been initialized
            if (!clientComponents.ContainsKey(name))
            {
                //get a new component
                IComponent component = ConnectionManager.ComponentFactory.GetClientComponent(name);

                //check if a component was found
                if (component == null)
                    return null;
                
                //check the component's type
                ComponentAttribute attribute = (ComponentAttribute)component.GetType().GetCustomAttributes(typeof(ComponentAttribute), false).First();

                clientComponents.Add(name, component);

                //if the component is a Client-Server component, also add it to the list of server components
                if (ConnectionManager.ComponentFactory.IsClientServerComponent(component))
                    serverComponents.Add(name, component);
            }

            clientComponents[name].ClientConnection = this;
            //return the requested component
            return clientComponents[name];
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
            //check if a matching component has already been initialized
            if (!serverComponents.ContainsKey(name))
            {
                //get a new component
                IComponent component = ConnectionManager.ComponentFactory.GetServerComponent(name);
                
                //check if a component was found
                if (component == null)
                    return null;

                //check the component's type
                ComponentAttribute attribute = (ComponentAttribute)component.GetType().GetCustomAttributes(typeof(ComponentAttribute), false).First();
                
                if (attribute.Type == ComponentType.Server)
                {
                    serverComponents.Add(name, component);
                }
                //if component is a ClientServer component, add it to both the list of client and server components
                else if (attribute.Type == ComponentType.ClientServer)
                {
                    serverComponents.Add(name, component);
                    clientComponents.Add(name, component);                    
                }
            }

            serverComponents[name].ClientConnection = this;

            //return the requested component
            return serverComponents[name];
        }

        /// <summary>
        /// Gets a new EventWaitHandle that will be signaled once a response matching the given Id is received or the connection times out.
        /// </summary>
        /// <param name="callId">The CallId of the response to wait for</param>
        /// <returns>
        /// Returns a new EventWaitHandle
        /// </returns>
        public EventWaitHandle GetWaitHandle(Guid callId)
        {
            if (!synchronousCalls_WaitHandles.ContainsKey(callId))
                synchronousCalls_WaitHandles.Add(callId, new EventWaitHandle(false, EventResetMode.ManualReset));

            return synchronousCalls_WaitHandles[callId];
        }

        /// <summary>
        /// Calls the remote specfied remote method and waits until a reponse is received.
        /// </summary>
        /// <param name="call">The remote method to call</param>
        /// <returns>
        /// Returns the value returned by the remote method call or a RemoteErrorException if the method call returned an error.
        /// If the request timed out, throws a TimeoutException
        /// </returns>
        public object GetReponseValueBlocking(RemoteMethodCall call)
        {            
            //send the RemoteMethodCall
            SendMessage(call);

            //wait for the response using a EventWaitHandle
            GetWaitHandle(call.CallId).WaitOne();

            //get the returned value from the cache
            object value = synchonousCalls_ValueCache[call.CallId];
            
            //remove the value from value cache
            synchonousCalls_ValueCache.Remove(call.CallId);

            if (value is TimeoutException)
            {
                throw (value as TimeoutException);
            }
            else
            {
                return value;
            }

        }

        /// <summary>
        /// Gets a <see cref="Wolpertinger.Core.ClientInfo" /> abouts the connection's target client
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



        #region Event Handlers

        private void timeoutTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //do nothing if no respones are expected
            if (expectedResponseCount == 0)
            {
                return;
            }

#if DEBUG
            logger.Info("timeoutTimer.Elapsed: ExpectedResponseCount: {0}", expectedResponseCount);
#endif

            timeoutTimer.Stop();
            //release all threads waiting for a response (they probably won't get one)
            //the "response value" is a TimeoutException (will cause the "GetReponseValueBlocking" meethod to throw that exception
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

        protected virtual void onConnectionTimedOut()
        {
            if (this.ConnectionTimedOut != null)
            {
                this.ConnectionTimedOut(this, EventArgs.Empty);
            }
        }

        protected virtual void onConnectionReset()
        {
            if (this.ConnectionReset != null)
            {
                this.ConnectionReset(this, EventArgs.Empty);
            }
        }

        protected virtual void onRemoteErrorOccurred(RemoteError err)        
        {
            if (this.RemoteErrorOccurred != null)
                this.RemoteErrorOccurred(this, err);
        }

        #endregion Event Raisers

        /// <summary>
        /// Tries to retrive a <see cref="CallResult"/> from the specified component for the speicified RemoteMethodCall using Reflection
        /// </summary>
        /// <param name="call">The RemoteMethodCall that needs to be answered</param>
        /// <param name="component">The component the RemoteMethodCall targeted</param>
        /// <returns>Returns the <see cref="CallResult"/> for the specified RemoteMethodCall</returns>
        protected virtual CallResult getCallResult(RemoteMethodCall call, IComponent component)
        {
            //search the component for a method taht can handle the call
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
                    //if no attribute could be found, fall back to the default (highest possible trustlevel)
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
                SendMessage(
                    new RemoteError((callResult as ErrorResult).ErrorCode)
                        {
                            CallId = call.CallId,
                            TargetName = call.TargetName
                        }
                    );
            }
            else if (callResult is ResponseResult)
            {
                //result was a response => send RemoteMethodResponse
                RemoteMethodResponse response = new RemoteMethodResponse();
                response.CallId = call.CallId;
                response.TargetName = call.TargetName;
                response.ResponseValue = (callResult as ResponseResult).ResponseValue;

                SendMessage(response);
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
        /// <param name="component">The component responsible for the response</param>
        protected virtual void processRemoteMethodResponse(RemoteMethodResponse response, IComponent component)
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
                //pass ansynchronous calls to the client component that made the call
                else
                {
                    //get methods that handle the response
                    var methodInfos = component.GetType()
                                        .GetMethods(BindingFlags.NonPublic| BindingFlags.Instance | BindingFlags.Public)
                                        .Where(x => x.GetCustomAttributes(typeof(ResponseHandlerAttribute), true)
                                            .Any(y => (y as ResponseHandlerAttribute).MethodName == call.MethodName));
                    
                    if(methodInfos.Any())
                    {
                        methodInfos.First().Invoke(component, new object[] { response.ResponseValue });
                    }

                }

                //remove call from unreplied calls
                unrepliedCalls.Remove(response.CallId);

                //decrease the number of repsonses expected from the target client (connection will not time out when no responses are expected
                if (call.ResponseExpected)
                    this.expectedResponseCount--;

                //if there are still call taht are unreplied, restart the timeout-timer
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
