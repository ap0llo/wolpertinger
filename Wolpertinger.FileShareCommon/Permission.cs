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
using Nerdcave.Common.Extensions;
using Nerdcave.Common.Xml;

namespace Wolpertinger.FileShareCommon
{
    /// <summary>
    /// Describes a permission to access a item in a shared folder.
    /// </summary>
    [XmlTypeName("Wolpertinger.Permission")]
    public class Permission : ISerializable
    {

        public string Path { get; set; }

        public List<string> PermittedClients { get; set; }


        
        public Permission()
        {
            Path = "";
            PermittedClients = new List<string>();
        }



        #region ISerializable Members


        private class XmlElementNames
        {
            public static string XmlNamespace = "http://nerdcave.eu/wolpertinger";
            public static XName Permission = XName.Get("Permission", XmlNamespace);
            public static XName Path = XName.Get("Path", XmlNamespace);
            public static XName PermittedClients = XName.Get("PermittedClients", XmlNamespace);
            public static XName Client = XName.Get("Client", XmlNamespace);
        }

        public XElement Serialize()
        {
            XElement result = new XElement(XmlElementNames.Permission);

            result.Add(new XElement(XmlElementNames.Path, this.Path));
            result.Add(new XElement(XmlElementNames.PermittedClients, this.PermittedClients.Select(x => new XElement(XmlElementNames.Client, x))));
            
            return result;
        }

        public void Deserialize(XElement xmlData)
        {
            Path = xmlData.Element(XmlElementNames.Path).Value;

            var clients = from client in xmlData.Element(XmlElementNames.PermittedClients).Elements(XmlElementNames.Client)
                        select client.Value;

            PermittedClients = clients.ToList<string>();
        }

        #endregion


        public override string ToString()
        {
            string result = String.Format("Permission: {0}, Clients:", Path);
            foreach (string item in PermittedClients)
            {
                result += " " + item;
            }

            return result;
        }





    }
}
