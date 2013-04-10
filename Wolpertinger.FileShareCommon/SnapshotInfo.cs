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
using Wolpertinger.Core;

namespace Wolpertinger.FileShareCommon
{
    /// <summary>
    /// Encapsulates information about a wolpertinger filesystem-snapshot
    /// </summary>
    [Serializable]
    public class SnapshotInfo : Serializable
    {

        private static class XmlNames
        {
            public static bool initialized = false;

            private const string _id = "Id";
            private const string _time = "Time";


            
            public static XName SnapshotInfo;
            public static XName Id;
            public static XName Time;

            public static void Init(string xmlNamespace)
            {
                if (initialized)
                {
                    return;
                }

                SnapshotInfo = XName.Get("SnapshotInfo", xmlNamespace);
                Id = XName.Get(_id, xmlNamespace);
                Time = XName.Get(_time, xmlNamespace);

                initialized = true;
            }
        }


        
        /// <summary>
        /// Initializes a new instace of SnapshotInfo
        /// </summary>
        public SnapshotInfo()
        {
            XmlNames.Init(xmlNamespace);

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

        protected override string schemaTypeName { get { return "snapshotInfo"; } }
        protected override string rootElementName { get { return "SnapshotInfo"; } }

        /// <summary>
        /// Serializes the object into XML
        /// </summary>
        /// <returns>Returns a XML representation of the object</returns>
        public override XElement Serialize()
        {
            XmlNames.Init(xmlNamespace);

            XElement result = new XElement(XmlNames.SnapshotInfo);
            result.Add(new XElement(XmlNames.Id, this.Id));
            result.Add(new XElement(XmlNames.Time, this.Time.ToUniversalTime().ToString("o")));

            return result;
        }

        /// <summary>
        /// Deserializes the given XML and sets the callee's Properties accordingly
        /// </summary>
        /// <param name="xmlData">The data to be deserialized</param>
        public override void Deserialize(XElement xmlData)
        {
            this.Id = Guid.Parse(xmlData.Element(XmlNames.Id).Value);
            this.Time = DateTime.Parse(xmlData.Element(XmlNames.Time).Value).ToUniversalTime();
        }
        
        #endregion
    }
}
