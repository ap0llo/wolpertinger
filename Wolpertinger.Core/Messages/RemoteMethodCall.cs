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
using System.Xml.Linq;
using System.Text;
using System.Diagnostics;
using Slf;
using Nerdcave.Common.Xml;

namespace Wolpertinger.Core
{
    /// <summary>
    /// Encapsulates the call a remote Method
    /// </summary>
    [XmlTypeName("Wolpertinger.RemoteMethodCall")]
    public class RemoteMethodCall : RpcMessage, ISerializable
    {
        private static ILogger logger = LoggerService.GetLogger("RemoteMethodCall");


        /// <summary>
        /// The Method being called
        /// </summary>        
        public string MethodName { get; set; }

        /// <summary>
        /// The Paramters for the invoked method
        /// </summary>
        public List<object> Parameters { get;  set; }



        [DebuggerStepThrough()]
        public RemoteMethodCall()
        {
            Parameters = new List<object>();
        }


        //only used internally
        public bool ResponseExpected { get; set; }


        #region ISerializable Members

        public override XElement Serialize()
        {
            XElement xml = new XElement("RemoteMethodCall");
            xml.Add(new XElement("ComponentName", this.ComponentName));
            xml.Add(new XElement("CallId", this.CallId.ToString()));
            xml.Add(new XElement("MethodName", this.MethodName));
            
            xml.Add(new XElement("Parameters"), Parameters.Select(x=> XmlSerializer.Serialize(x)));

            return xml;
        }

        public override void Deserialize(XElement xmlData)
        {
            ComponentName = xmlData.Element("ComponentName").Value;
            MethodName = xmlData.Element("MethodName").Value;
                      
            CallId = XmlSerializer.DeserializeAs<Guid>(xmlData.Element("CallId"));            
            Parameters = xmlData.Element("Parameters").Elements().Select(x => XmlSerializer.Deserialize(x)).ToList<object>();            
        }

        #endregion
    }

 

    
}
