using Nerdcave.Common.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Xml.Schema;
using System.IO;
using System.Xml;


namespace Wolpertinger.Core
{
    /// <summary>
    /// Abstract class implementing ISerializable offering useful functionality for classes
    /// (so this functionality doesn't need to be implemented in every class that offers serialization)
    /// </summary>
    [Serializable]
    public abstract class Serializable : ISerializable
    {
        [NonSerialized]
        protected XmlSchemaSet schemaSet;

        
        protected virtual string xmlNamespace { get { return "http://nerdcave.eu/wolpertinger"; } }       
        protected virtual string schemaFile { get { return "complex.xsd"; } }
        

        protected abstract string schemaTypeName { get; }
        protected abstract string rootElementName { get; }




        public Serializable()
        {
            //Build a Xml-Schema
            schemaSet = new XmlSchemaSet();

            XmlSchema schema;
            using (var reader = File.Open(schemaFile, FileMode.Open, FileAccess.Read, FileShare.Read))
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



        public virtual bool Validate(XElement xml)
        {
            if (xml == null)
            {
                return false;
            }

            var document = new XDocument(xml);

            try
            {
                document.Validate(schemaSet, null);
            }
            catch (XmlSchemaValidationException ex)
            {
                return false;
            }

            return true;
        }

        public abstract XElement Serialize();
        
        public abstract void Deserialize(XElement xmlData);

    }
}
