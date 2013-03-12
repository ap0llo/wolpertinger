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
        public void TestClientInfoSerialization_Valid()
        {
            var clientInfo = new ClientInfo() { JId = "test@example.com", ProtocolVersion = 0, Profiles = new List<Profile>() { Profile.FileServer }, TrustLevel = 2 };

            var xml = clientInfo.Serialize();

            var roundTrip = new ClientInfo();

            Assert.IsTrue(roundTrip.Validate(xml));

            roundTrip.Deserialize(xml);

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

        [TestMethod]
        public void TestClientInfoSerialization_Invalid()
        {
            var clientInfo = new ClientInfo();

            var xml = clientInfo.Serialize();

            var roundTrip = new ClientInfo();

            Assert.IsFalse(roundTrip.Validate(clientInfo.Serialize()));
        }

    }
}
