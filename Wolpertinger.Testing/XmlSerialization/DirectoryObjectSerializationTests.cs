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


            var strResult = xml.ToString();

            validate(strResult, "DirectoryObject", "directoryObject");


            var roundTrip = new DirectoryObject();
            roundTrip.Deserialize(xml);

            assertAreEqual(dir, roundTrip);
        }

    }
}
