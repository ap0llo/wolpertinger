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
    /// Client implementation of the ClientInfoProvider component.
    /// See Wolpertinger API Documentation for details on the component.
    /// </summary>
    [Component(ComponentNamesExtended.ClientInfoProvider, ComponentType.Client)]
    public class ClientInfoClientComponent : ClientComponent
    {
        /// <summary>
        /// Occurs when a response to a GetClientInfo request is received
        /// </summary>
        public event EventHandler<ObjectEventArgs> GetClientInfoCompleted;

        /// <summary>
        /// Occurs when a response to a GetKnownClients request is received
        /// </summary>
        public event EventHandler<ObjectEventArgs> GetKnownClientsCompleted;    
      
        /// <summary>
        /// Synchronously call the GetClientInfo RemoteMethod on the target client.
        /// </summary>
        /// <returns>Returns the repsonse when it is received (Blocking)</returns>
        public ClientInfo GetClientInfo()
        {
            return (ClientInfo)callRemoteMethod(ClientInfoProviderMethods.GetClientInfo.ToString());
        }

        /// <summary>
        /// Asynchronously call the GetClientInfo RemoteMethod on the target client and 
        /// raises the GetClientInfoCompleted event, once a response is received.
        /// </summary>
        public void GetClientInfoAsync()
        {
            callRemoteMethodAsync(ClientInfoProviderMethods.GetClientInfo,true);
        }

        /// <summary>
        /// Synchronously calls the GetKnownClients RemoteMethod on the target client.
        /// </summary>
        /// <returns>Returns a lsit of clients known to the target cleint once a response is received (Blocking)</returns>
        public IEnumerable<ClientInfo> GetKnownClients()
        {
            return (IEnumerable<ClientInfo>)callRemoteMethod(ClientInfoProviderMethods.GetKnownClients);
        }

        /// <summary>
        /// Asynchronously calls the GetKnownClients RemoteMethod on the target client and
        /// raises the GetKnownClientsCompleted event when a response is received.
        /// </summary>
        public void GetKnownClientsAsync()
        {
            callRemoteMethodAsync(ClientInfoProviderMethods.GetKnownClients, true);
        }


        /// <summary>
        /// Response handler for the GetClientInfo RemoteMethod
        /// </summary>
        /// <param name="info">The ClientInfo returned by the target client</param>
        [ResponseHandler(ClientInfoProviderMethods.GetClientInfo)]
        protected void responseHandlerGetClientInfo(ClientInfo info)
        {
            if (info != null)           
                onGetClientInfoCompleted(info);           
        }

        /// <summary>
        /// Response handler for the GetKnownClients RemoteMethod
        /// </summary>
        /// <param name="clients">The list of clients returned by the target client.</param>
        [ResponseHandler(ClientInfoProviderMethods.GetKnownClients)]
        protected void responseHandlerGetKnownClients(IEnumerable<ClientInfo> clients)
        {
            if (clients != null)            
                onGetKnownClientsCompleted(clients);            
        }



        protected void onGetClientInfoCompleted(ClientInfo info)
        {
            if (GetClientInfoCompleted != null)            
                GetClientInfoCompleted(this, new ObjectEventArgs(info));            
        }

        protected void onGetKnownClientsCompleted(IEnumerable<ClientInfo> clients)
        {
            if (this.GetKnownClientsCompleted != null)
                this.GetKnownClientsCompleted(this, new ObjectEventArgs(clients));
        }
    }



    
}
