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
using System.IO;
using Nerdcave.Common.Extensions;
using System.Xml;

namespace Nerdcave.Common.Xml
{
    /// <summary>
    /// Implements a simple key-value-store
    /// Non-primitive types need to implement <see cref="Nerdcave.Common.Xml.Iserializable"/> in order to be saved
    /// </summary>
    public class KeyValueStore
    {


        private class XmlNames
        {
            public const string XmlNamespace = "http://nerdcave.eu/wolpertinger";

            public static XName Items = XName.Get("items", XmlNamespace);
            public static XName Object = XName.Get("object", XmlNamespace);
        }

        string filename;
        XDocument storageFile;

        /// <summary>
        /// Initializes a new instance of KeyValueStore with the specified file-name
        /// </summary>
        /// <param name="filename">The name of the key-value-store on disk</param>
        /// <exception cref="System.ArgumentException">The specified file-name was null or empty</exception>
        public KeyValueStore(string filename)
        {
            this.filename = filename;

            //check if filename is valid
            if (filename.IsNullOrEmpty())
                throw new ArgumentException(filename + " is no a valid filename");

            //if file already exists, try to load it
            if (File.Exists(filename))
            {
                try
                {
                    storageFile = XDocument.Load(filename);

                    if (storageFile.Root.Name != XmlNames.Items)
                        storageFile = null;
                }
                catch (XmlException)
                {
                    storageFile = null;
                }
            }

            //file does not exist yet => create file
            if (storageFile == null)
            {
                storageFile = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XComment("Wolpertinger KeyValueStore"));
                storageFile.Add(new XElement(XmlNames.Items));
                
                //save the xml file
                if(Directory.Exists(Path.GetDirectoryName(filename)))
                    storageFile.Save(filename);
            }
        }

        /// <summary>
        /// Saves an item to the key-value-store
        /// </summary>
        /// <param name="key">The item's key</param>
        /// <param name="value">The item to store</param>
        public void SaveItem(string key, object value)
        {
            if (value == null)
            {
                return;
            }

            //serialzie the item and add a 'key' attribute
            XElement xmlItem = XmlSerializer.Serialize(value, XmlNames.XmlNamespace);
            xmlItem.Add(new XAttribute("key", key));

            //check if item with that key already exists => overwrite it
            if (storageFile.Root.Elements(XmlNames.Object).Any() &&
                storageFile.Root.Elements(XmlNames.Object).Any(x => x.Attribute("key").Value == key))
            {
                var item = storageFile.Root.Elements(XmlNames.Object).First(x => x.Attribute("key").Value == key);

                item.Remove();
                storageFile.Root.Add(xmlItem);               
            }
            else
            {
                storageFile.Root.Add(xmlItem);
            }

            //save file to disk
            storageFile.Save(filename);
        }

        /// <summary>
        /// Gets an item from the key-value-store
        /// </summary>
        /// <typeparam name="T">The type of the item</typeparam>
        /// <param name="key">The item's key</param>
        /// <returns>Retunrs the item from the key-value-store or default(T) if the item could not be found</returns>
        public T GetItem<T>(string key)
        {
            //check if there are any items stored in the file
            if (!storageFile.Root.Elements(XmlNames.Object).Any())
                return default(T);

            //try to get the specified value
            var item = storageFile.Root.Elements(XmlNames.Object)
                        .Any(x => x.Attribute("key").Value == key)
                            ? storageFile.Root.Elements(XmlNames.Object).First(x => x.Attribute("key").Value == key) 
                            : null;
            
            //item could not be found => return default value
            if (item == null)
                return default(T);

            //try to deserialize the item
            object oItem = XmlSerializer.Deserialize(item);
            Type type = oItem.GetType();

            //return item if it could be derserialized to the specified type, otherwise return default value
            if (oItem is T)
                return (T)oItem;
            else
                return default(T);
        }


    }
}
