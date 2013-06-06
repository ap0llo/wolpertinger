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
using Wolpertinger.Core;
using Slf;
using System.Security;
using Nerdcave.Common;
using Nerdcave.Common.Extensions;
using Wolpertinger.FileShareCommon;

namespace Wolpertinger.Manager.CLI
{
    public class Session : Context
    {

        public string Target { get; private set; }

        IConnectionManager connectionManager;

        IClientConnection connection;
        private AuthenticationComponent authComponent;
        private ClientInfoClientComponent clientInfoComponent;
        private XmppLoggingConfiguratorComponent loggingConfigurator;
        private FileShareClientComponent fileShareComponent;

        ILogger logger = LoggerService.GetLogger("Session");



        public Session(string target, IConnectionManager connectionManager)
        {
            this.connectionManager = connectionManager;

            this.Target = target;
            this.Name = target;

            this.connection = connectionManager.GetClientConnection(target);            
            if (connection == null)
            {
                connection = connectionManager.AddClientConnection(target);
                connection.ConnectionReset += connection_ConnectionReset;
                connection.ConnectionTimedOut += connection_ConnectionTimedOut;
                connection.RemoteErrorOccurred += connection_RemoteErrorOccurred;
            }

            connection.WtlpClient.MessagingClient.Connect();

            //authComponent = (AuthenticationComponent)connection.GetClientComponent(ComponentNamesExtended.Authentication);
            //clientInfoComponent = (ClientInfoClientComponent)connection.GetClientComponent(ComponentNamesExtended.ClientInfoProvider);
            //loggingConfigurator = (XmppLoggingConfiguratorComponent)connection.GetClientComponent(ComponentNamesExtended.XmppLoggingConfigurator);
            //fileShareComponent = (FileShareClientComponent)connection.GetClientComponent(ComponentNamesExtended.FileShare);

            authComponent = new AuthenticationComponent() { ClientConnection = connection };
            clientInfoComponent = new ClientInfoClientComponent() { ClientConnection = connection };
            loggingConfigurator = new XmppLoggingConfiguratorComponent() { ClientConnection = connection };
            fileShareComponent = new FileShareClientComponent() { ClientConnection = connection };

        }





        //protected void getStatusCommand(IEnumerable<string> cmds)
        //{
        //    Program.StatusLine(this, connection.Connected ? String.Format("Connected, TrustLevel {0}, My TrsutLevel {1}", connection.TrustLevel, connection.MyTrustLevel) : "Not Connected");
        //}

     

        //internal void disconnectCommand(IEnumerable<string> cmds)
        //{

        //    if(connection != null)
        //        connection.ResetConnection(true);

        //    if (connection != null)
        //    {
        //        connectionManager.RemoveClientConnection(connection.Target);
        //    }

        //}

     

        //protected void exitSessionCommand(IEnumerable<string> cmds)
        //{
        //    Program.activeContext = Program.mainContext;
        //}


        
        

        
        

        #region Event Handlers

        private void connection_ConnectionTimedOut(object sender, EventArgs e)
        {
            //Program.ErrorLine(this, "Connection timed out");
        }

        private void connection_ConnectionReset(object sender, EventArgs e)
        {
            //Program.ErrorLine(this, "Connection reset");
            //connection = null;
        }

        private void connection_RemoteErrorOccurred(object sender, ObjectEventArgs<RemoteError> e)
        {
            //if (!e.Handled)
            //{
            //    Program.ErrorLine(this, "RemoteError: {0} in {1}", e.Value.ErrorCode.ToString(), e.Value.ComponentName);
            //    e.Handled = true;
            //}
        }

        #endregion Event Handlers






        protected override bool checkConnection()
        {
            if (connection == null)
            {
                //Program.ErrorLine(this, "Client connection not initialized");
                return false;
            }
            else if (!connection.Connected)
            {
                //Program.ErrorLine(this, "Not Connected");
                return false;
            }
            else
            {
                return true;
            }
        }



       

        

        


    }



}
