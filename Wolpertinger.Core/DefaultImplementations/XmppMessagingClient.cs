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

using agsXMPP;
using agsXMPP.protocol.client;
using Nerdcave.Common;
using Nerdcave.Common.Extensions;
using Slf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wolpertinger.Core
{
    public class XmppMessagingClient : IMessagingClient
    {
        private ILogger logger = LoggerService.GetLogger("XmppMessagingClient");
        private XmppClientConnection xmpp;
        private ConcurrentQueue<Message> messageQueue = new ConcurrentQueue<Message>();


        #region IMessagingClient Members

        public event EventHandler<ObjectEventArgs<Message>> MessageReceived;
        
        public event EventHandler ConnectedChanged;

        public event EventHandler<ObjectEventArgs<string>> PeerDisconnected;


        /// <summary>
        /// Gets the name of the service.
        /// </summary>
        public string ServiceName
        {
            get
            {
                return "XMPP";
            }

        }

        public bool Connected 
        {
            get
            {
                return (xmpp.XmppConnectionState == XmppConnectionState.Connected || 
                        xmpp.XmppConnectionState == XmppConnectionState.SessionStarted) 
                        ? true 
                        : false;
            }
        }

        public string Server { get; set; }

        public string Username { get; set; }
        
        public string Password  { get; set; }



        public void Connect()
        {
            //Initialize XmppClientConnection
            xmpp = new XmppClientConnection();
            xmpp.Show = ShowType.chat;

            //Set event handlers
            xmpp.OnClose += xmpp_OnClose;
            xmpp.OnMessage += xmpp_OnMessage;
            xmpp.OnPresence += xmpp_OnPresence;
            xmpp.OnLogin += xmpp_OnLogin;
            xmpp.OnError += xmpp_OnError;

            logger.Info("Connecting to Xmpp-Server, JabberId {0}@{1}", Username, Server);

            xmpp.Server = Server;
            xmpp.Resource = "";            

            //Connect to the server
            xmpp.Open(Username, Password);

        }

        
        public void Disconnect()
        {
            logger.Info("Closing connection to Xmpp Server");

            lock (this)
            {
                if (xmpp == null)
                {
                    logger.Warn("Connection already closed");
                }
                else
                {
                    xmpp.Close();
                    xmpp = null;
                }
            }
        }

        public void SendMessage(Message message)
        {
            //Message can only be sent, if client is connected
            if (!Connected)
            {
                logger.Warn("Cannot send message. Not connected. Message will be queued");
                messageQueue.Enqueue(message);
                return;
            }

            //send queued messages before sending new ones
            sendQueuedMessages();

            xmpp.Send(new agsXMPP.protocol.client.Message(message.Recipient, message.MessageBody));

        }

        public void SendMessage(string recipient, string message)
        {
            var msg = new Message() { Sender = null, Recipient = recipient, MessageBody = message };
            SendMessage(msg);
        }

        #endregion IMessagingClient Members



        #region Xmpp Event Handlers

        private void xmpp_OnError(object sender, Exception ex)
        {
            logger.Error(ex, "Xmpp Error");
        }

        private void xmpp_OnLogin(object sender)
        {
            onConnectedChanged();
        }

        private void xmpp_OnPresence(object sender, Presence pres)
        {
            string from = String.Format("{0}@{1}", pres.From.User, pres.From.Server);

            logger.Info("{0} disconnected", from);
            
            onPeerDisconnected(from);
        }

        private void xmpp_OnMessage(object sender, agsXMPP.protocol.client.Message msg)
        {
            new Task(delegate
                {
                    //ignore empty messages
                    if (msg.Body.IsNullOrEmpty() || msg.Error != null)
                        return;

                    //get the message's sender, remove XMPP resource from the message
                    var messageSender = msg.From.ToString();
                    messageSender = messageSender.Contains("/") ? messageSender.Substring(0, messageSender.IndexOf("/")) : messageSender;

                    //if messages bounced and sender is this client itself, ignore the message
                    if (messageSender.ToLower() == String.Format("{0}@{1}", this.Username, this.Server).ToLower())
                    {
                        logger.Warn("Bounced message revceived. Ignoring");
                        return;
                    }

                    logger.Info("Received message from {0}", messageSender);

                    //Raise MessageReceived event
                        this.onMessageReceived(messageSender, msg.Body);
                }).Start();
        }


        private void xmpp_OnClose(object sender)
        {
            logger.Info("Xmpp Connection closed");
            onConnectedChanged();
        }


        #endregion Xmpp Event Handlers


        #region Event Raisers

        /// <summary>
        /// Raises the MessageReceived event
        /// </summary>
        /// <param name="from">The message's sender</param>
        /// <param name="message">The message's body</param>
        private void onMessageReceived(string from, string message)
        {
            if (this.MessageReceived != null)
            {
                var msg = new Message() { Sender = from, MessageBody = message };
                this.MessageReceived(this, msg);
            }
        }

        /// <summary>
        /// Raises the ConnectedChanged event
        /// </summary>
        private void onConnectedChanged()
        {
            if (this.ConnectedChanged != null)
                this.ConnectedChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the PeerDisconnected event
        /// </summary>
        /// <param name="peer">The id of the peer that is no longer available</param>
        private void onPeerDisconnected(string peer)
        {
            if (this.PeerDisconnected != null)
                this.PeerDisconnected(this, peer);
        }

        #endregion Event Raisers


        private void sendQueuedMessages()
        {
            //prevent multiple threads sending queued messages simultaneously
            lock (this)
            {
                Message msg;
                while (!messageQueue.IsEmpty)
                {
                    //try to get a message from the queue
                    if(messageQueue.TryDequeue(out msg))
                        this.SendMessage(msg);
                }
            }

        }



    }
}
