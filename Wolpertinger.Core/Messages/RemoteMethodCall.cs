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
using System.Xml.Serialization;
using Slf;
using Nerdcave.Common.Xml;

namespace Wolpertinger.Core
{
    /// <summary>
    /// Encapsulates the call a remote Method
    /// </summary>
    [XmlTypeName("Wolpertinger.RemoteMethodCall")]
    public class RemoteMethodCall : Message
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
            xml.Add(new XElement("TargetName") { Value = this.TargetName });
            xml.Add(new XElement("CallId") { Value = this.CallId.ToString() });
            xml.Add(new XElement("MethodName") { Value = this.MethodName });
            xml.Add(new XElement("Parameters"));


            foreach (object o in this.Parameters)
            {
                xml.Element("Parameters").Add(XmlSerializationHelper.SerializeToXmlObjectElement(o));
            }

            return xml;
        }

        public override object Deserialize(XElement xmlData)
        {
            if (xmlData == null) return null;
            if (xmlData.Name.LocalName != "RemoteMethodCall") return null;

            try
            {
                RemoteMethodCall call = new RemoteMethodCall();
                call.TargetName = xmlData.Element("TargetName").Value;
                call.MethodName = xmlData.Element("MethodName").Value;
                Guid id;

                if(!Guid.TryParse(xmlData.Element("CallId").Value, out id))
                {
                    logger.Error("No CallId found, illegal RemoteMethodCall. Returning null");
                    return null;
                }
                else
                {
                    call.CallId = id;
                }


                foreach (XElement elem in xmlData.Element("Parameters").Elements())
                {
                    object o = XmlSerializationHelper.DeserializeFromXMLObjectElement(elem);
                    if(o != null)call.Parameters.Add(o);
                }

                return call;
            }
            catch (NullReferenceException e)
            {
                logger.Error(e);
                return null;
            }

        }

        #endregion
    }

 

    
}
