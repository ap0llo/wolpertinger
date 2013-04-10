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
    public class DirectoryObjectDiff// TODO : ISerializable
    {
        /// <summary>
        /// Name that identifies the 'left' DirectoryObject that has been compared
        /// </summary>
        public string LeftName { get; set; }

        /// <summary>
        /// Name that identifies the 'right' DirectoryObject that has been compared
        /// </summary>
        public string RightName { get; set; }


        /// <summary>
        /// Files (unordered) from the Directory (including all subdirectories) that have been found on the right
        /// but not on the left (not including confilcts)
        /// </summary>
        public IEnumerable<FileObject> FilesMissingFromLeft { get; set; }

        /// <summary>
        /// Files (unordered) from the Directory (including all subdirectories) that have been found on the left
        /// but not on the right (not including confilcts)
        /// </summary>
        public IEnumerable<FileObject> FilesMissingFromRight { get; set; }

        /// <summary>
        /// Files that exist in both the left and the right directories that have been compared 
        /// where at least one property differs
        /// </summary>
        public IEnumerable<Tuple<FileObject, FileObject>> FileConfilcts { get; set; }

        /// <summary>
        /// Directories (without any subdirectorie or files) that are present in the right directory but not in the left (unordered)
        /// </summary>
        public IEnumerable<DirectoryObject> DirectoriesMissigFromLeft { get; set; }

        /// <summary>
        /// Directories (without any subdirectorie or files) that are present in the left directory but not in the right (unordered)
        /// </summary>
        public IEnumerable<DirectoryObject> DirectoriesMissingFromRight { get; set; }


        #region ISerializable Members

        public XElement Serialize()
        {
            throw new NotImplementedException();
        }

        public object Deserialize(XElement xmlData)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Static Members


        public static DirectoryObjectDiff CompareDirectories(DirectoryObject left, DirectoryObject right, string leftName = "", string rightName = "")
        {
            var result = new DirectoryObjectDiff() { LeftName = leftName, RightName = rightName };


            compareDirectoriesHelper(left, right, result);
            

            return result;
        }


        private static void compareDirectoriesHelper(DirectoryObject left, DirectoryObject right, DirectoryObjectDiff result)
        {

            //      ##  Items missing in left Directory  ##

            //find directories that exist in right but not in left
            var dirsMissingLeft = right.Directories.Where(x => !left.Directories.Any(y => y.Name.ToLower() == x.Name.ToLower()))                                                
                                                   .Select(x => getAllSubdirectories(x))   
                                                   .Aggregate((l1, l2) => l1.Union<DirectoryObject>(l2));

            //find files that exist in right but not in left
            var filesMissingLeft = right.Files.Where(x => !left.Files.Any(y => y.Name.ToLower() == x.Name.ToLower()));
            //add all files from all the directories missing from left
            filesMissingLeft = filesMissingLeft.Union(dirsMissingLeft.Select(x => x.Files).Aggregate((l1, l2) => l1.Union(l2)));

            //copy directories in the list and remove the files
            dirsMissingLeft = dirsMissingLeft.Select(x => { var copy = x.Clone(0); copy.ClearFiles(); return copy; });



            //      ##  Items missing in right Directory    ##  

            //find directories that exist in left but not in right
            var dirsMissingRight = left.Directories.Where(x => !right.Directories.Any(y => y.Name.ToLower() == x.Name.ToLower()))
                                                   .Select(x => getAllSubdirectories(x))
                                                   .Aggregate((l1, l2) => l1.Union<DirectoryObject>(l2));
                                                  

            //find files that exist in left but not in right
            var filesMissingRight = left.Files.Where(x => !right.Files.Any(y => y.Name.ToLower() == x.Name.ToLower()));
            //add all files from all the directories missing from right
            filesMissingRight = filesMissingRight.Union(dirsMissingRight.Select(x => x.Files).Aggregate((l1, l2) => l1.Union(l2)));

            //make dirsMissingLeft (list of trees) to a simple list of directories (minus files)
            dirsMissingRight = dirsMissingRight.Select(x => { var copy = x.Clone(0); copy.ClearFiles(); return copy; });



            //      ##  Conflicts   ##

            //find files that exist in both directories but are not equal
            var fileConflicts = right.Files.Where(x => left.Files.Any(y => y.Name.ToLower() == x.Name.ToLower()))
                                           .Select(x => new Tuple<FileObject, FileObject>(left.GetFile(x.Name), x))
                                           .Where(x => !compareFileObject(x.Item1, x.Item2));


            //add the newy-found conflicts to the existing result
            result.DirectoriesMissigFromLeft = result.DirectoriesMissigFromLeft.Union<DirectoryObject>(dirsMissingLeft).ToList<DirectoryObject>();
            result.DirectoriesMissingFromRight = result.DirectoriesMissingFromRight.Union<DirectoryObject>(dirsMissingRight).ToList<DirectoryObject>();

            result.FilesMissingFromLeft = result.FilesMissingFromLeft.Union<FileObject>(filesMissingLeft);
            result.FilesMissingFromRight = result.FilesMissingFromRight.Union<FileObject>(filesMissingRight);


            //      ##  Recursion   ##

            foreach (var item in left.Directories.Where(x => right.Directories.Any(y => y.Name.ToLower() == x.Name.ToLower())))
            {
                compareDirectoriesHelper(item, right.GetDirectory(item.Name), result);
            }


        }


        private static bool compareFileObject(FileObject file1, FileObject file2)
        {
            throw new NotImplementedException();
        }


        private static IEnumerable<DirectoryObject> getAllSubdirectories(DirectoryObject dir)
        {

            var result = new List<DirectoryObject>();
        
            result.Add(dir);

            for (int i = 0; i < result.Count; i++)
            {
                foreach (var item in result[i].Directories)
                {
                    result.Add(item);
                }
            }

            return result;

        }

        private static IEnumerable<FileObject> getAllFiles(DirectoryObject dir)
        {
            var dirs = getAllSubdirectories(dir);

            return dirs.Select(x => x.Files).Aggregate((l1, l2) => l1.Union(l2));
        }

        #endregion


    }
}
