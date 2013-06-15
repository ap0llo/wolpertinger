/*

Licensed under the new BSD-License
 
Copyright (c) 2011-2013, Andreas Grünwald 
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

    Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
    Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer 
	in the documentation and/or other materials provided with the distribution.
    Neither the name of the Wolpertinger project nor the names of its contributors may be used to endorse or promote products 
	derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS 
BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, 
STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/
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
