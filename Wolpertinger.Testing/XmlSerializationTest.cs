using System;
using System.Xml.Linq;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wolpertinger.Core;
using System.Collections.Generic;
using System.Xml.Schema;
using Nerdcave.Common.Extensions;
using Wolpertinger.FileShareCommon;
using System.IO;

namespace Wolpertinger.Testing
{
    [TestClass]
    public class XmlSerializationTest
    {

        string schemaFileName = @"..\..\..\Resources\complex.xsd";
        XmlSchemaSet schemas = new XmlSchemaSet();

        [TestInitialize]
        public void Initialize()
        {
            schemas.Add("http://nerdcave.eu/wolpertinger", schemaFileName);


            FileObject.HashingService = new TestHashingService();
        }



        [TestMethod]
        public void TestClientInfoSerialization1()
        {
            var clientInfo = new ClientInfo() { JId = "test@example.com", ProtocolVersion = 0, Profiles = new List<Profile>() { Profile.FileServer }, TrustLevel = 2 };

            testClientInfoSerialization(clientInfo);
        }

        [TestMethod, ExpectedException(typeof(XmlSchemaValidationException),AllowDerivedTypes=false)]
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

            XDocument.Parse(strResult).Validate(schemas, null);

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
        public void TestMountInfoSerialization1()
        {
            var mountInfo = new MountInfo() { LocalPath = "/foo/bar", MountPoint = "/bar/foo" };
            testMountInfoSerialization(mountInfo);
        }

        [TestMethod]
        public void TestMountInfoSerialization2()
        {
            testMountInfoSerialization(new MountInfo());
        }

        private void testMountInfoSerialization(MountInfo mountInfo)
        {
            var xml = mountInfo.Serialize();

            var strResult = xml.ToString();


            XDocument.Parse(strResult).Validate(schemas, null);

            var roundtrip = new MountInfo();
            roundtrip.Deserialize(xml);

            Assert.IsNotNull(mountInfo);
            Assert.IsNotNull(roundtrip);


            Assert.AreEqual<string>(mountInfo.LocalPath, roundtrip.LocalPath);
            Assert.AreEqual<string>(mountInfo.MountPoint, roundtrip.MountPoint);
        }



        [TestMethod]
        public void TestPermissionSerializsation()
        {
            var permission = new Permission() { Path = "/foo/bar", PermittedClients = new List<string>() { "client1@example.com", "client2@foo.org" } };

            var xml = permission.Serialize();

            var strResult = xml.ToString();


            XDocument.Parse(strResult).Validate(schemas, null);

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


        [TestMethod]
        public void TestFileObjectSerialization()
        {
            FileObject file = new FileObject();
            file.LocalPath = Directory.GetFiles(".").First();
            file.LoadFromDisk();


            var xml = file.Serialize();

            var strResult = xml.ToString();

            XDocument.Parse(strResult).Validate(schemas, null);

            var roundtrip = new FileObject();
            roundtrip.Deserialize(xml);

            assertAreEqual(file, roundtrip);

        }


        private void assertAreEqual(FileObject file1, FileObject file2)
        {
            Assert.AreEqual<DateTime>(file1.Created, file2.Created);
            Assert.AreEqual<DateTime>(file1.LastAccessed, file2.LastAccessed);
            Assert.AreEqual<DateTime>(file1.LastEdited, file2.LastEdited);
            Assert.AreEqual<string>(file1.Hash, file2.Hash);
            Assert.AreEqual<string>(file1.Name, file2.Name);
        }

    }


    public class TestHashingService : IHashingService
    {

        public event EventHandler<GetHashEventArgs> GetHashAsyncCompleted;

        public string GetHash(string filename, Nerdcave.Common.Priority priority)
        {
            return "SomeString".GetHashSHA1();
        }

        public void GetHashAsync(string filename, Nerdcave.Common.Priority priority)
        {
            if (this.GetHashAsyncCompleted != null)
                this.GetHashAsyncCompleted(this, new GetHashEventArgs() { Path = filename, Hash = GetHash(filename, priority) });
        }
    }
}
