﻿/*

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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;
using Wolpertinger.Core;

namespace Wolpertinger.FileShareCommon
{
    /// <summary>
    /// Base class for FileObject and DirectoryObject
    /// </summary>
    [Serializable]
    public class FilesystemObject : Serializable
    {
        protected class XmlNames
        {
            private static bool initialized = false;

            public static XName FilesystemObject;
            public static XName Name;
            public static XName Path;
            public static XName LastEdited;
            public static XName Created;

            public static void Init(string xmlNamespace)
            {
                if (initialized)
                {
                    return;
                }

                FilesystemObject = XName.Get("FilesystemObject", xmlNamespace);
                Name = XName.Get("Name", xmlNamespace);
                Path = XName.Get("Path", xmlNamespace);
                LastEdited = XName.Get("LastEdited", xmlNamespace);
                Created = XName.Get("Created", xmlNamespace);

                initialized = true;
            }

        }


        private string _localPath;
        private DateTime _lastEdited;
        private DateTime _created;

       

        public DirectoryObject Parent { get; set; }

        public string LocalPath
        {
            get { return _localPath; }
            set
            {
                _localPath = value.Replace("\\", "/");
                if (_localPath.EndsWith("/"))
                    _localPath = _localPath.RemoveLastChar();
            }
        }

        public string Name { get; set; }

        public virtual string Path { get; set; }

        public DateTime LastEdited 
        {
            get
            {
                return _lastEdited;
            }
            set
            {
                _lastEdited = value;
            }
        }

        public DateTime Created 
        {
            get
            {
                return _created;
            }
            set { _created = value; } 
        }


        
        public FilesystemObject()
        {
            XmlNames.Init(xmlNamespace);
        }


        #region ISerializable Members

        protected override string rootElementName
        {
            get { throw new NotSupportedException("FilesystemObject should not be used on directly"); }
        }

        protected override string schemaTypeName
        {
            get { throw new NotSupportedException("FilesystemObject should not be used on directly"); }
        }


        public override XElement Serialize()
        {
            XElement xml = new XElement(XmlNames.FilesystemObject);

            xml.Add(new XElement(XmlNames.Name, this.Name));
            xml.Add(new XElement(XmlNames.Path, this.Path));
            xml.Add(new XElement(XmlNames.Created, XmlSerializer.Serialize(this.Created.ToUniversalTime()).Value));
            xml.Add(new XElement(XmlNames.LastEdited, XmlSerializer.Serialize(this.LastEdited.ToUniversalTime()).Value));

            return xml;
        }

        public override void Deserialize(XElement xmlData)
        {
            this.Name = xmlData.Element(XmlNames.Name).Value;
            this.Path = xmlData.Element(XmlNames.Path).Value;
            this.LastEdited = XmlSerializer.DeserializeAs<DateTime>(xmlData.Element(XmlNames.LastEdited)).ToUniversalTime();
            this.Created = XmlSerializer.DeserializeAs<DateTime>(xmlData.Element(XmlNames.Created)).ToUniversalTime();
        }

        #endregion
    }
}
