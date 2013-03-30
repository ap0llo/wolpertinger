using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nerdcave.Common.Xml;
using Wolpertinger.FileShareCommon;
using System.Collections.Generic;
using System.Collections;

namespace Wolpertinger.Testing.XmlSerialization
{
    [TestClass]
    public class XmlSerializerTest
    {
        [TestInitialize]
        public void Initialize()
        {
            XmlSerializer.RegisterType(typeof(SnapshotInfo), "snapshotInfo");
        }

        [TestMethod]
        public void TestXmlSerializer_knownType()
        {
            int value = new Random().Next();

            var xml = XmlSerializer.Serialize(value);

            var roundTrip = (int)XmlSerializer.Deserialize(xml);

            Assert.AreEqual<int>(value, roundTrip);
        }


        [TestMethod]
        public void TestXmlSerializer_ISerializable()
        {
            var value = new SnapshotInfo() { Time = DateTime.Now.ToUniversalTime() };

            var xml = XmlSerializer.Serialize(value);

            var roundtrip = (SnapshotInfo)XmlSerializer.Deserialize(xml);

            Assert.IsNotNull(roundtrip);
            Assert.AreEqual<Guid>(value.Id, roundtrip.Id);
            Assert.AreEqual<DateTime>(value.Time, roundtrip.Time);

        }


        [TestMethod]
        public void TestXmlSerializer_IEnumerable()
        {
            IEnumerable<int> list = new List<int>() { 1, 2, 3, 4 };


            var xml = XmlSerializer.Serialize(list);

            var roundTrip = (XmlSerializer.Deserialize(xml) as IEnumerable).Cast<int>();


           
            Assert.IsNotNull(roundTrip);
            Assert.AreEqual<int>(list.Count(), roundTrip.Count());
            int count = list.Count();
            for (int i = 0; i < count; i++)
            {
                Assert.AreEqual<int>(list.Skip(i).First(), roundTrip.Skip(i).First());
            }
        }
    }
}
