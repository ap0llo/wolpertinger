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
using Nerdcave.Common;

namespace Wolpertinger.Core
{
    public interface IClientConnection
    {
        /// <summary>
        /// Event that will be raised when the connection to the target client has timed out
        /// </summary>
        event EventHandler ConnectionTimedOut;

        /// <summary>
        /// Event that will be raised when the connection has been reset (either by this client or the target client)
        /// </summary>
        event EventHandler ConnectionReset;

        /// <summary>
        /// Event that will be raised if a remote error shoud has received
        /// </summary>
        event EventHandler<ObjectEventArgs<RemoteError>> RemoteErrorOccurred;



        /// <summary>
        /// Specifies whether incoming connection request should be accepted or not
        /// </summary>
        /// <remarks>Should be initialized with true</remarks>
        bool AcceptConnections { get; set; }

        /// <summary>
        /// The adress of the target-client
        /// </summary>
        string Target { get; set; }

        /// <summary>
        /// The level of trust the target client has
        /// </summary>
        int TrustLevel { get; set; }

        /// <summary>
        /// The level of trust in this client has on the target client
        /// </summary>
        int MyTrustLevel {get; set;}

        /// <summary>
        /// Indicates whether the connection to the target client is open
        /// </summary>
        bool Connected { get; set; }

        /// <summary>
        /// Gets (or sets) The ConnectionManager that hosts the ClientConnection. 
        /// Will be set automatically by ConnectionManager. Should be accessed readonly otherwise.
        /// </summary>
        IConnectionManager ConnectionManager { get; set; }

        /// <summary>
        /// Gets or sets the WtlpClient used by the connection for communication
        /// </summary>
        IWtlpClient WtlpClient { get; set; }

        /// <summary>
        /// Gets or sets the ComponentFactory used by the ClientConnection to instantiate new components
        /// </summary>
        IComponentFactory ComponentFactory { get; set; }
        


        ///// <summary>
        ///// Send a message to the target client
        ///// </summary>
        ///// <param name="msg">The message to process and send</param>
        //void SendMessage(RpcMessage msg);
        

        /// <summary>
        /// Gets a <see cref="Wolpertinger.Core.ClientInfo"/> abouts the connection's target client
        /// </summary>
        /// <returns>Returns a new ClientInfo object with information about the target client</returns>
        ClientInfo GetClientInfo();

        /// <summary>
        /// Tells the client connection the target client is still sending data and prevents the connection from timing out
        /// </summary>
        void Hearbeat();

        /// <summary>
        /// Resets the client connection
        /// </summary>
        /// <param name="sendNotice">Indicates whether the target client should be notified about the connection reset</param>
        void ResetConnection(bool sendNotice = false);



        ///// <summary>
        ///// Gets the connection's client component that matches the given name
        ///// </summary>
        ///// <param name="name">The component-name to look for</param>
        ///// <returns>Returns the matching client component or null if component could not be found</returns>
        //IComponent GetClientComponent(string name);

        /// <summary>
        /// Gets the connection's server component that matches the given name
        /// </summary>
        /// <param name="name">The component-name to look for</param>
        /// <returns>Returns the matching server component or null if component could not be found</returns>
        IComponent GetServerComponent(string name);


        ///// <summary>
        ///// Gets a new EventWaitHandle that will be signaled once a response matching the given Id is received or the connection times out.
        ///// </summary>
        ///// <param name="callId">The CallId of the response to wait for</param>
        ///// <returns>Returns a new EventWaitHandle</returns>
        //EventWaitHandle GetWaitHandle(Guid callId);

        ///// <summary>
        ///// Calls the remote specfied remote method and waits until a reponse is received.
        ///// </summary>
        ///// <param name="call">The remote method to call</param>
        ///// <returns>
        ///// Returns the value returned by the remote method call or a RemoteErrorException if the method call returned an error.
        ///// If the request timed out, throws a TimeoutException
        ///// </returns>
        //object GetReponseValueBlocking(RemoteMethodCall call);

        /// <summary>
        /// Calls the specified remote method
        /// </summary>
        /// <param name="name">The name of the method to call</param>
        /// <param name="args">The parameters to pass to the method</param>
        void CallRemoteAction(string component, string name, params object[] args);

        /// <summary>
        /// Calls the specified remote method and returns it's return value
        /// </summary>
        /// <param name="name">The name of the method to call</param>
        /// <param name="args">The parameters to pass to the method</param>
        /// <returns>Returns the value returned by the remote method</returns>
        object CallRemoteFunction(string component, string name, params object[] args);


    }
}
