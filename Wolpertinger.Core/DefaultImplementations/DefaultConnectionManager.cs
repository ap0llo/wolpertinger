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
using agsXMPP;
using agsXMPP.protocol.client;
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

        //XMPP spec says limit on server side must not be smaller than 10K, googling shows 65k is common
        //Limiting to 30500 characters should result in message sizes of about 30K (splitting messages adds a little overhead) which seems like a good compromise
        private const int MESSAGELENGTHLIMIT = 30500;


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

        private Dictionary<string, IClientConnection> connections = new Dictionary<string, IClientConnection>();

        private XmppClientConnection xmpp = new XmppClientConnection();


        private Dictionary<string, string[]> partialMessages = new Dictionary<string, string[]>();
        private Dictionary<string, string> partialMessageSenders = new Dictionary<string, string>();
        
        private Dictionary<string, int> partialMessagesCount = new Dictionary<string, int>();

        Queue<agsXMPP.protocol.client.Message> queuedMessages = new Queue<agsXMPP.protocol.client.Message>();        

        private KeyValueStore settingsFile;
        private bool settingsLoaded = false;


        private string _xmppServer;
        private string _xmppUsername;
        private string _xmppResource;
        private SecureString _xmppPassword;
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

            ThreadPool.SetMaxThreads(100, 100);

            applicationDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Wolpertinger");

            if (!Directory.Exists(applicationDataFolder))
            {
                Directory.CreateDirectory(applicationDataFolder);
            }

            AllowedConnectionCount = -1;

            logger = LoggerService.GetLogger();


            xmpp = new XmppClientConnection();
            xmpp.Show = ShowType.chat;

            xmpp.OnClose += new ObjectHandler(xmpp_OnClose);
            xmpp.OnMessage += new agsXMPP.protocol.client.MessageHandler(xmpp_OnMessage);
            xmpp.OnPresence += new PresenceHandler(xmpp_OnPresence);
            xmpp.OnLogin += s => { sendQueuedMessages(); };


        }


        #region IConnectionManager Members

        /// <summary>
        /// Occurs when the state of the connection to the backend service (for sending messages) changed
        /// </summary>
        public event EventHandler<EventArgs> IsConnectedChanged;

        /// <summary>
        /// Occurs when a new ClientConnection is added
        /// </summary>
        public event EventHandler<ObjectEventArgs<IClientConnection>> ClientConnectionAdded;

        /// <summary>
        /// The XMPP Server to connect to
        /// </summary>
        public string XmppServer
        {
            get
            {
                return _xmppServer;
            }
            set
            {
                if (value != _xmppServer)
                {
                    _xmppServer = value;
                    settingsFile.SaveItem("XmppServer", _xmppServer);
                }
            }
        }

        /// <summary>
        /// The username of the XMPP account to connect to
        /// </summary>
        public string XmppUsername
        {
            get { return _xmppUsername; }
            set
            {
                if (value != _xmppUsername)
                {
                    _xmppUsername = value;
                    settingsFile.SaveItem("XmppUsername", _xmppUsername);
                }
            }
        }

        /// <summary>
        /// Gets or sets the XMPP resource.
        /// </summary>
        /// <value>
        /// The XMPP resource.
        /// </value>
        public string XmppResource
        {
            get { return _xmppResource; }
            set
            {
                if (value != _xmppResource)
                {
                    _xmppResource = value;
                    settingsFile.SaveItem("XmppResource", _xmppResource);
                }
            }
        }

        /// <summary>
        /// The password for the XMPP account to connect to
        /// </summary>
        public SecureString XmppPassword
        {
            get
            {
                return _xmppPassword;
            }
            set
            {
                _xmppPassword = value;
                settingsFile.SaveItem("XmppPassword", _xmppPassword.Unwrap());
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
        /// Inidicates whether the ConnectionManager is connected to the backend service for sending messages
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return (xmpp.XmppConnectionState == XmppConnectionState.Connected || xmpp.XmppConnectionState == XmppConnectionState.SessionStarted) ? true : false;
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
        /// The IComponentFactory used by the manager's client connections to retrive components
        /// </summary>
        public IComponentFactory ComponentFactory { get; set; }

        /// <summary>
        /// Loads the ConnectionManagers settings from the specified folder
        /// </summary>
        /// <param name="folder">The folder to use for storing settings</param>
        public void LoadSettings(string settingsFolder)
        {
            applicationDataFolder = settingsFolder;

            settingsFile = new KeyValueStore(Path.Combine(settingsFolder, "Account.xml"));

            if (settingsFile != null)
            {
                _clusterKey = settingsFile.GetItem<string>("ClusterKey").GetBytesHexString();

                XmppUsername = settingsFile.GetItem<string>("XmppUsername");
                XmppServer = settingsFile.GetItem<string>("XmppServer");
                XmppResource = settingsFile.GetItem<string>("XmppResource");
                XmppPassword = settingsFile.GetItem<string>("XmppPassword").ToSecureString();

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
        /// Connects to the backend service
        /// </summary>
        public void Connect()
        {            
            logger.Info("Connecting to Server, JabberId {0}@{1}", XmppUsername, XmppServer);

            xmpp.Server = XmppServer;
            xmpp.Resource = XmppResource;
            xmpp.OnLogin += delegate(object o) { onIsConnectedChanged(); };

            xmpp.Open(XmppUsername, XmppPassword.Unwrap());
        }

        /// <summary>
        /// Closes the connection to the backend service
        /// </summary>
        public void Disconnect()
        {
            logger.Info("Closing connection to XMPP-Server");
            xmpp.Close();
        }

        /// <summary>
        /// Sends a the specified message to the target
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <param name="target">The receipeint of the message</param>
        public void SendMessage(string to, string message)
        {
            //Split the message if it is to long to be sent as one piece
            //Added tolerance range of 100 characters to avoid endless recurison as splitting messages adds a little overhead
            if (message.Length > MESSAGELENGTHLIMIT + 100)
            {
                //Split messages and send it piece by piece
                string id = Guid.NewGuid().ToString();
                string[] fragments = message.Split((message.Length / MESSAGELENGTHLIMIT) + 1);
                
                //Wrap the fragments into XML "parts" so they can be put together by the recipient
                for (int i = 0; i < fragments.Length; i++)
                {
                    XElement msg = new XElement("part");
                    msg.Add(new XAttribute("total", fragments.Length));
                    msg.Add(new XAttribute("number", i + 1));
                    msg.Add(new XAttribute("id", id));

                    msg.Value = fragments[i];

                    //Send every fragment
                    SendMessage(to, msg.ToString());
                }
                return;
            }

            //Warp text into a xmpp-Message
            agsXMPP.protocol.client.Message newMessage = new agsXMPP.protocol.client.Message(to, MessageType.chat, message);

            if (!IsConnected)
            {
                logger.Error("Could not send message, Not connected to server, Message has been queued");
                //queue the message if there is no connection to the XMPP server
                queuedMessages.Enqueue(newMessage);
                return;
            }

            //Send queued messages first
            sendQueuedMessages();

            //Send the new message
            xmpp.Send(newMessage);
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
            if (connections.Count < AllowedConnectionCount ||AllowedConnectionCount < 0)
            {
                //get a new ClientConneciton using the ComponentFactory
                IClientConnection connection = ComponentFactory.GetClientConnection(target);
                connection.ConnectionManager = this;
                connections.Add(target, connection);

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
            return connections.ContainsKey(target) ? connections[target] : null;
        }

        /// <summary>
        /// Resets and closes the specified ClientConnection
        /// </summary>
        /// <param name="target">The target of the ClientConnection to be removed</param>
        public void RemoveClientConnection(string target)
        {
            if (connections.ContainsKey(target))
            {
                //before removing a ClientConnection, save it as known client
                if (knownClientsCache.ContainsKey(target))
                    knownClientsCache[target] = connections[target].GetClientInfo();
                else
                    knownClientsCache.Add(target, connections[target].GetClientInfo());

                saveKnownClients();
                logger.Info("Removing Client-Connection: {0}", target);
                connections.Remove(target);
            }
        }

        /// <summary>
        /// Resets and cloeses all of the ConnectionManager's ClientConnections
        /// </summary>
        public void RemoveAllClientConnections()
        {
            int connectionCount = connections.Count;
            for(int i = 0; i < connectionCount; i++)
            {
                RemoveClientConnection(connections.Last().Key);
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
                    .Select(x => connections.ContainsKey(x.JId) ? connections[x.JId].GetClientInfo() : x)
                    .ToList<ClientInfo>();
        }

        #endregion IConnectionManager Members


        /// <summary>
        /// Raises the IsConnectedChanged event
        /// </summary>
        protected virtual void onIsConnectedChanged()
        {
            if (IsConnectedChanged != null)
            {
                IsConnectedChanged(this, EventArgs.Empty);
            }
        }

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
                while (connections.Count > AllowedConnectionCount)
                {
                    RemoveClientConnection(connections.Last().Key);
                }
            }
        }

        /// <summary>
        /// Send all messages that have been queued for sending
        /// </summary>
        private void sendQueuedMessages()
        {
            while (queuedMessages.Count > 0)
            {
                xmpp.Send(queuedMessages.Dequeue());
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
        /// Handles incoming messages from the XMPP connection
        /// </summary>
        protected virtual void xmpp_OnMessage(object sender, agsXMPP.protocol.client.Message msg)
        {
            //ignore empty messages
            if (msg.Body.IsNullOrEmpty() | msg.Error != null)
                return; 

            string from = msg.From.ToString();

            //remove the XMPP resource from the 'from' string
            if (from.Contains("/"))
            {
                from = from.Substring(0, from.IndexOf("/"));
            }

            //if messages bounced and sender is this client itself, ignore the message
            if (from == String.Format("{0}@{1}", XmppUsername, XmppServer))
            {
                return;
            }

            //try to parse the message as XML (works if message is a part of a splitted message)
            XElement message;
            try
            {
                message = XElement.Parse(msg.Body);
            }
            catch (XmlException)
            {
                message = null;
            }

            //check if messgae if part of a splitted message
            if (message != null && message.Name.LocalName.ToLower() == "part")
            {
                //send heartbeat to ClientConnection to prevent connection from timing out, when messages get splitted
                if (connections.ContainsKey(from))
                {                    
                    connections[from].Hearbeat();
                }

                //retrive information about the message fragment
                string id = "";
                int total = 0;
                int number = 0;
                try
                {
                    id = message.Attribute("id").Value;
                    total = int.Parse(message.Attribute("total").Value);
                    number = int.Parse(message.Attribute("number").Value);
                }
                catch (FormatException) { }
                catch (NullReferenceException) { }

                lock (this)
                {
                    if (!partialMessages.ContainsKey(id))
                    {
                        //message is first fragment of a message to be received => add it to the list of partial messages
                        partialMessages.Add(id, new string[total]);
                        partialMessageSenders.Add(id, from);
                        partialMessagesCount.Add(id, 0);
                    }

                    //add the fragment to the list of partialMessages at the rigth location
                    partialMessages[id][number - 1] = message.Value;
                    partialMessagesCount[id] = partialMessagesCount[id] + 1;

                    //check if fragment is last fragment of the message
                    if (partialMessagesCount[id] == partialMessages[id].Length)
                    {
                        agsXMPP.protocol.client.Message newMsg = new agsXMPP.protocol.client.Message() { From = new Jid(partialMessageSenders[id]) };

                        //stitch together all of the message's fragments
                        string[] fragments = partialMessages[id];
                        for (int i = 0; i < fragments.Length; i++)
                        {
                            newMsg.Body += fragments[i];
                        }

                        //remove fragments from the list of partial messages
                        partialMessages.Remove(id);
                        partialMessageSenders.Remove(id);
                        partialMessagesCount.Remove(id);

                        //handle message as if it was received as one piece
                        xmpp_OnMessage(sender, newMsg);
                    }
                }
                return;
            }

            
            if (connections.ContainsKey(from))
            {
                //connection is to sender has already been opened => pass message to ClientConnetion
                connections[from].ProcessMessage(msg.Body.ToString());
            }
            else
            {
                //connection is from new sender => add new ClientConnection
                IClientConnection connection = AddClientConnection(from);

                if (connection != null)
                {
                    //connection was added successfully => pass message to the new connection
                    connection.ProcessMessage(msg.Body.ToString());
                }
                else
                {
                    //Connection will not be accepted, initialize connection and send refuse, then dispose of the connection (do not add to connections)
                    IClientConnection c = ComponentFactory.GetClientConnection(from);
                    c.AcceptConnections = false;
                    c.ProcessMessage(msg.Body.ToString());
                }
            }

        }

        /// <summary>
        /// Handles changing presence of XMPP buddies
        /// </summary>
        protected virtual void xmpp_OnPresence(object sender, Presence pres)
        {            
            string from = String.Format("{0}@{1}", pres.From.User, pres.From.Server);

            if (connections.ContainsKey(from) && pres.Type != PresenceType.available)
            {
                //client is no longer available => remove the ClientConnection
                RemoveClientConnection(from);
            }

        }

        /// <summary>
        /// Handles closing of the XMPP connection
        /// </summary>
        protected virtual void xmpp_OnClose(object sender)
        {
            logger.Info("XMPP Connection closed");
            onIsConnectedChanged();
        }

        /// <summary>
        /// Handles a ClientConnection being rest
        /// </summary>
        protected virtual void connection_ConnectionReset(object sender, EventArgs e)
        {
            //Remove ClientConnection that has been reset
            RemoveClientConnection((sender as IClientConnection).Target);
        }

        #endregion



      
    }
}
