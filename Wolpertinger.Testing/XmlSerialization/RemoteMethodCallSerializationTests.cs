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
