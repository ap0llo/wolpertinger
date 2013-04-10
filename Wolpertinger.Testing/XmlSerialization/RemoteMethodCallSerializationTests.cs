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
    public class RemoteMethodCallSerializationTests : XmlSerializationTest
    {
        [TestMethod]
        public void TestRemoteMethodCallSerialization()
        {
            var call = new RemoteMethodCall();

            call.ComponentName = "component";
            call.CallId = Guid.NewGuid();
            call.MethodName = "sampleMethod";
            call.Parameters = new List<object> { 1, 2, 3 };

            var xml = call.Serialize();


            var roundtrip = new RemoteMethodCall();
            Assert.IsTrue(roundtrip.Validate(xml));
            roundtrip.Deserialize(xml);


            Assert.IsNotNull(roundtrip);
            Assert.AreEqual<Guid>(call.CallId, roundtrip.CallId);
            Assert.AreEqual<string>(call.ComponentName, roundtrip.ComponentName);
            Assert.AreEqual<string>(call.MethodName, roundtrip.MethodName);
            Assert.AreEqual<int>(call.Parameters.Count(), roundtrip.Parameters.Count());

            int count = call.Parameters.Count();
            for (int i = 0; i < count; i++)
            {
                Assert.AreEqual<int>((int)call.Parameters.Skip(i).First(), (int)roundtrip.Parameters.Skip(i).First());
            }

        }

        [TestMethod]
        public void TestRemoteMethodCallSerialization_NoParameters()
        {
            var call = new RemoteMethodCall();
            
            call.ComponentName = "component";
            call.CallId = Guid.NewGuid();
            call.MethodName = "sampleMethod";            

            var xml = call.Serialize();


            var roundtrip = new RemoteMethodCall();
            Assert.IsTrue(roundtrip.Validate(xml));
            roundtrip.Deserialize(xml);


            Assert.IsNotNull(roundtrip);
            Assert.AreEqual<Guid>(call.CallId, roundtrip.CallId);
            Assert.AreEqual<string>(call.ComponentName, roundtrip.ComponentName);
            Assert.AreEqual<string>(call.MethodName, roundtrip.MethodName);            
        }

        [TestMethod]
        public void TestRemoteMethodCallSerialization_MissingComponentNameElement()
        {
            var call = new RemoteMethodCall();

            call.ComponentName = "component";
            call.CallId = Guid.NewGuid();
            call.MethodName = "sampleMethod";
            call.Parameters = new List<object> { 1, 2, 3 };

            var xml = call.Serialize();

            xml.Element(XName.Get("ComponentName", xml.Name.NamespaceName)).Remove();

            var roundtrip = new RemoteMethodCall();
            Assert.IsFalse(roundtrip.Validate(xml));
        }

        [TestMethod]
        public void TestRemoteMethodCallSerialization_MissingCallIdElement()
        {
            var call = new RemoteMethodCall();

            call.ComponentName = "component";
            call.CallId = Guid.NewGuid();
            call.MethodName = "sampleMethod";
            call.Parameters = new List<object> { 1, 2, 3 };

            var xml = call.Serialize();

            xml.Element(XName.Get("CallId", xml.Name.NamespaceName)).Remove();

            var roundtrip = new RemoteMethodCall();
            Assert.IsFalse(roundtrip.Validate(xml));
        }

        [TestMethod]
        public void TestRemoteMethodCallSerialization_MissingMethodNameNameElement()
        {
            var call = new RemoteMethodCall();

            call.ComponentName = "component";
            call.CallId = Guid.NewGuid();
            call.MethodName = "sampleMethod";
            call.Parameters = new List<object> { 1, 2, 3 };

            var xml = call.Serialize();

            xml.Element(XName.Get("MethodName", xml.Name.NamespaceName)).Remove();

            var roundtrip = new RemoteMethodCall();
            Assert.IsFalse(roundtrip.Validate(xml));
        }

        [TestMethod]
        public void TestRemoteMethodCallSerialization_MissingParametersElement()
        {
            var call = new RemoteMethodCall();

            call.ComponentName = "component";
            call.CallId = Guid.NewGuid();
            call.MethodName = "sampleMethod";
            call.Parameters = new List<object> { 1, 2, 3 };

            var xml = call.Serialize();

            xml.Element(XName.Get("Parameters", xml.Name.NamespaceName)).Remove();

            var roundtrip = new RemoteMethodCall();
            Assert.IsFalse(roundtrip.Validate(xml));
        }

        [TestMethod]
        public void TestRemoteMethodCallValidation_NullorEmpty()
        {
            Assert.IsFalse(new RemoteMethodCall().Validate(null));            
        }

    }
}
