/*

Licensed under the new BSD-License
 
Copyright (c) 2011-2013, Andreas Grünwald 
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

    Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
    Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
    Neither the name of the Wolpertinger project nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

using Nerdcave.Common.Extensions;
using Nerdcave.Common.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Wolpertinger.FileShareCommon
{
    /// <summary>
    /// 
    /// </summary>
    public class MountInfo : ISerializable
    {
        private class XmlHelper : XmlHelperBase
        {

            protected override string xmlNamespace { get { return "http://nerdcave.eu/wolpertinger"; } }
            protected override string schemaFile { get { return "complex.xsd"; } }
            protected override string schemaTypeName { get { return "mountInfo"; } }
            protected override string rootElementName { get { return "MountInfo"; } }


            public static XmlSchemaSet SchemaSet;

            public static XName MountInfo;
            public static XName MountPoint;
            public static XName LocalPath;


            public XmlHelper()
            {
                SchemaSet = schemaSet;

                MountInfo = XName.Get(rootElementName, xmlNamespace);
                MountPoint = XName.Get("MountPoint", xmlNamespace);
                LocalPath = XName.Get("LocalPath", xmlNamespace);
            }

        }


        public string MountPoint { get; set; }

        public string LocalPath { get; set; }


        static MountInfo()
        {
            //Initialize a instace of XmlHelper (this will set it's static members)
            XmlHelper xmlHelper = new XmlHelper();
        }

        public MountInfo()
        {
            this.MountPoint = "";
            this.LocalPath = "";
        }



        #region ISerializable Members




        public virtual bool Validate(XElement xml)
        {
            if (xml == null)
            {
                return false;
            }

            bool valid = true;
            new XDocument(xml).Validate(XmlHelper.SchemaSet, (s, e) => { valid = false; });
            return valid;
        }

        public XElement Serialize()
        {
            XElement xml = new XElement(XmlHelper.MountInfo);

            xml.Add(new XElement(XmlHelper.MountPoint, this.MountPoint));
            xml.Add(new XElement(XmlHelper.LocalPath, this.LocalPath));

            return xml;
        }

        public void Deserialize(XElement xmlData)
        {            
            this.MountPoint = xmlData.Element(XmlHelper.MountPoint).Value;
            this.LocalPath = xmlData.Element(XmlHelper.LocalPath).Value;
        }

        #endregion
    }
}
