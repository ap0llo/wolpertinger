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
        /// Occurs when a new ClientConnection is added
        /// </summary>
        event EventHandler<ObjectEventArgs<IClientConnection>> ClientConnectionAdded;



        /// <summary>
        /// Gets or sets the factory used for creating new instances of IClientConnection
        /// </summary>
        IConnectionFactory ConnectionFactory { get; set; }

        /// <summary>
        /// Gets or sets whether new incoming connection are to be accepted for all ClientConnections hostes by the ConnectionManager
        /// </summary>
        bool AcceptIncomingConnections { get; set; }

        /// <summary>
        /// The number of active connection the ConnectionManager allows (-1 for unlimited)
        /// </summary>
        int AllowedConnectionCount { get; set; }
        

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
        /// Get a list of all ClientConnections currently managed by the ConnectionManager
        /// </summary>
        /// <returns></returns>
        IEnumerable<IClientConnection> GetClientConnections();

        /// <summary>
        /// Returns a list if MessagingClients currently loaded
        /// </summary>        
        IEnumerable<IMessagingClient> GetMessagingClients();

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
