using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace Nerdcave.Common.Xml
{
    public abstract class XmlHelperBase
    {
        protected abstract string xmlNamespace { get; }
        protected abstract string schemaFile { get; }
        protected abstract string schemaTypeName { get; }
        protected abstract string rootElementName { get; }


        protected XmlSchemaSet schemaSet;


        public XmlHelperBase()
        {
            //Build a Xml-Schema for SnapshotInfo
            schemaSet = new XmlSchemaSet();

            XmlSchema schema;
            using (var reader = new FileStream(schemaFile, FileMode.Open))
            {
                schema = XmlSchema.Read(reader, null);
            }
            var schemaElement = new XmlSchemaElement()
            {
                Name = rootElementName,
                SchemaTypeName = new XmlQualifiedName(schemaTypeName, xmlNamespace)
            };
            schema.Items.Add(schemaElement);
            schemaSet.Add(schema);
        }
    }
}
