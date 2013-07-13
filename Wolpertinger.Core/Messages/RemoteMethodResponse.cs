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
using System.Xml.Linq;
using Slf;
using Nerdcave.Common.Xml;
using System.Xml;

namespace Wolpertinger.Core
{
    public class RemoteMethodResponse : RpcMessage, ISerializable
    {
        private static ILogger logger = LoggerService.GetLogger("RemoteMethodResponse");


        /// <summary>
        /// The method call's response value
        /// </summary>
        public object ResponseValue { get; set; }


        public RemoteMethodResponse()
        {
            XmlNames.Init(xmlNamespace);
        }

        #region ISerializable Members

        private static class XmlNames
        {
            private static bool initialized = false;

            public static XName RemoteMethodResponse;
            public static XName ComponentName;
            public static XName CallId;
            public static XName ResponseValue;


            public static void Init(string xmlNamespace)
            {
                if (initialized)
                {
                    return;
                }

                RemoteMethodResponse = XName.Get("RemoteMethodResponse", xmlNamespace);
                ComponentName = XName.Get("ComponentName", xmlNamespace);
                CallId = XName.Get("CallId", xmlNamespace);
                ResponseValue = XName.Get("ResponseValue", xmlNamespace);

                initialized = true;
            }
        }


        protected override string rootElementName
        {
            get { return "RemoteMethodResponse"; }
        }

        protected override string schemaTypeName
        {
            get { return "remoteMethodResponse"; }

        }

        
        public override XElement Serialize()
        {
            XElement root = new XElement(XmlNames.RemoteMethodResponse);
            root.Add(new XElement(XmlNames.ComponentName, this.ComponentName));
            root.Add(new XElement(XmlNames.CallId, this.CallId.ToString()));
            
            //serialize the response value
            root.Add(new XElement(XmlNames.ResponseValue));
            if (ResponseValue != null)
            {
                XElement value = XmlSerializer.Serialize(this.ResponseValue, xmlNamespace);
                if (value != null)
                {
                    root.Element(XmlNames.ResponseValue).Add(value);
                }
            }

            return root;
        }

        public override void Deserialize(XElement xmlData)
        {
            this.ComponentName = xmlData.Element(XmlNames.ComponentName).Value;
            this.CallId = XmlSerializer.DeserializeAs<Guid>(xmlData.Element(XmlNames.CallId));

            //deserialize the response value
            if (xmlData.Element(XmlNames.ResponseValue).Elements().Any())
            {
                this.ResponseValue = XmlSerializer.Deserialize(xmlData.Element(XmlNames.ResponseValue).Elements().First());                    
            }
        }

        #endregion
    }
}
