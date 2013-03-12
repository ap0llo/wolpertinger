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



        #region ISerializable Members
        
        public override bool Validate(XElement xml)
        {
            //TODO
            throw new NotImplementedException();
        }

        public override XElement Serialize()
        {            
            XElement xmlData = new XElement("RemoteMethodResponse");
            xmlData.Add(new XElement("ComponentName", this.ComponentName));
            xmlData.Add(new XElement("CallId", this.CallId.ToString()));
            
            //serialize the response value
            xmlData.Add(new XElement("ResponseValue"));
            if (ResponseValue != null)
            {
                XElement x = XmlSerializer.Serialize(this.ResponseValue);
                if (x != null)
                {
                    xmlData.Element("ResponseValue").Add(x);
                }
            }

            return xmlData;
        }

        public override void Deserialize(XElement xmlData)
        {
            if (xmlData == null || xmlData.Name.LocalName != "RemoteMethodResponse")
                throw new XmlException();

            this.ComponentName = xmlData.Element("ComponentName").Value;

            //deserialze the reponse value
            if(xmlData.Element("ResponseValue").Elements().Any())
            {
                this.ResponseValue = XmlSerializer.Deserialize(xmlData.Element("ResponseValue").Elements().First());                    
            }

            this.CallId = XmlSerializer.DeserializeAs<Guid>(xmlData.Element("CallId"));                         
        }

        #endregion
    }
}
