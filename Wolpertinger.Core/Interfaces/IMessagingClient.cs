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

namespace Wolpertinger.Core
{
    /// <summary>
    /// A client for a messaging protocol (for example XMPP)
    /// </summary>
    public interface IMessagingClient
    {
        /// <summary>
        /// Is raised every time a message is received
        /// </summary>
        event EventHandler<ObjectEventArgs<Message>> MessageReceived;

        /// <summary>
        /// Is raised when the value of the 'Connected' property has changed
        /// </summary>
        event EventHandler ConnectedChanged;

        /// <summary>
        /// Is raised when a peer is no longer available
        /// </summary>
        event EventHandler<ObjectEventArgs<string>> PeerDisconnected;



        /// <summary>
        /// The address of the server to connect to
        /// </summary>
        string Server { get; set; }

        /// <summary>
        /// The username for connecting to the server
        /// </summary>
        string Username { get; set; }

        /// <summary>
        /// The resource gives details on the clients connection. This could be a XMPP resource, a network port or similar
        /// </summary>
        string Resource { get; set; }

        /// <summary>
        /// The password for connecting to the server
        /// </summary>
        string Password { get; set; }

        /// <summary>
        /// Indicates whether the client is connected to the backend service and can recive and send messages
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// Gets the name of the service the IMessagingClient implements
        /// </summary>
        string ServiceName
        {
            get;
        }



        /// <summary>
        /// Connects to the messaging service's backend (if necessary)
        /// </summary>
        void Connect();

        /// <summary>
        /// Disconnects to the messaging service's backend
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Sends a message
        /// </summary>
        /// <param name="message">The Message to be sent</param>
        void SendMessage(Message message);

        /// <summary>
        /// Sends a message
        /// </summary>
        /// <param name="recipient">The message's recipient</param>
        /// <param name="message">The message's content</param>
        void SendMessage(string recipient, string message);

    }


    /// <summary>
    /// Encapsulates a message sent or reveived bz the IMessageingClient 
    /// </summary>
    public struct Message
    {
        /// <summary>
        /// The message's sender
        /// </summary>
        public string Sender { get; set; }

        /// <summary>
        /// The message's recipient
        /// </summary>
        public string Recipient { get; set; }

        /// <summary>
        /// The message's content
        /// </summary>
        public string  MessageBody { get; set; }
    }
}
