using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wolpertinger.FileShareCommon;

namespace Wolpertinger.Testing.XmlSerialization
{
    [TestClass]
    public class DirectoryObjectSerializationTests : XmlSerializationTest
    {
        [TestMethod]
        public void TestDirectoryObjectSerialization()
        {
            var dir = new DirectoryObject() { LocalPath = ".." };
            dir.LoadFromDisk();
            dir.Path = "/";

            var xml = dir.Serialize();
            
            var roundTrip = new DirectoryObject();
            Assert.IsTrue(roundTrip.Validate(xml));
            roundTrip.Deserialize(xml);

            assertAreEqual(dir, roundTrip);
        }

    }
}
