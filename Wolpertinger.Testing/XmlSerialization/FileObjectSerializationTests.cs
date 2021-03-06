﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Wolpertinger.FileShareCommon;

namespace Wolpertinger.Testing.XmlSerialization
{
    [TestClass]
    public class FileObjectSerializationTests : XmlSerializationTest
    {
        [TestMethod]
        public void TestFileObjectSerialization()
        {
            FileObject file = new FileObject();
            file.LocalPath = Directory.GetFiles(".").First();
            file.LoadFromDisk();

            file.Path = "/" + file.Name;

            var xml = file.Serialize();

            var strResult = xml.ToString();

            var roundtrip = new FileObject();
            Assert.IsTrue(roundtrip.Validate(xml));
            roundtrip.Deserialize(xml);

            assertAreEqual(file, roundtrip);
        }
    }
}
