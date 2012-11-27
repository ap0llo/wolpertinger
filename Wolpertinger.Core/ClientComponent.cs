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
using System.Threading;
using Nerdcave.Common;

namespace Wolpertinger.Core
{
    /// <summary>
    /// A abstract class that can be used as base class for a client component
    /// </summary>
    public abstract class ClientComponent : IComponent
    {
        private string _componentName;


        /// <summary>
        /// Initializes a new instance of ClientComponent
        /// </summary>
        public ClientComponent()
        {
            //read the component's name from the class' ComponentAttribute attribute and save it in a local field
            ComponentAttribute attribute = (ComponentAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(ComponentAttribute));
            this._componentName = attribute.Name;
        }



        #region IClientComponent Members

        /// <summary>
        /// The IClientConnection used to communicate with the target-client
        /// </summary>
        public IClientConnection ClientConnection { get; set; }
        
        #endregion IClientComponent Members


        /// <summary>
        /// Reads the classes ComponentName from the ComponentAttribute
        /// </summary>
        protected string ComponentName
        {
            get { return _componentName; }
        }




        /// <summary>
        /// Calls a remote method and blocks until a response is received or the connection times out.
        /// Only use this for calling RemoteMethods that actually return a value!
        /// </summary>
        /// <param name="methodname">The name of the method to invoke</param>
        /// <param name="args">The arguments that will be passed to the method</param>
        /// <returns>Returns the value returned by the remote method.</returns>
        protected object callRemoteMethod(string methodname, params object[] args)
        {
            RemoteMethodCall call = newMethodCall(methodname, true, args);

            object value = ClientConnection.GetReponseValueBlocking(call);             

            if (value is RemoteError)
            {
                throw new RemoteErrorException(value as RemoteError);
            }
            else
            {
                return value;
            }

        }

        /// <summary>
        /// Calls a remote method and returns immediately. Will not wait for response.
        /// </summary>
        /// <param name="methodname">The name of the method to invoke</param>
        /// <param name="args">The arguments that will be passed to the method</param>
        protected void callRemoteMethodAsync(string methodname, bool responseExpected, params object[] args)
        {
            ClientConnection.SendMessage(newMethodCall(methodname, responseExpected, args));
        }


        /// <summary>
        /// Initializes a new RemoteMethodCall with the specified values
        /// </summary>
        /// <param name="methodname">The name of the method to be called</param>
        /// <param name="responseExpected">Specifies whether the called client is expected to respond to the call</param>
        /// <param name="args">The method call's parameters</param>
        /// <remarks>
        /// The return value's TargetName property will be set to the components name 
        /// </remarks>
        /// <returns>Retutns a new RemoteMethodCall with the specified properties</returns>
        private RemoteMethodCall newMethodCall(string methodname, bool responseExpected, object[] args)
        {
            RemoteMethodCall call = new RemoteMethodCall();
            call.TargetName = this.ComponentName;
            call.MethodName = methodname;
            call.Parameters = args.ToList<object>();
            call.ResponseExpected = responseExpected;

            return call;
        }



        
    }

}
