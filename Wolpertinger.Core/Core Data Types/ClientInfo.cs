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
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;
using Slf;
using Nerdcave.Common.Xml;
using System.Diagnostics;

namespace Wolpertinger.Core
{
    /// <summary>
    /// Class is used to exchange information about wolpertinger clients
    /// </summary>
    [XmlTypeName("Wolpertinger.ClientInfo")]
    public class ClientInfo : ISerializable
    {

        static ILogger logger = LoggerService.GetLogger("ClientInfo");

        /// <summary>
        /// The Jabber-Id of the client the ClientInfo describes
        /// </summary>
        public string JId { get; set; }

        /// <summary>
        /// The profiles the client supports
        /// </summary>
        public List<Profile> Profiles {get;set;}

        /// <summary>
        /// The trust-level the client reached during the last connection (-1 indicates unknown/unspecified)
        /// </summary>
        public int TrustLevel { get; set; }

        /// <summary>
        /// The most-recent supported version of the Wolpteringer protocol supported by the client
        /// </summary>
        public int ProtocolVersion { get; set; }

        [DebuggerStepThrough]
        public ClientInfo()
        {
            this.TrustLevel = -1;
            this.Profiles = new List<Profile>();
        }


        public override string ToString()
        {
            string s = String.Format("ClientInfo {0}, TrustLevel {1}, ProtocolVersion {2}, Profiles ", JId, TrustLevel, ProtocolVersion);

            foreach (Profile p in Profiles)
            {
                s += p.ToString() + " ";
            }
            s.Trim();

            return s;
        }


        #region ISerializable Members



        

        public XElement Serialize()
        {
            XElement result = new XElement(XmlElementNames.ClientInfo);
            
            result.Add(new XElement(XmlElementNames.JId, this.JId));            
            result.Add(new XElement(XmlElementNames.TrustLevel, this.TrustLevel.ToString()));
            result.Add(new XElement(XmlElementNames.ProtocolVersion, this.ProtocolVersion));
            result.Add(new XElement(XmlElementNames.Profiles, this.Profiles.Select(x => new XElement(XmlElementNames.Profile, x))));
            
            return result;
        }

        public void Deserialize(XElement xmlData)
        {

            if (xmlData == null)
                throw new XmlException("'xmlData' is null");

            if (xmlData.Name.LocalName != "ClientInfo")
                throw new XmlException("Invalid root element");

            try
            {
                this.JId = xmlData.Element(XmlElementNames.JId).Value;
                this.TrustLevel = int.Parse(xmlData.Element(XmlElementNames.TrustLevel).Value);
                this.ProtocolVersion = int.Parse(xmlData.Element(XmlElementNames.ProtocolVersion).Value);

                var profiles = from elem in xmlData.Element(XmlElementNames.Profiles).Elements(XmlElementNames.Profile)
                               select (Profile)(Enum.Parse(typeof(Profile), elem.Value, true));

                this.Profiles = profiles.ToList<Profile>();
            }
            catch (NullReferenceException ex)
            {
                logger.Error(ex);
                throw new XmlException("Could not parse xml");
            }
        }


        private static class XmlElementNames
        {
            public static string XmlNamespace = "http://nerdcave.eu/wolpertinger";

            public static XName ClientInfo = XName.Get("ClientInfo", XmlNamespace);

            public static XName JId = XName.Get("JId", XmlNamespace);
            public static XName TrustLevel = XName.Get("TrustLevel", XmlNamespace);
            public static XName ProtocolVersion = XName.Get("ProtocolVersion", XmlNamespace);
            public static XName Profiles = XName.Get("Profiles", XmlNamespace);
            public static XName Profile = XName.Get("Profile", XmlNamespace);
        }

        #endregion
    }



    public enum Profile
    {
        ManagementClient,
        FileServer,
    }
}
