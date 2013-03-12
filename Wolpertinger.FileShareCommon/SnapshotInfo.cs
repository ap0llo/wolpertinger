using Nerdcave.Common.Xml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Wolpertinger.FileShareCommon
{
    /// <summary>
    /// Encapsulates information about a wolpertinger filesystem-snapshot
    /// </summary>
    [Serializable]
    public class SnapshotInfo : ISerializable
    {

        private class XmlHelper : XmlHelperBase
        {
            protected override string xmlNamespace { get { return "http://nerdcave.eu/wolpertinger"; } }
            protected override string schemaFile { get { return "complex.xsd"; } }
            protected override string schemaTypeName { get {return "snapshotInfo";}}
            protected override string rootElementName { get { return "SnapshotInfo"; } }


            private const string _id = "Id";
            private const string _time = "Time";


            public static XmlSchemaSet SchemaSet;

            public static XName SnapshotInfo;

            public static XName Id;
            public static XName Time;

            public XmlHelper()
            {
                SchemaSet = schemaSet;

                SnapshotInfo = XName.Get(rootElementName, xmlNamespace);
                Id = XName.Get(_id, xmlNamespace);
                Time = XName.Get(_time, xmlNamespace);                
            }
        }

        static SnapshotInfo()
        {
            //Initialize a instace of XmlHelper (this will set it's static members)
            XmlHelper xmlHelper = new XmlHelper();
        }
        
        /// <summary>
        /// Initializes a new instace of SnapshotInfo
        /// </summary>
        public SnapshotInfo()
        {
            this.Id = Guid.NewGuid();
        }

        /// <summary>
        /// The Snapshot's Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The time the Snapshot has been taken
        /// </summary>
        public DateTime Time { get; set; }


        #region ISerializable Members



        /// <summary>
        /// Validates a piece of XML and determines wheter it can be deserialized as SnapshotInfo
        /// </summary>
        /// <param name="xml">The XML to be validated</param>
        /// <returns>Returns true when the specified xml is valid and can be deserialzed into a instance of SnapshotInfo. Otherwise returns false</returns>
        public bool Validate(XElement xml)
        {
            if (xml == null)
            {
                return false;
            }

            var document = new XDocument(xml);
            bool valid = true;
            document.Validate(XmlHelper.SchemaSet, (s, e) => { valid = false; });

            return valid;
        }


        /// <summary>
        /// Serializes the object into XML
        /// </summary>
        /// <returns>Returns a XML representation of the object</returns>
        public XElement Serialize()
        {
            XElement result = new XElement(XmlHelper.SnapshotInfo);
            result.Add(new XElement(XmlHelper.Id, this.Id));
            result.Add(new XElement(XmlHelper.Time, this.Time.ToUniversalTime().ToString("o")));

            return result;
        }

        /// <summary>
        /// Deserializes the given XML and sets the callee's Properties accordingly
        /// </summary>
        /// <param name="xmlData">The data to be deserialized</param>
        public void Deserialize(XElement xmlData)
        {
            this.Id = Guid.Parse(xmlData.Element(XmlHelper.Id).Value);
            this.Time = DateTime.Parse(xmlData.Element(XmlHelper.Time).Value).ToUniversalTime();
        }
        
        #endregion
    }
}
