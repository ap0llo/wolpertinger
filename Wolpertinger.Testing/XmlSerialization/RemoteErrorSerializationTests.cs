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
    public class RemoteErrorSerializationTests : XmlSerializationTest
    {
        [TestMethod]
        public void TestRemoteErrorSerialization_withCallId()
        {
            var error = new RemoteError();
            error.CallId = Guid.NewGuid();

            error.ComponentName = "component";


            var xml = error.Serialize();


            var roundtrip = new RemoteError();
            Assert.IsTrue(roundtrip.Validate(xml));

            roundtrip.Deserialize(xml);


            Assert.IsNotNull(roundtrip);
            Assert.AreEqual<Guid>(error.CallId, roundtrip.CallId);
            Assert.AreEqual<string>(error.ComponentName, roundtrip.ComponentName);
            Assert.AreEqual<RemoteErrorCode>(error.ErrorCode, roundtrip.ErrorCode);
        }



        [TestMethod]
        public void TestRemoteErrorSerialization_NoCallId()
        {
            var error = new RemoteError();
            error.ComponentName = "component";

            var xml = error.Serialize();


            var roundtrip = new RemoteError();
            Assert.IsTrue(roundtrip.Validate(xml));
            roundtrip.Deserialize(xml);

            Assert.IsNotNull(roundtrip);
            Assert.AreEqual<Guid>(error.CallId, roundtrip.CallId);
            Assert.AreEqual<string>(error.ComponentName, roundtrip.ComponentName);
            Assert.AreEqual<RemoteErrorCode>(error.ErrorCode, roundtrip.ErrorCode);
        }


        [TestMethod]
        public void TestRemoteErrorSerialization_MissingComponentNameElement()
        {
            var error = new RemoteError();
            error.ComponentName = "component";

            var xml = error.Serialize();
            xml.Element(XName.Get("ComponentName", xml.Name.NamespaceName)).Remove();

            var roundtrip = new RemoteError();
            Assert.IsFalse(roundtrip.Validate(xml));            
        }


        [TestMethod]
        public void TestRemoteErrorSerialization_MissingErrorCodeElement()
        {
            var error = new RemoteError();
            error.ComponentName = "component";

            var xml = error.Serialize();
            xml.Element(XName.Get("ErrorCode", xml.Name.NamespaceName)).Remove();

            var roundtrip = new RemoteError();
            Assert.IsFalse(roundtrip.Validate(xml));
        }


        [TestMethod]
        public void TestRemoteErrorValidation_NullorEmpty()
        {
            Assert.IsFalse(new RemoteError().Validate(null));
        }

    }
}
