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
    public class RemoteMethodCall : RpcMessage, ISerializable
    {
        private static ILogger logger = LoggerService.GetLogger("RemoteMethodCall");


        /// <summary>
        /// The Method being called
        /// </summary>        
        public string MethodName { get; set; }

        /// <summary>
        /// The Parameters for the invoked method
        /// </summary>
        public List<object> Parameters { get;  set; }



        [DebuggerStepThrough()]
        public RemoteMethodCall()
        {
            XmlNames.Init(xmlNamespace);

            Parameters = new List<object>();
        }


        //only used internally
        public bool ResponseExpected { get; set; }


        #region ISerializable Members

        private static class XmlNames
        {
            private static bool initialized = false;

            public static XName RemoteMethodCall;
            public static XName ComponentName;
            public static XName CallId;
            public static XName MethodName;
            public static XName Parameters;
            

            public static void Init(string xmlNamespace)
            {
                if (initialized)
                {
                    return;
                }

                RemoteMethodCall = XName.Get("RemoteMethodCall", xmlNamespace);
                ComponentName = XName.Get("ComponentName", xmlNamespace);
                CallId = XName.Get("CallId", xmlNamespace);
                MethodName = XName.Get("MethodName", xmlNamespace);
                Parameters = XName.Get("Parameters", xmlNamespace);

                initialized = true;
            }
        }


        protected override string rootElementName
        {
            get { return "RemoteMethodCall"; }
        }

        protected override string schemaTypeName
        {
            get { return "remoteMethodCall"; }
        }


        public override XElement Serialize()
        {
            XElement root = new XElement(XmlNames.RemoteMethodCall);

            root.Add(new XElement(XmlNames.ComponentName, this.ComponentName));
            root.Add(new XElement(XmlNames.CallId, this.CallId.ToString()));
            root.Add(new XElement(XmlNames.MethodName, this.MethodName));
            root.Add(new XElement(XmlNames.Parameters, Parameters.Select(x => XmlSerializer.Serialize(x, xmlNamespace))));

            return root;
        }

        public override void Deserialize(XElement xmlData)
        {
            ComponentName = xmlData.Element(XmlNames.ComponentName).Value;
            MethodName = xmlData.Element(XmlNames.MethodName).Value;
            CallId = XmlSerializer.DeserializeAs<Guid>(xmlData.Element(XmlNames.CallId));            
            Parameters = xmlData.Element(XmlNames.Parameters).Elements().Select(x => XmlSerializer.Deserialize(x)).ToList<object>();            
        }

        #endregion
    }

 

    
}
