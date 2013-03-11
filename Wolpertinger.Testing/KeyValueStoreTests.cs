using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nerdcave.Common.Xml;
using System.IO;
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
