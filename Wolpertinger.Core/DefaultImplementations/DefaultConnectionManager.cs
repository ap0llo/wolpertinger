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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Nerdcave.Common;
using Nerdcave.Common.Extensions;
using Nerdcave.Common.Xml;
using Slf;
using System.Diagnostics;
using System.Xml;
using System.Collections;
using System.Timers;
using System.Security;
using System.Threading;

namespace Wolpertinger.Core
{
    /// <summary>
    /// Default implementation of <see cref="Wolpertinger.Core.IConnectionManager" /> that uses a XMPP connection
    /// </summary>
    public class DefaultConnectionManager : IConnectionManager
    {

        #region Static Members

        private static ILogger logger = LoggerService.GetLogger("ConnectionManager");

        private static string applicationDataFolder;

        /// <summary>
        /// The path of the folder that is used to store settings-files (components etc. should store their data there, too)
        /// </summary>
        public static string SettingsFolder { get { return applicationDataFolder; } }

        #endregion Static Members


        #region Fields

        private Dictionary<string, ClientInfo> knownClientsCache = new Dictionary<string, ClientInfo>();

        private Dictionary<string, IClientConnection> clientConnections = new Dictionary<string, IClientConnection>();
        private List<IMessagingClient> messagingClients = new List<IMessagingClient>();


        private KeyValueStore settingsFile;
        private bool settingsLoaded = false;


        private int _allowedConnectionCount;
        private string _wolpertingerUsername;
        private SecureString _wolpertignerPassword;
        private byte[] _clusterKey;

        #endregion Fields



        /// <summary>
        /// Initializes a new instance of DefaultConnectionManager
        /// </summary>
        public DefaultConnectionManager()
        {
            applicationDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Wolpertinger");

            if (!Directory.Exists(applicationDataFolder))
            {
                Directory.CreateDirectory(applicationDataFolder);
            }

            AllowedConnectionCount = -1;

            ConnectionFactory = new DefaultConnectionFactory();

            logger = LoggerService.GetLogger();

        }


        #region IConnectionManager Members

        /// <summary>
        /// Occurs when a new ClientConnection is added
        /// </summary>
        public event EventHandler<ObjectEventArgs<IClientConnection>> ClientConnectionAdded;


        /// <summary>
        /// Gets or sets the factory used for creating new instances of IClientConnection
        /// </summary>
        public IConnectionFactory ConnectionFactory { get; set; }

        /// <summary>
        /// Indicates whether the ConnectionManager accepts incoming connections
        /// </summary>
        public bool AcceptIncomingConnections { get; set; }

        /// <summary>
        /// The number of active connection the ConnectionManager allows (-1 for unlimited)
        /// </summary>
        public int AllowedConnectionCount
        {
            get { return _allowedConnectionCount; }
            set
            {
                if (value != _allowedConnectionCount)
                {
                    _allowedConnectionCount = value;
                    applyAllowedConnectionCount();
                }
            }
        }


        /// <summary>
        /// The client username for user-authentication (required to gain Trust Level 4)
        /// </summary>
        public string WolpertingerUsername
        {
            get
            {
                return _wolpertingerUsername;
            }
            set
            {
                _wolpertingerUsername = value;
            }
        }

        /// <summary>
        /// The client password for user-authentication (required to gain Trust Level 4)
        /// </summary>
        public SecureString WolpertingerPassword
        {
            get
            {
                return _wolpertignerPassword;
            }
            set
            {
                _wolpertignerPassword = value;                
            }
        }


        /// <summary>
        /// The key required to join a cluser
        /// </summary>
        public byte[] ClusterKey
        {
            get { return _clusterKey; }
        }


        /// <summary>
        /// Loads the ConnectionManagers settings from the specified folder
        /// </summary>
        /// <param name="settingsFolder">The folder to use for storing settings</param>
        public void LoadSettings(string settingsFolder)
        {
            applicationDataFolder = settingsFolder;

            settingsFile = new KeyValueStore(Path.Combine(settingsFolder, "Account.xml"));

            if (settingsFile != null)
            {
                _clusterKey = settingsFile.GetItem<string>("ClusterKey").GetBytesBase64();


                //Set up XmppMessagingClient and connect
                var messagingClient = new XmppMessagingClient()
                    {
                        Username = settingsFile.GetItem<string>("XmppUsername"),
                        Server = settingsFile.GetItem<string>("XmppServer"),
                        Password = settingsFile.GetItem<string>("XmppPassword")
                    };

                messagingClients.Add(messagingClient);
                messagingClient.MessageReceived += messagingClient_MessageReceived;
                messagingClient.Connect();
   


                var clients = settingsFile.GetItem<IEnumerable<object>>("KnownClients");
                if (clients == null)
                    knownClientsCache = new Dictionary<string, ClientInfo>();
                else
                    knownClientsCache = clients
                                    .Cast<ClientInfo>()
                                    .ToDictionary(x => x.JId);

                settingsLoaded = true;

                logger.Info("Sucessfully loaded settings");
            }
        }



