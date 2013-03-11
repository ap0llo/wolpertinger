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
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Nerdcave.Common.Extensions;
using Nerdcave.Common.Xml;
using Nerdcave.Common.IO;
using System.Threading;
using Nerdcave.Common;
using Wolpertinger.Core;

namespace Wolpertinger.FileShareCommon
{
    /// <summary>
    /// Class encapsulating information about a file
    /// </summary>
    public class FileObject : FilesystemObject
    {
        public static IHashingService HashingService;


        public String Hash { get; set; }


        public virtual void LoadFromDisk()
        {
            if (!LocalPath.IsNullOrEmpty() && File.Exists(LocalPath))
            {
                FileInfo info = new FileInfo(LocalPath);
                this.Name = info.Name;
                this.Created = info.CreationTimeUtc;
                this.LastEdited = info.LastWriteTimeUtc;
                this.Hash = HashingService.GetHash(this.LocalPath, Priority.High);
            }
            else
            {
                throw new FileNotFoundException("FileObject: Could not find file: {0}", this.LocalPath);
            }
        }


        public FileObject Clone()
        {
            FileObject copy = new FileObject();

            //Copy ServiceName values
            copy.Created = this.Created;
            copy.Hash = this.Hash;
            copy.LastEdited = this.LastEdited;
            copy.LocalPath = this.LocalPath;
            copy.Name = this.Name;
            copy.Parent = this.Parent;

            return copy;
        }


        #region ISerializable Members

        private class XmlElementNamesExtended : XmlElementNames
        {
            public static XName FileObject = XName.Get("FileObject", XmlNamespace);
            public static XName Hash = XName.Get("Hash", XmlNamespace);
        }

        public override XElement Serialize()
        {           
            XElement xml = base.Serialize();

            xml.Name = XmlElementNamesExtended.FileObject;
            xml.Add(new XElement(XmlElementNamesExtended.Hash));
            xml.Element(XmlElementNamesExtended.Hash).Add(new XAttribute("hashtype", "sha1"));
            xml.Element(XmlElementNamesExtended.Hash).Value = (this.Hash == null) ? "" : this.Hash;

            return xml;
        }

        public override void Deserialize(XElement xmlData)
        {
            base.Deserialize(xmlData);

            this.Hash = xmlData.Element(XmlElementNamesExtended.Hash).Value;

            if (xmlData.Element(XmlElementNamesExtended.Hash).Attribute("hashtype").Value != "sha1")
                throw new Exception("FileObject: Unsupported Hash-Type encountered");
        }

        #endregion





        private void HashingHelper_GetHashAsyncCompleted(object sender, GetHashEventArgs e)
        {
            if (e.Path.ToLower() == LocalPath.ToLower())
            {
                this.Hash = e.Hash;
            }
        }

    }

}
