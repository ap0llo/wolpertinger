using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using Wolpertinger.Core;
using Nerdcave.Common;
using Nerdcave.Common.Extensions;
using Nerdcave.Common.Xml;

namespace Wolpertinger.FileShareCommon
{
    public class DirectoryObjectDiff : Serializable
    {


        public IEnumerable<string> FilesMissingLeft { get; private set; }

        public IEnumerable<string> FilesMissingRight { get; private set; }

        public IEnumerable<string> FileConflicts { get; private set; }

        public IEnumerable<string> DirectoriesMissingLeft { get; private set; }

        public IEnumerable<string> DirectoriesMissingRight { get; private set; }



        public DirectoryObjectDiff()
        {
            XmlNames.Init(xmlNamespace);
        }


        public bool IsEmpty()
        {
            return !(FilesMissingLeft.Any() || FilesMissingRight.Any() || FileConflicts.Any() ||
                DirectoriesMissingLeft.Any() || DirectoriesMissingRight.Any());
        }


        public static DirectoryObjectDiff GetDiff(DirectoryObject left, DirectoryObject right)
        {
            Dictionary<string, string> dirsLeft = new Dictionary<string, string>();
            Dictionary<string, string> filesLeft = new Dictionary<string, string>();
            Dictionary<string, string> dirsRight = new Dictionary<string, string>();
            Dictionary<string, string> filesRight = new Dictionary<string, string>();

            //flatten both trees
            flattenTree(left, dirsLeft, filesLeft);
            flattenTree(right, dirsRight, filesRight);

            //determine missing files
            var filesMissingLeft = filesRight.Where(r => !filesLeft.ContainsKey(r.Key)).Select(x => x.Value);
            var filesMissingRight = filesLeft.Where(l => !filesRight.ContainsKey(l.Key)).Select(x => x.Value);

            //determine missing directories
            var dirsMissingLeft = dirsRight.Where(r => !dirsLeft.ContainsKey(r.Key)).Select(x => x.Value);
            var dirsMissingRight = dirsLeft.Where(l => !dirsRight.ContainsKey(l.Key)).Select(x => x.Value);


            //determine file confilcts
            var filesLeftAndRight = filesLeft.Where(r => filesRight.ContainsKey(r.Key)).Select(x => x.Value);
            var conflictedFiles = filesLeftAndRight.Where(x => !fileObjectsEqual(left.GetFile(x), right.GetFile(x)));


            DirectoryObjectDiff diff = new DirectoryObjectDiff()
                {
                    DirectoriesMissingLeft = dirsMissingLeft,
                    DirectoriesMissingRight = dirsMissingRight,
                    FilesMissingLeft = filesMissingLeft,
                    FilesMissingRight = filesMissingRight,
                    FileConflicts = conflictedFiles
                };

            return diff;
        }

        
        private static void flattenTree(DirectoryObject directory, Dictionary<string, string> directories, Dictionary<string, string> files)
        {
            foreach (var file in directory.Files)
            {
                files.Add(file.Path.ToLower(), file.Path);
            }
            
            
            foreach (var dir in directory.Directories)
            {
                directories.Add(dir.Path.ToLower(), dir.Path);
                flattenTree(dir, directories, files);
            }
        }

        private static bool fileObjectsEqual(FileObject left, FileObject right)
        {
            return (left.Hash == right.Hash);
        }


        #region Serializable

        private static class XmlNames
        {
            private static bool initialized = false;

            public static XName DirectoryObjectDiff;
            public static XName FilesMissingLeft;
            public static XName FilesMissingRight;
            public static XName File;
            public static XName FileConflicts;
            public static XName DirectoriesMissingLeft;
            public static XName DirectoriesMissingRight;
            public static XName Directory;


            public static void Init(string xmlNamespace)
            {
                if (initialized)
                {
                    return;
                }

                DirectoryObjectDiff = XName.Get("DirectoryObjectDiff", xmlNamespace);
                FilesMissingLeft = XName.Get("FilesMissingLeft", xmlNamespace);
                FilesMissingRight = XName.Get("FilesMissingRight", xmlNamespace);
                FileConflicts = XName.Get("FileConflicts", xmlNamespace);
                File = XName.Get("File", xmlNamespace);
                DirectoriesMissingLeft = XName.Get("DirectoriesMissingLeft", xmlNamespace);
                DirectoriesMissingRight = XName.Get("DirectoriesMissingRight", xmlNamespace);
                Directory = XName.Get("Directory", xmlNamespace);
            }

        }


        protected override string schemaTypeName { get { return "directoryObjectDiff"; } }
        protected override string rootElementName { get { return "DirectoryObjectDiff"; } }

        public override XElement Serialize()
        {
            var result = new XElement(XmlNames.DirectoryObjectDiff);

            result.Add(new XElement(XmlNames.FilesMissingLeft, FilesMissingLeft.Select(x => new XElement(XmlNames.File, x))));
            result.Add(new XElement(XmlNames.FilesMissingRight, FilesMissingRight.Select(x => new XElement(XmlNames.File, x))));
            result.Add(new XElement(XmlNames.FileConflicts, FileConflicts.Select(x => new XElement(XmlNames.File, x))));

            result.Add(new XElement(XmlNames.DirectoriesMissingLeft, DirectoriesMissingLeft.Select(x => new XElement(XmlNames.Directory, x))));
            result.Add(new XElement(XmlNames.DirectoriesMissingRight, DirectoriesMissingRight.Select(x => new XElement(XmlNames.Directory, x))));

            return result;
        }


        public override void Deserialize(XElement xmlData)
        {
            this.FilesMissingLeft = xmlData.Element(XmlNames.FilesMissingLeft).Elements(XmlNames.File).Select(x => x.Value);
            this.FilesMissingRight = xmlData.Element(XmlNames.FilesMissingRight).Elements(XmlNames.File).Select(x => x.Value);
            this.FileConflicts = xmlData.Element(XmlNames.FileConflicts).Elements(XmlNames.File).Select(x => x.Value);

            this.DirectoriesMissingLeft = xmlData.Element(XmlNames.DirectoriesMissingLeft).Elements(XmlNames.Directory).Select(x => x.Value);
            this.DirectoriesMissingRight = xmlData.Element(XmlNames.DirectoriesMissingRight).Elements(XmlNames.Directory).Select(x => x.Value);

        }


        #endregion


    }
}
