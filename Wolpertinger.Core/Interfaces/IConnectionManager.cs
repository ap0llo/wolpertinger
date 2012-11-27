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
using System.Security;
using Nerdcave.Common;

namespace Wolpertinger.Core
{
    public interface IConnectionManager
    {
        /// <summary>
        /// Occurs when the state of the connection to the backend service (for sending messages) changed
        /// </summary>
        event EventHandler<EventArgs> IsConnectedChanged;

        /// <summary>
        /// Occurs when a new ClientConnection is added
        /// </summary>
        event EventHandler<ObjectEventArgs<IClientConnection>> ClientConnectionAdded;

        /// <summary>
        /// The XMPP Server to connect to
        /// </summary>
        string XmppServer { get; set; }

        /// <summary>
        /// The username of the XMPP account to connect to
        /// </summary>
        string XmppUsername { get; set; }

        string XmppResource { get; set; }

        /// <summary>
        /// The password for the XMPP account to connect to
        /// </summary>
        SecureString XmppPassword { get; set; }

        /// <summary>
        /// Indicates whether the ConnectionManager accepts incoming connections
        /// </summary>
        bool AcceptIncomingConnections { get; set; }

        /// <summary>
        /// The number of active connection the ConnectionManager allows (-1 for unlimited)
        /// </summary>
        int AllowedConnectionCount { get; set; }
        
        /// <summary>
        /// Inidicates whether the ConnectionManager is connected to the backend service for sending messages
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// The client username for user-authentication (required to gain Trust Level 4)
        /// </summary>
        string WolpertingerUsername { get; set; }

        /// <summary>
        /// The client password for user-authentication (required to gain Trust Level 4)
        /// </summary>
        SecureString WolpertingerPassword { get; set; }

        /// <summary>
        /// The key required to join a cluser
        /// </summary>
        byte[] ClusterKey { get; }

        /// <summary>
        /// Loads the ConnectionManagers settings from the specified folder
        /// </summary>
        /// <param name="folder">The folder to use for storing settings</param>
        void LoadSettings(string folder);

        /// <summary>
        /// The IComponentFactory used by the manager's client connections to retrive components
        /// </summary>
        IComponentFactory ComponentFactory { get; set; }


        /// <summary>
        /// Connects to the backend service
        /// </summary>
        void Connect();

        /// <summary>
        /// Closes the connection to the backend service
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Sends a the specified message to the target
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <param name="target">The receipeint of the message</param>
        void SendMessage(string message, string target);


        /// <summary>
        /// Adds a ClientConnection to the specified target
        /// </summary>
        /// <param name="target">The target to connect to</param>
        /// <returns>
        /// Returns the new ClientConnection or null, if the a connection could not be added
        /// </returns>
        IClientConnection AddClientConnection(string target);

        /// <summary>
        /// Gets the ClientConnection for the specified targt
        /// </summary>
        /// <param name="target">The target of the connection</param>
        /// <returns>Returns the ConnectionManager's ClientConnection for the target or null, if connection could not be found</returns>
        IClientConnection GetClientConnection(string target);

        /// <summary>
        /// Resets and closes the specified ClientConnection
        /// </summary>
        /// <param name="target">The target of the ClientConnection to be removed</param>
        void RemoveClientConnection(string target);

        /// <summary>
        /// Resets and cloeses all of the ConnectionManager's ClientConnections
        /// </summary>
        void RemoveAllClientConnections();

        /// <summary>
        /// Gets a list of call other known clients 
        /// </summary>
        /// <returns>Returns a list of ClientInfos containing information about the knwon clients</returns>
        IEnumerable<ClientInfo> GetKnownClients();

    }

}
