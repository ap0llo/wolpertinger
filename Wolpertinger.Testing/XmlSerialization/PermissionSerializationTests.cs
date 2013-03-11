using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wolpertinger.FileShareCommon;

namespace Wolpertinger.Testing.XmlSerialization
{
    [TestClass]
    public class PermissionSerializationTests : XmlSerializationTest
    {
        [TestMethod]
        public void TestPermissionSerializsation()
        {
            var permission = new Permission() { Path = "/foo/bar", PermittedClients = new List<string>() { "client1@example.com", "client2@foo.org" } };

            var xml = permission.Serialize();

            var strResult = xml.ToString();


            validate(strResult, "Permission", "permission");

            var roundTrip = new Permission();
            roundTrip.Deserialize(xml);


            Assert.IsNotNull(permission);
            Assert.IsNotNull(roundTrip);

            Assert.AreEqual<string>(permission.Path, roundTrip.Path);
            Assert.AreEqual<int>(permission.PermittedClients.Count, roundTrip.PermittedClients.Count);


            for (int i = 0; i < permission.PermittedClients.Count; i++)
            {
                Assert.AreEqual<string>(permission.PermittedClients[i], roundTrip.PermittedClients[i]);
            }
        }

    }
}
