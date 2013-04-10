using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;
using Wolpertinger.Core;

namespace Wolpertinger.Testing.XmlSerialization
{
    [TestClass]
    public class RemoteMethodResponseSerializationTests : XmlSerializationTest
    {
        [TestMethod]
        public void TestRemoteMethodResponseSerialization()
        {
            var response = new RemoteMethodResponse();

            response.ComponentName = "component";
            response.CallId = Guid.NewGuid();
            response.ResponseValue = 5;

            var xml = response.Serialize();


            var roundtrip = new RemoteMethodResponse();
            Assert.IsTrue(roundtrip.Validate(xml));
            roundtrip.Deserialize(xml);

            Assert.IsNotNull(roundtrip);
            Assert.AreEqual<Guid>(response.CallId, roundtrip.CallId);
            Assert.AreEqual<string>(response.ComponentName, roundtrip.ComponentName);
            Assert.AreEqual<int>((int)response.ResponseValue, (int)roundtrip.ResponseValue);
        }


        [TestMethod]
        public void TestRemoteMethodResponseSerialization_NullResponse()
        {
            var response = new RemoteMethodResponse();

            response.ComponentName = "component";
            response.CallId = Guid.NewGuid();
            response.ResponseValue = null;

            var xml = response.Serialize();


            var roundtrip = new RemoteMethodResponse();
            Assert.IsTrue(roundtrip.Validate(xml));
            roundtrip.Deserialize(xml);

            Assert.IsNotNull(roundtrip);
            Assert.AreEqual<Guid>(response.CallId, roundtrip.CallId);
            Assert.AreEqual<string>(response.ComponentName, roundtrip.ComponentName);
            Assert.IsNull(roundtrip.ResponseValue);
        }

        [TestMethod]
        public void TestRemoteMethodResponseSerialization_MissingComponentNameElement()
        {
            var response = new RemoteMethodResponse();

            response.ComponentName = "component";
            response.CallId = Guid.NewGuid();
            response.ResponseValue = null;

            var xml = response.Serialize();
            xml.Elements(XName.Get("ComponentName", xml.Name.NamespaceName)).Remove();

            var roundtrip = new RemoteMethodResponse();
            Assert.IsFalse(roundtrip.Validate(xml));
        }


        [TestMethod]
        public void TestRemoteMethodResponseSerialization_MissingCallIdElement()
        {
            var response = new RemoteMethodResponse();

            response.ComponentName = "component";
            response.CallId = Guid.NewGuid();
            response.ResponseValue = null;

            var xml = response.Serialize();
            xml.Elements(XName.Get("CallId", xml.Name.NamespaceName)).Remove();

            var roundtrip = new RemoteMethodResponse();
            Assert.IsFalse(roundtrip.Validate(xml));
        }


        [TestMethod]
        public void TestRemoteMethodResponseSerialization_MissingResponseValueElement()
        {
            var response = new RemoteMethodResponse();

            response.ComponentName = "component";
            response.CallId = Guid.NewGuid();
            response.ResponseValue = null;

            var xml = response.Serialize();
            xml.Elements(XName.Get("ResponseValue", xml.Name.NamespaceName)).Remove();

            var roundtrip = new RemoteMethodResponse();
            Assert.IsFalse(roundtrip.Validate(xml));
        }


        [TestMethod]
        public void TestRemoteMethodResponseValidation_NullorEmpty()
        {
            Assert.IsFalse(new RemoteMethodResponse().Validate(null));            
        }

    }
}
