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
using Wolpertinger.Core;

namespace Wolpertinger.FileShareCommon
{
    /// <summary>
    /// 
    /// </summary>
    public class MountInfo : Serializable
    {
        private static class XmlNames
        {
            private static bool initialized = false;       
            

            public static XName MountInfo;
            public static XName MountPoint;
            public static XName LocalPath;


            public static void Init(string xmlNamespace)
            {
                if (initialized)
                {
                    return;
                }

                MountInfo = XName.Get("MountInfo", xmlNamespace);
                MountPoint = XName.Get("MountPoint", xmlNamespace);
                LocalPath = XName.Get("LocalPath", xmlNamespace);
                initialized = true;
            }

        }


        public string MountPoint { get; set; }

        public string LocalPath { get; set; }

        

        public MountInfo()
        {
            XmlNames.Init(xmlNamespace);

            this.MountPoint = "";
            this.LocalPath = "";
        }



        #region ISerializable Members

        protected override string schemaTypeName { get { return "mountInfo"; } }
        protected override string rootElementName { get { return "MountInfo"; } }

        

        public override XElement Serialize()
        {
            XElement xml = new XElement(XmlNames.MountInfo);

            xml.Add(new XElement(XmlNames.MountPoint, this.MountPoint));
            xml.Add(new XElement(XmlNames.LocalPath, this.LocalPath));

            return xml;
        }

        public override void Deserialize(XElement xmlData)
        {            
            this.MountPoint = xmlData.Element(XmlNames.MountPoint).Value;
            this.LocalPath = xmlData.Element(XmlNames.LocalPath).Value;
        }

        #endregion
    }
}
