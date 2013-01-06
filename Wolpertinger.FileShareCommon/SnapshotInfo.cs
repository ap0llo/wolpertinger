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
    public class SnapshotInfo : ISerializable
    {
        public SnapshotInfo()
        {
            this.Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        public DateTime Time { get; set; }

        public XElement Serialize()
        {
            XElement result = new XElement("SnapshotInfo");
            result.Add(new XElement("Id") { Value = this.Id.ToString() });
            result.Add(new XElement("Time") { Value = this.Time.ToUniversalTime().ToString(CultureInfo.CreateSpecificCulture("en-gb"))});

            return result;
        }

        public object Deserialize(XElement xmlData)
        {
            var result = new SnapshotInfo();

            result.Id = Guid.Parse(xmlData.Element("Id").Value);
            result.Time = DateTime.Parse(xmlData.Element("Time").Value, CultureInfo.CreateSpecificCulture("en-gb"));

            return result;
        }
    }
}
