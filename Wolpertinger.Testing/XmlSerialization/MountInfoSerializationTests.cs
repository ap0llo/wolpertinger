using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wolpertinger.FileShareCommon;

namespace Wolpertinger.Testing.XmlSerialization
{
    [TestClass]
    public class MountInfoSerializationTests : XmlSerializationTest
    {

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



            var roundtrip = new MountInfo();
            Assert.IsTrue(roundtrip.Validate(xml));
            roundtrip.Deserialize(xml);

            Assert.IsNotNull(mountInfo);
            Assert.IsNotNull(roundtrip);


            Assert.AreEqual<string>(mountInfo.LocalPath, roundtrip.LocalPath);
            Assert.AreEqual<string>(mountInfo.MountPoint, roundtrip.MountPoint);
        }

    }
}