        /// <summary>
        /// Adds a ClientConnection to the specified target
        /// </summary>
        /// <param name="target">The target to connect to</param>
        /// <returns>
        /// Returns the new ClientConnection or null, if the a connection could not be added
        /// </returns>
        public IClientConnection AddClientConnection(string target)
        {
            if (clientConnections.Count < AllowedConnectionCount ||AllowedConnectionCount < 0)
            {
                //get a new ClientConneciton using the ComponentFactory
                IClientConnection connection = ConnectionFactory.GetClientConnection();
                connection.Target = target;
                connection.ConnectionManager = this;
                connection.WtlpClient = new DefaultWtlpClient(this.messagingClients.First(), target);
                clientConnections.Add(target.ToLower(), connection);

                //subscribe to the connection's reset event, so it can be removed when it has been reset
                connection.ConnectionReset += connection_ConnectionReset;

                //raise the ClientConnectionAdded event
                onClientConnectionAdded(connection);

                //save new connection as known client
                if (knownClientsCache.ContainsKey(target))
                    knownClientsCache[target] = connection.GetClientInfo();
                else
                    knownClientsCache.Add(target, connection.GetClientInfo());

                saveKnownClients();

                return connection;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the ClientConnection for the specified targt
        /// </summary>
        /// <param name="target">The target of the connection</param>
        /// <returns>
        /// Returns the ConnectionManager's ClientConnection for the target or null, if connection could not be found
        /// </returns>
        public IClientConnection GetClientConnection(string target)
        {
            return clientConnections.ContainsKey(target) ? clientConnections[target] : null;
        }

        /// <summary>
        /// Get a list of all ClientConnections currently managed by the ConnectionManager
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IClientConnection> GetClientConnections()
        {
            return clientConnections.Values.ToList<IClientConnection>();
        }

        /// <summary>
        /// Returns a list if MessagingClients currently loaded
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IMessagingClient> GetMessagingClients()
        {
            return messagingClients.ToList<IMessagingClient>();
        }

        /// <summary>
        /// Resets and closes the specified ClientConnection
        /// </summary>
        /// <param name="target">The target of the ClientConnection to be removed</param>
        public void RemoveClientConnection(string target)
        {
            if (clientConnections.ContainsKey(target))
            {
                //before removing a ClientConnection, save it as known client
                if (knownClientsCache.ContainsKey(target))
                    knownClientsCache[target] = clientConnections[target].GetClientInfo();
                else
                    knownClientsCache.Add(target, clientConnections[target].GetClientInfo());

                saveKnownClients();
                logger.Info("Removing Client-Connection: {0}", target);
                clientConnections.Remove(target);
            }
        }

        /// <summary>
        /// Resets and cloeses all of the ConnectionManager's ClientConnections
        /// </summary>
        public void RemoveAllClientConnections()
        {
            int connectionCount = clientConnections.Count;
            for(int i = 0; i < connectionCount; i++)
            {
                RemoveClientConnection(clientConnections.Last().Key);
            }
        }


        /// <summary>
        /// Gets a list of call other known clients
        /// </summary>
        /// <returns>
        /// Returns a list of ClientInfos containing information about the knwon clients
        /// </returns>
        public IEnumerable<ClientInfo> GetKnownClients()
        {
            //return knwon clients from the cache, if connection is still active, get ClientInfo from active connection
            return knownClientsCache.Values
                    .Select(x => clientConnections.ContainsKey(x.JId) ? clientConnections[x.JId].GetClientInfo() : x)
                    .ToList<ClientInfo>();
        }

        #endregion IConnectionManager Members




        /// <summary>
        /// Raises the ClientConnectionAdded event with the specified IClientConnection
        /// </summary>
        /// <param name="newConnection">The connetion that has been added</param>
        protected virtual void onClientConnectionAdded(IClientConnection newConnection)
        {
            logger.Info("Client-Connection added: {0}", newConnection.Target);
            if (this.ClientConnectionAdded != null)
            {
                this.ClientConnectionAdded(this, new ObjectEventArgs<IClientConnection>(newConnection));                
            }
        }

        /// <summary>
        /// Closes ClientConnections until there are only as many connection open as allowed
        /// </summary>
        private void applyAllowedConnectionCount()
        {
            if (AllowedConnectionCount > -1)
            {
                while (clientConnections.Count > AllowedConnectionCount)
                {
                    RemoveClientConnection(clientConnections.Last().Key);
                }
            }
        }
        
        /// <summary>
        /// Saves the list of known clients to the settings-file
        /// </summary>
        private void saveKnownClients()
        {
            settingsFile.SaveItem("KnownClients", knownClientsCache.Values.ToList<ClientInfo>());
        }

        
        #region Event Handlers

        /// <summary>
        /// Handles a ClientConnection being rest
        /// </summary>
        protected virtual void connection_ConnectionReset(object sender, EventArgs e)
        {
            //Remove ClientConnection that has been reset
            RemoveClientConnection((sender as IClientConnection).Target);
        }

        protected virtual void messagingClient_MessageReceived(object sender, ObjectEventArgs<Message> e)
        {
            if (!e.Handled)
            {
                if (!clientConnections.ContainsKey(e.Value.Sender.ToLower()))
                {
                    var connection = AddClientConnection(e.Value.Sender);
                    connection.AcceptConnections = this.AcceptIncomingConnections && !(clientConnections.Count < AllowedConnectionCount);

                    connection.WtlpClient.HandleMessage(e.Value.MessageBody);

                    e.Handled = true;
                }
                
            }
        }

        #endregion

    }

   
}
