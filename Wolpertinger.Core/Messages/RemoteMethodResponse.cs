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
using System.Xml.Serialization;
using Slf;
using Nerdcave.Common.Xml;

namespace Wolpertinger.Core
{
    [XmlTypeName("Wolpertinger.RemoteMethodResponse")]
    public class RemoteMethodResponse : Message
    {
        private static ILogger logger = LoggerService.GetLogger("RemoteMethodResponse");


        /// <summary>
        /// The method call's response value
        /// </summary>
        public object ResponseValue { get; set; }



        #region ISerializable Members
        
        public override XElement Serialize()
        {            
            XElement xmlData = new XElement("RemoteMethodResponse");
            xmlData.Add(new XElement("TargetName") { Value = this.TargetName });
            xmlData.Add(new XElement("CallId") { Value = this.CallId.ToString() });
            xmlData.Add(new XElement("ResponseValue"));

            //serialize the response value
            if (ResponseValue != null)
            {
                XElement x = XmlSerializationHelper.SerializeToXmlObjectElement(this.ResponseValue);
                if (x != null)
                {
                    xmlData.Element("ResponseValue").Add(x);
                }
            }

            return xmlData;
        }

        public override object Deserialize(XElement xmlData)
        {
            if (xmlData == null || xmlData.Name.LocalName != "RemoteMethodResponse") 
                return null;

            //try to parse the message
            try
            {
                RemoteMethodResponse response = new RemoteMethodResponse();
                response.TargetName = xmlData.Element("TargetName").Value;

                //deserialze the reponse value
                if(xmlData.Element("ResponseValue").Elements().Any())
                {
                    response.ResponseValue = XmlSerializationHelper.DeserializeFromXMLObjectElement(xmlData.Element("ResponseValue").Elements().First());                    
                }

                //chek the message for a CallId
                Guid id;
                if (!Guid.TryParse(xmlData.Element("CallId").Value, out id))
                {
                    logger.Error("No CallId found, illegal RemoteMethodResponse. Returning null");
                    return null;
                }
                else
                {
                    response.CallId = id;
                }

                return response;

            }
            catch (NullReferenceException ex)
            {
                logger.Error(ex);
                return null;
            }

        }

        #endregion
    }
}
