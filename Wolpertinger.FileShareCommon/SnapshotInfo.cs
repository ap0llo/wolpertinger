using Nerdcave.Common.Xml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Wolpertinger.FileShareCommon
{
    [Serializable]
    public class SnapshotInfo : ISerializable
    {
        public SnapshotInfo()
        {
            this.Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        public DateTime Time { get; set; }


        #region ISerializable Members

        private class XmlNames
        {
            protected const string xmlNamespace = "http://nerdcave.eu/wolpertinger";

            public static readonly XName SnapshotInfo = XName.Get("SnapshotInfo", xmlNamespace);
            public static readonly XName Id = XName.Get("Id", xmlNamespace);
            public static readonly XName Time = XName.Get("Time", xmlNamespace);
        }


        public XElement Serialize()
        {
            XElement result = new XElement(XmlNames.SnapshotInfo);
            result.Add(new XElement(XmlNames.Id, this.Id));
            result.Add(new XElement(XmlNames.Time, this.Time.ToUniversalTime().ToString("o")));

            return result;
        }

        public void Deserialize(XElement xmlData)
        {
            this.Id = Guid.Parse(xmlData.Element(XmlNames.Id).Value);
            this.Time = DateTime.Parse(xmlData.Element(XmlNames.Time).Value).ToUniversalTime();
        }
        
        #endregion
    }
}
