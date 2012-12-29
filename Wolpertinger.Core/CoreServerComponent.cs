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
using System.Threading.Tasks;
using System.Text;

namespace Wolpertinger.Core
{
    /// <summary>
    /// Server-implementation of the "Core" component
    /// See Wolpertinger API Documentation for details on the component.
    /// </summary>
    [Component(ComponentNames.Core)]
    public class CoreServerComponent : IComponent
    {


        public IClientConnection ClientConnection { get; set; }
        


        /// <summary>
        /// Server implementation of the Heartbeat RemoteMethod
        /// </summary>
        /// <returns></returns>
        [MethodCallHandler(CoreMethods.Heartbeat), TrustLevel(0)]
        public CallResult Heartbeat_server()
        {            
            ClientConnection.Hearbeat();
            return new VoidResult();
        }

        /// <summary>
        /// Server implementation of the SendResetNotice RemoteMethod
        /// </summary>
        /// <returns></returns>
        [MethodCallHandler(CoreMethods.SendResetNotice), TrustLevel(0)]
        public CallResult SendResetNotice_server()
        {
            this.ClientConnection.ResetConnection(false);
            return new VoidResult();
        }



    }
}
