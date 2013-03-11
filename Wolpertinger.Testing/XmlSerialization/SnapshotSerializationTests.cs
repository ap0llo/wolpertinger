using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Wolpertinger.FileShareCommon;

namespace Wolpertinger.Testing.XmlSerialization
{
    [TestClass]
    public class SnapshotSerializationTests : XmlSerializationTest
    {

        [TestMethod]
        public void TestSnapshotInfoSerialization()
        {
            var snapshotInfo = new SnapshotInfo();
            snapshotInfo.Time = DateTime.Now.ToUniversalTime();

            var strResult = snapshotInfo.Serialize().ToString();

            validate(strResult, "SnapshotInfo", "snapshotInfo");


            var roundTrip = new SnapshotInfo();
            roundTrip.Deserialize(XElement.Parse(strResult));


            Assert.IsNotNull(snapshotInfo);
            Assert.IsNotNull(roundTrip);

            Assert.AreEqual<Guid>(snapshotInfo.Id, roundTrip.Id);
            Assert.AreEqual<DateTime>(snapshotInfo.Time, roundTrip.Time);

        }
    }
}
