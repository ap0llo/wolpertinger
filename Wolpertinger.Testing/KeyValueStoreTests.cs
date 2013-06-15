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
using Nerdcave.Common.Xml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wolpertinger.FileShareCommon;

namespace Wolpertinger.Testing
{
    [TestClass]
    public class KeyValueStoreTests
    {

        KeyValueStore testFile;
        string testFileName;
        string key;

        [TestInitialize]
        public void Initialize()
        {
            testFileName = Path.GetRandomFileName();
            testFile = new KeyValueStore(testFileName);
            key = Path.GetRandomFileName();

            XmlSerializer.RegisterType(typeof(SnapshotInfo), "snapshotInfo");
        }


        [TestCleanup]
        public void Cleanup()
        {
            File.Delete(testFileName);
        }

        [TestMethod]
        public void TestSaveAndGet_int()
        {
            int value = new Random().Next();

            testFile.SaveItem(key, value);

            Assert.AreEqual<int>(value, testFile.GetItem<int>(key));
        }


        [TestMethod]
        public void TestSaveAndGet_DateTime()
        {
            Random random = new Random();
            DateTime value = DateTime.Now.AddDays(random.Next(1000)).AddMinutes(random.Next(1000));           

            testFile.SaveItem(key, value);


            Assert.AreEqual<DateTime>(value, testFile.GetItem<DateTime>(key));
        }


        [TestMethod]
        public void TestSaveAndGet_ISerializable()
        {
            var testObject = new SnapshotInfo() { Time = DateTime.Now };

            testFile.SaveItem(key, testObject);


            var roundTrip = testFile.GetItem<SnapshotInfo>(key);

            Assert.IsNotNull(roundTrip);
            Assert.AreEqual<Guid>(testObject.Id, roundTrip.Id);
            Assert.AreEqual<DateTime>(testObject.Time, roundTrip.Time.ToLocalTime());
        }

        [TestMethod]
        public void TestSaveAndGet_IEnumerable()
        {
            IEnumerable<int> testList = new List<int>() { 1, 2, 3, 4 };

            testFile.SaveItem(key, testList);

            var roundtrip = testFile.GetItem<IEnumerable>(key).Cast<int>();

            Assert.IsNotNull(testList);
            Assert.IsNotNull(roundtrip);

            Assert.AreEqual<int>(testList.Count(), roundtrip.Count());

            int count = testList.Count();
            for (int i = 0; i < count; i++)
            { 
                Assert.AreEqual<int>(testList.Skip(i).First(), roundtrip.Skip(i).First());
            }
        }

        [TestMethod]
        public void TestNullValue()
        {
            testFile.SaveItem(key, null);

            var returnValue = testFile.GetItem<object>(key);

            Assert.IsNull(returnValue);
        }

        [TestMethod]
        public void TestNotExistentKey()
        {
            Assert.IsNull(testFile.GetItem<object>(Path.GetRandomFileName()));
            Assert.AreEqual<int>(testFile.GetItem<int>(Path.GetRandomFileName()), 0);
            Assert.AreEqual<DateTime>(testFile.GetItem<DateTime>(Path.GetRandomFileName()), DateTime.MinValue);
        }


        [TestMethod, ExpectedException(typeof(TypeNotSupportedException))]
        public void TestNotSupportedType()
        {
            FileObject foo = new FileObject();
            testFile.SaveItem(key, foo);
        }
    }
}
