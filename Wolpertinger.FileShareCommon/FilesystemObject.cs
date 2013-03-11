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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Nerdcave.Common.Xml;
using Nerdcave.Common.Extensions;
using System.Globalization;

namespace Wolpertinger.FileShareCommon
{
    /// <summary>
    /// Base class for FileObject and DirectoryObject
    /// </summary>
    public class FilesystemObject : ISerializable
    {

        private string _localPath;
        private DateTime _lastEdited;
        private DateTime _created;
        private DateTime _lastAccessed;

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


        

        #region ISerializable Members

        protected class XmlElementNames
        {
            protected const string XmlNamespace = "http://nerdcave.eu/wolpertinger";
            public static XName FilesystemObject = XName.Get("FilesystemObject", XmlNamespace);
            public static XName Name = XName.Get("Name", XmlNamespace);
            public static XName Path = XName.Get("Path", XmlNamespace);
            public static XName LastEdited = XName.Get("LastEdited", XmlNamespace);
            public static XName Created = XName.Get("Created", XmlNamespace);

        }
    
        public virtual XElement Serialize()
        {
            XElement xml = new XElement(XmlElementNames.FilesystemObject);

            xml.Add(new XElement(XmlElementNames.Name, this.Name));
            xml.Add(new XElement(XmlElementNames.Path, this.Path));
            xml.Add(new XElement(XmlElementNames.Created, XmlSerializer.Serialize(this.Created.ToUniversalTime()).Value));
            xml.Add(new XElement(XmlElementNames.LastEdited, XmlSerializer.Serialize(this.LastEdited.ToUniversalTime()).Value));

            return xml;
        }

        public virtual void Deserialize(XElement xmlData)
        {
            this.Name = xmlData.Element(XmlElementNames.Name).Value;
            this.Path = xmlData.Element(XmlElementNames.Path).Value;
            this.LastEdited = XmlSerializer.DeserializeAs<DateTime>(xmlData.Element(XmlElementNames.LastEdited)).ToUniversalTime();
            this.Created = XmlSerializer.DeserializeAs<DateTime>(xmlData.Element(XmlElementNames.Created)).ToUniversalTime();
        }

        #endregion
    }
}
