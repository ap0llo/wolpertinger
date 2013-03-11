using System;
using System.Xml.Linq;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wolpertinger.Core;
using System.Collections.Generic;
using System.Xml.Schema;
using Nerdcave.Common.Extensions;
using Wolpertinger.FileShareCommon;
using System.IO;
using System.Xml;

namespace Wolpertinger.Testing.XmlSerialization
{
    [TestClass]
    public class XmlSerializationTest
    {

        string schemaFileName = @"..\..\..\Resources\complex.xsd";
        XmlSchema schema;
        XmlSchemaSet schemas = new XmlSchemaSet();

        [TestInitialize]
        public void Initialize()
        {
            //schemas.Add("http://nerdcave.eu/wolpertinger", schemaFileName);

            schemas = new XmlSchemaSet();
            schemas.Add("http://nerdcave.eu/wolpertinger", @"..\..\..\Resources\primitive.xsd");
            using (var reader = new FileStream(schemaFileName, FileMode.Open))
            {
                schema = XmlSchema.Read(reader, null);                
            }


            FileObject.HashingService = new TestHashingService();
        }






        protected void assertAreEqual(FileObject file1, FileObject file2)
        {
            Assert.AreEqual<DateTime>(file1.Created, file2.Created);
            Assert.AreEqual<DateTime>(file1.LastEdited, file2.LastEdited);
            Assert.AreEqual<string>(file1.Hash, file2.Hash);
            Assert.AreEqual<string>(file1.Name, file2.Name);
            Assert.AreEqual<string>(file1.Path, file2.Path);
            Assert.IsFalse(file1.Name.IsNullOrEmpty());
        }

        protected void assertAreEqual(DirectoryObject dir1, DirectoryObject dir2)
        {
            Assert.IsNotNull(dir1);
            Assert.IsNotNull(dir2);

            Assert.AreEqual<DateTime>(dir1.Created, dir2.Created);
            Assert.AreEqual<DateTime>(dir1.LastEdited, dir2.LastEdited);
            Assert.AreEqual<string>(dir1.Name, dir2.Name);
            Assert.AreEqual<string>(dir1.Path, dir2.Path);
            Assert.IsFalse(dir1.Name.IsNullOrEmpty());


            var dir1Dirs = dir1.Directories.ToList<DirectoryObject>();
            var dir2Dirs = dir2.Directories.ToList<DirectoryObject>();

            Assert.AreEqual<int>(dir1Dirs.Count, dir2Dirs.Count);
            for (int i = 0; i < dir1Dirs.Count; i++)
            {
                assertAreEqual(dir1Dirs[i], dir2Dirs[i]);
            }


            var dir1Files = dir1.Files.ToList<FileObject>();
            var dir2Files = dir2.Files.ToList<FileObject>();

            Assert.AreEqual<int>(dir1Files.Count, dir2Files.Count);

            for (int i = 0; i < dir1Files.Count; i++)
            {
                assertAreEqual(dir1Files[i], dir2Files[i]);
            }

        }


        protected void validate(string xml, string elementName, string type)
        {
            var schemaElement = new XmlSchemaElement()
            {
                Name = elementName,
                SchemaTypeName = new XmlQualifiedName(type, "http://nerdcave.eu/wolpertinger")
            };
            schema.Items.Add(schemaElement);
            schemas.Add(schema);
            XDocument.Parse(xml).Validate(schemas, null);

            schema.Items.Remove(schemaElement);
        }

    }



}
