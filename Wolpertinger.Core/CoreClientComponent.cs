﻿/*

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
using System.Threading.Tasks;
using System.Text;
using Slf;

namespace Wolpertinger.Core
{
    /// <summary>
    /// Client-implementation of the "Core" component
    /// See Wolpertinger API Documentation for details on the component.
    /// </summary>
    public class CoreClientComponent : IComponent
    {


        private ILogger logger = LoggerService.GetLogger("CoreClientComponent");


        /// <summary>
        /// The IClientConnection used to communicate with the target-client
        /// </summary>
        public IClientConnection ClientConnection { get; set; }


        /// <summary>
        /// Initializes a new instance of CoreClientComponent
        /// </summary>
        public CoreClientComponent()
        { }

        /// <summary>
        /// Initializes a new instance of CoreClientComponent
        /// </summary>
        /// <param name="clientConnection">The ClientConnection used by the component</param>
        public CoreClientComponent(IClientConnection clientConnection)
        {
            this.ClientConnection = clientConnection;
        }


        /// <summary>
        /// Asynchronously calls the Hearbeat RemoteMethod on the target client
        /// </summary>
        public Task HeartbeatAsync()
        {
            return Task.Factory.StartNew(() => 
                {
                    try
                    {
                        ClientConnection.CallRemoteAction(ComponentNames.Core, CoreMethods.Heartbeat); 
                    }
                    catch (TimeoutException)
                    {
                        logger.Warn("TimeOutException in HeartbeatAsync() was caught and ignored");   
                    }
                });
                
        }

        /// <summary>
        /// Asnychronously calls the SendResetNotice in the target client
        /// </summary>
        public Task SendResetNoticeAsync()
        {
            var task = new Task(delegate { ClientConnection.CallRemoteAction(ComponentNames.Core, CoreMethods.SendResetNotice); });
            task.Start();
            return task;
        }
 

    }
}