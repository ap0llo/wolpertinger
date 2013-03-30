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
using Nerdcave.Common.IO;
using Nerdcave.Common.Xml;
using Slf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Wolpertinger.FileShareCommon
{
    /// <summary>
    /// Class encapsulating information about a directory and it's files
    /// </summary>
    public class DirectoryObject : FilesystemObject
    {
        private class XmlNamesExtended : XmlNames
        {            
            public static XName DirectoryObject;
            public static XName FileObject;
            public static XName Files;
            public static XName Directories;

            public static void Init(string xmlNamespace)
            {
                DirectoryObject = XName.Get("DirectoryObject", xmlNamespace);
                FileObject = XName.Get("FileObject", xmlNamespace);
                Files = XName.Get("Files", xmlNamespace);
                Directories = XName.Get("Directories", xmlNamespace);
            }
        }


        private string _path;


        ILogger logger = LoggerService.GetLogger("DirectoryObject");

        protected Dictionary<string, FileObject> files = new Dictionary<string, FileObject>();
        protected Dictionary<string, DirectoryObject> directories = new Dictionary<string, DirectoryObject>();       


        public IEnumerable<FileObject> Files { get { return files.Values; } }

        public IEnumerable<DirectoryObject> Directories { get { return directories.Values; } }


        public DirectoryObject()
        {
            XmlNamesExtended.Init(xmlNamespace);
        }


        public override string Path 
        {
            get { return _path; }
            set
            {
                if (value != _path)
                {
                    _path = value;

                    foreach (var item in files.Values)
                    {
                        item.Path = System.IO.Path.Combine(value, item.Name).Replace("\\", "/");
                    }

                    foreach (var item in directories.Values)
                    {
                        item.Path = System.IO.Path.Combine(value, item.Name).Replace("\\", "/");
                    }
                }
            }
        }

        /// <summary>
        /// Adds a directory to the data-structure
        /// </summary>
        /// <param name="dir"></param>
        public void AddDirectory(DirectoryObject dir)
        {
            if (!files.ContainsKey(dir.Name.ToLower()) && !directories.ContainsKey(dir.Name.ToLower()))
            {
                directories.Add(dir.Name.ToLower(), dir);
                dir.Parent = this;
            }
        }


        /// <summary>
        /// Removes the specified directory, if it exists
        /// </summary>
        /// <param name="name">The directory to remove</param>
        public void RemoveDirectory(string name)
        {
            name = name.ToLower();
            if(directories.ContainsKey(name))
            {
                directories.Remove(name);
            }
        }

        /// <summary>
        /// Removes all directiories
        /// </summary>
        public void ClearDirectories()
        {
            directories.Clear();
        }

        /// <summary>
        /// Adds a file to the data-structure
        /// </summary>
        /// <param name="file"></param>
        public void AddFile(FileObject file)
        {
            if (!files.ContainsKey(file.Name.ToLower()) && !directories.ContainsKey(file.Name.ToLower()))
            {
                files.Add(file.Name.ToLower(), file);
                file.Parent = this;
            }
        }

        /// <summary>
        /// Removes the file with the specified name, if it exits
        /// </summary>
        /// <param name="name">The file to remove</param>
        public void RemoveFile(string name)
        {
            name = name.ToLower();
            if (files.ContainsKey(name))
            {
                files.Remove(name);
            }
        }

        /// <summary>
        /// Removes all files 
        /// </summary>
        public void ClearFiles()
        {
            files.Clear();
        }

        public DirectoryObject GetDirectory(string path)
        {
            return (DirectoryObject)getItem(path, true);
 
        }

        public FileObject GetFile(string path)
        {
            return (FileObject)getItem(path, false);

        }

        public virtual void LoadFromDisk(int depth = -1)
        {

            logger.Info("Loading Directory {0}", LocalPath);


            if (!Directory.Exists(this.LocalPath))
                throw new DirectoryNotFoundException(String.Format("Directory '{0}' not found", LocalPath));



            DirectoryInfo info = new DirectoryInfo(LocalPath);

            this.Name = info.Name;
            this.Created = info.CreationTimeUtc;
            this.LastEdited = info.LastWriteTimeUtc;


            //  ##  Update files    ##
            try
            {
                foreach (string item in Directory.GetFiles(LocalPath))
                {
                    string fileName = System.IO.Path.GetFileName(item);

                    if (files.ContainsKey(fileName))
                    {
                        files[fileName].LoadFromDisk();
                    }
                    else
                    {
                        FileObject newFile = new FileObject();
                        newFile.Name = fileName;
                        newFile.LocalPath = item;
                        AddFile(newFile);
                        newFile.LoadFromDisk();
                    }
                }
            }
            catch (DirectoryNotFoundException)
            {
                files = new Dictionary<string, FileObject>();
            }
               


            //TODO test if this really works!
            files.Values.Where(x => !File.Exists(LocalPath)).Select(x => files.Remove(x.Name));
                
            //  ##  Update directories ##
            try
            {
                foreach (string item in Directory.GetDirectories(LocalPath))
                {
                    try
                    {
                        string dirName = ExtendedPath.GetDirectoryName(item);

                        if (!directories.ContainsKey(dirName))
                        {
                            DirectoryObject newDir = new DirectoryObject();

                            newDir.Name = dirName;
                            newDir.LocalPath = item;

                            if (depth > 1 || depth < 0)
                            {
                                newDir.LoadFromDisk(depth - 1);
                            }

                            AddDirectory(newDir);
                        }
                    }
                    catch (IOException)
                    {
                    }
                }
            }
            catch (DirectoryNotFoundException)
            {
                directories = new Dictionary<string, DirectoryObject>();
            }


            directories.Values.Where(x => !Directory.Exists(x.LocalPath)).Select(x => directories.Remove(x.Name));
            directories.Values.Select(x => { x.LoadFromDisk(); return x; });
        }


        protected FilesystemObject getItem(string path, bool getDir)
        {
            DirectoryObject start = this;
            if(path.StartsWith("/"))
            {
                while(start.Parent != null)
                {
                    start = start.Parent;
                }
            }


            return start.getItem(path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries), 0, getDir);
        }

        /// <summary>
        /// Helper method used by GetFile() and GetDirectory()
        /// </summary>
        /// <param name="path"></param>
        /// <param name="firstIndex"></param>
        /// <param name="getDir"></param>
        /// <returns></returns>
        protected FilesystemObject getItem(string[] path, int firstIndex, bool getDir)
        {

            if (!path.Any() && getDir)
            {
                return this;
            }
            else if((!path.Any() && !getDir) || firstIndex >= path.Length)
            {
                if (getDir)
                    throw new DirectoryNotFoundException("The directory could not be located");
                else
                    throw new FileNotFoundException("The file could not be located");               
            }           

            if (path.Length-1 == firstIndex)
            {
                if (getDir)
                {
                    if (this.directories.ContainsKey(path[firstIndex].ToLower()))
                    {
                        return this.directories[path[firstIndex].ToLower()];
                    }
                    else if (path[firstIndex] == ".." && Parent != null)
                    {
                        return Parent;
                    }
                    else if ((path[firstIndex] == "/" || path[firstIndex] =="\\") && this.Name.IsNullOrEmpty())
                    {
                        return this;
                    }
                    else
                    {
                        throw new DirectoryNotFoundException(String.Format("The directory {0} could not be located", path[firstIndex]));
                    }
                }
                else
                {
                    if (this.files.ContainsKey(path[firstIndex].ToLower()))
                    {
                        return this.files[path[firstIndex].ToLower()];
                    }
                    else
                    {
                        throw new FileNotFoundException(String.Format("The file {0} could not be located", path[firstIndex]));
                    }
                }
                
            }
            else
            {
                if (this.directories.ContainsKey(path[firstIndex].ToLower()))
                {
                    return directories[path[firstIndex].ToLower()].getItem(path, firstIndex + 1, getDir);
                }
                else if (path[firstIndex] == ".." && Parent != null)
                {
                    return Parent.getItem(path, firstIndex + 1, getDir);
                }
                else
                {
                    if (getDir)
                        throw new DirectoryNotFoundException(String.Format("The directory {0} could not be located", path[firstIndex]));
                    else
                        throw new FileNotFoundException(String.Format("The file {0} could not be located", path[firstIndex]));
                }
            }
        }



        #region ISerializable Members

        protected override string rootElementName { get { return "DirectoryObject"; } }
        protected override string schemaTypeName { get { return "directoryObject"; } }
     
        public override XElement Serialize()
        {
            XElement xml = base.Serialize();

            xml.Name = XmlNamesExtended.DirectoryObject;
            
            //Serialize files in the directory
            var files = new XElement(XmlNamesExtended.Files);
            xml.Add(files);
            foreach (FileObject item in Files)
            {
                files.Add(item.Serialize());
            }

            //Serialize child directories
            var dirs = new XElement(XmlNamesExtended.Directories);
            xml.Add(dirs);
            foreach (DirectoryObject item in Directories)
            {
                dirs.Add(item.Serialize());
            }
            
            return xml;
        }

        public override void Deserialize(XElement xmlData)
        {

            base.Deserialize(xmlData);

            files = new Dictionary<string, FileObject>();

            foreach (XElement item in xmlData.Element(XmlNamesExtended.Files).Elements(XmlNamesExtended.FileObject))
            {
                FileObject newFile = new FileObject();
                newFile.Deserialize(item);
                files.Add(newFile.Name.ToLower(), newFile);
            }

            directories = new Dictionary<string, DirectoryObject>();
            foreach (XElement item in xmlData.Element(XmlNamesExtended.Directories).Elements(XmlNamesExtended.DirectoryObject))
            {
                DirectoryObject newDir = new DirectoryObject();
                newDir.Deserialize(item);
                directories.Add(newDir.Name.ToLower(), newDir);
            }
        }

        #endregion


        /// <summary>
        /// Clones the  directory object and all it's files and directories
        /// </summary>
        /// <returns>Returns a new DirectoryObjects which's member are all copies of this DirectoryObject</returns>
        public DirectoryObject Clone()
        {
            return Clone(-1);
        }

        /// <summary>
        /// Clones the directory and all it's files and directoris to a certain depth
        /// </summary>
        /// <param name="depth">Specifies how many levels to go down (items deeper in the hierachy will neither be cloned nor be referenced uncloned)</param>
        /// <returns>Returns a new DirectoryObjects which's member are all copies of this DirectoryObject</returns>
        public DirectoryObject Clone(int depth)
        {

            DirectoryObject copy = new DirectoryObject();
            
            //Clone ServiceName values
            copy.Created = this.Created;
            copy.LastEdited = this.LastEdited;
            copy.LocalPath = this.LocalPath;
            copy.Name = this.Name;
            copy.Parent = this.Parent;

            //Clone all child directories
            if (depth > 0)
            {
                foreach (DirectoryObject dir in directories.Values)
                {
                    copy.AddDirectory(dir.Clone(depth-1));
                }
            }

            //Clones files in this directory
            foreach (FileObject file in files.Values)
            {
                copy.AddFile(file.Clone());
            }

            return copy;
        }

    }
}