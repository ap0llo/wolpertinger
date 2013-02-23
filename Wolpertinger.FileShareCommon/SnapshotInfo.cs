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


        #region ISerializable Members

        public XElement Serialize()
        {
            XElement result = new XElement("SnapshotInfo");
            result.Add(new XElement("Id", this.Id));
            result.Add(new XElement("Time", this.Time.ToUniversalTime().ToString("o")));

            return result;
        }

        public void Deserialize(XElement xmlData)
        {
            this.Id = Guid.Parse(xmlData.Element("Id").Value);
            this.Time = DateTime.Parse(xmlData.Element("Time").Value);//, CultureInfo.CreateSpecificCulture("en-gb"));
        }
        
        #endregion
    }
}
