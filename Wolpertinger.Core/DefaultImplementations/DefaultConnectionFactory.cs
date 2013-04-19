using Nerdcave.Common.Extensions;
using Nerdcave.Common.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Wolpertinger.Core
{
    /// <summary>
    /// Default implementation  of <see cref="IConnectionFactory"/>
    /// </summary>
    public class DefaultConnectionFactory : IConnectionFactory
    {        
        static string username;
        static string password;
        static string server;
        

        public static void Initialize()
        {
            var filename = Path.Combine(DefaultConnectionManager.SettingsFolder, "Account.xml");

            if (File.Exists(filename))
            {
                //Initialize settings file
                var settingsFile = new KeyValueStore(filename);

                //get required values
                username = settingsFile.GetItem<string>("XmppUsername");
                server = settingsFile.GetItem<string>("XmppServer");
                password= settingsFile.GetItem<string>("XmppPassword");

                //check if all values were loaded
                if (username.IsNullOrEmpty()
                    || password.IsNullOrEmpty()
                    || server.IsNullOrEmpty())
                {
                    throw new Exception("Settings could not be loaded");
                }
            }
            else
            {
                throw new IOException(String.Format("Settings-file at {0} could not be found", filename));
            }            
        }


        /// <summary>
        /// Gets a new instance of a implementation of IClientConnection
        /// </summary>
        /// <returns>Returns a new instance of implementation of IClientConnection or null if no implementation could be found</returns>
        public virtual IClientConnection GetClientConnection(string recipient, string resource)
        {
            var connection = new DefaultClientConnection();
            connection.ComponentFactory = new DefaultComponentFactory();
            connection.Target = recipient;

            var messagingClient = GetMessagingClient("xmpp", resource);
            connection.WtlpClient = new DefaultWtlpClient(messagingClient, recipient);

            return connection;
        }



        public virtual IMessagingClient GetMessagingClient(string servicename, string resource)
        {
            if (servicename.ToLower() == "xmpp")
            {
                var messagingClient = new XmppMessagingClient();
                messagingClient.Resource = resource;
                messagingClient.Username = username;
                messagingClient.Password = password;
                messagingClient.Server = server;

                return messagingClient;
            }
            else
            {
                throw new NotSupportedException(String.Format("Unknown service-name: {0}", servicename));
            }
        }

    }
}
