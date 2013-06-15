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
