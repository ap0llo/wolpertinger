/*

Licensed under the new BSD-License
 
Copyright (c) 2011-2013, Andreas Grünwald 
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

    Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
    Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer 
    in the documentation and/or other materials provided with the distribution.
    Neither the name of the Wolpertinger project nor the names of its contributors may be used to endorse or promote products derived
    from this software without specific prior written permission.

 THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS 
 BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
 GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT 
 LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

using Nerdcave.Common.Xml;
using Slf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Wolpertinger.Core
{
    /// <summary>
    /// Class is used to exchange information about wolpertinger clients
    /// </summary>
    public class ClientInfo : Serializable
    {

        static ILogger logger = LoggerService.GetLogger("ClientInfo");
        static XmlSchemaSet schemas = new XmlSchemaSet();


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
        /// The most-recent supported version of the wolpertinger protocol supported by the client
        /// </summary>
        public int ProtocolVersion { get; set; }

        /// <summary>
        /// Initializes a new instance of ClientInfo
        /// </summary>
        [DebuggerStepThrough]
        public ClientInfo()
        {
            this.TrustLevel = -1;
            this.Profiles = new List<Profile>();

            //Initialize XmlNames helper class 
            XmlNames.Init(xmlNamespace);   
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

        private sealed class XmlNames
        {

            private static bool initialized = false;

            private const string _clientInfo = "ClientInfo";
            private const string _jId = "JId";
            private const string _trustLevel = "TrustLevel";
            private const string _protocolVersion = "ProtocolVersion";
            private const string _profiles = "Profiles";
            private const string _profile = "Profile";


            public static XName ClientInfo;

            public static XName JId;
            public static XName TrustLevel;
            public static XName ProtocolVersion;
            public static XName Profiles;
            public static XName Profile;


            public static void Init(string xmlNamespace)
            {
                if (initialized)
                {
                    return;
                }

                ClientInfo = XName.Get(_clientInfo, xmlNamespace);
                JId = XName.Get(_jId, xmlNamespace);
                TrustLevel = XName.Get(_trustLevel, xmlNamespace);
                ProtocolVersion = XName.Get(_protocolVersion, xmlNamespace);
                Profiles = XName.Get(_profiles, xmlNamespace);
                Profile = XName.Get(_profile, xmlNamespace);

                initialized = true;
            }

        }

        protected override string schemaTypeName { get { return "clientInfo"; } }
        protected override string rootElementName { get { return "ClientInfo"; } }
        

        /// <summary>
        /// Serializes the object into XML
        /// </summary>
        /// <returns>Returns a XML representation of the object</returns>
        public override XElement Serialize()
        {
            XElement result = new XElement(XmlNames.ClientInfo);

            result.Add(new XElement(XmlNames.JId, this.JId));
            result.Add(new XElement(XmlNames.TrustLevel, this.TrustLevel.ToString()));
            result.Add(new XElement(XmlNames.ProtocolVersion, this.ProtocolVersion));
            result.Add(new XElement(XmlNames.Profiles, this.Profiles.Select(x => new XElement(XmlNames.Profile, x))));
            
            return result;
        }

        /// <summary>
        /// Deserializes the given XML and sets the callee's Properties accordingly
        /// </summary>
        /// <param name="xmlData">The data to be deserialized</param>
        public override void Deserialize(XElement xmlData)
        {

            if (xmlData == null)
                throw new XmlException("'xmlData' is null");

            if (xmlData.Name.LocalName != "ClientInfo")
                throw new XmlException("Invalid root element");

            try
            {
                this.JId = xmlData.Element(XmlNames.JId).Value;
                this.TrustLevel = int.Parse(xmlData.Element(XmlNames.TrustLevel).Value);
                this.ProtocolVersion = int.Parse(xmlData.Element(XmlNames.ProtocolVersion).Value);

                var profiles = xmlData.Element(XmlNames.Profiles).Elements(XmlNames.Profile);
                if(profiles.Any())
                {
                    this.Profiles = profiles.Select(x => (Profile)(Enum.Parse(typeof(Profile), x.Value, true))).ToList<Profile>();
                }
                else
                {
                    this.Profiles = new List<Profile>();
                }
            }
            catch (NullReferenceException ex)
            {
                logger.Error(ex);
                throw new XmlException("Could not parse xml", ex);
            }
        }



        #endregion
    }


    /// <summary>
    /// List of possible Profiles a client can support
    /// </summary>
    public enum Profile
    {
        ManagementClient,
        FileServer,
    }
}
