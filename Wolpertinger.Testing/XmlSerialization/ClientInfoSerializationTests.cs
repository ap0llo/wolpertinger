using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Schema;
using Wolpertinger.Core;

namespace Wolpertinger.Testing.XmlSerialization
{
    [TestClass]
    public class ClientInfoSerializationTests : XmlSerializationTest
    {
        [TestMethod]
        public void TestClientInfoSerialization1()
        {
            var clientInfo = new ClientInfo() { JId = "test@example.com", ProtocolVersion = 0, Profiles = new List<Profile>() { Profile.FileServer }, TrustLevel = 2 };

            testClientInfoSerialization(clientInfo);
        }

        [TestMethod, ExpectedException(typeof(XmlSchemaValidationException), AllowDerivedTypes = false)]
        public void TestClientInfoSerialization2()
        {
            var clientInfo = new ClientInfo();

            testClientInfoSerialization(clientInfo);
        }

        private void testClientInfoSerialization(ClientInfo clientInfo)
        {
            var xml = clientInfo.Serialize();

            var strResult = xml.ToString();

            var roundTrip = new ClientInfo();
            roundTrip.Deserialize(xml);

            validate(strResult, "ClientInfo", "clientInfo");

            Assert.IsNotNull(clientInfo);
            Assert.IsNotNull(roundTrip);

            Assert.AreEqual<string>(clientInfo.JId, roundTrip.JId);
            Assert.AreEqual<int>(clientInfo.ProtocolVersion, roundTrip.ProtocolVersion);
            Assert.AreEqual<int>(clientInfo.TrustLevel, roundTrip.TrustLevel);

            Assert.AreEqual<int>(clientInfo.Profiles.Count, roundTrip.Profiles.Count);

            for (int i = 0; i < clientInfo.Profiles.Count; i++)
            {
                Assert.AreEqual<Profile>(clientInfo.Profiles[i], roundTrip.Profiles[i]);
            }
        }
    }
}
