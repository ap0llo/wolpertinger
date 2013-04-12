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
    public class DirectoryObjectDiff
    {


        public DirectoryObjectDiff() 
            : this(null, null)
        {
        }


        public DirectoryObjectDiff(DirectoryObject left, DirectoryObject right)
        {
            this.Left = left;
            this.Right = right;


            FilesMissingLeft = new List<FileObject>();
            FilesMissingRight = new List<FileObject>();
            ConflictedFiles = new List<FileObjectDiff>();

            DirectoriesMissingLeft = new List<DirectoryObject>();
            DirectoriesMissingRight = new List<DirectoryObject>();
            SubdirectoryDiffs = new List<DirectoryObjectDiff>();
        }

        public DirectoryObject Left { get; private set; }

        public DirectoryObject Right { get; private set; }


        public IEnumerable<FileObject> FilesMissingLeft { get; private set; }

        public IEnumerable<FileObject> FilesMissingRight { get; private set; }

        public IEnumerable<FileObjectDiff> ConflictedFiles { get; private set; }


        public IEnumerable<DirectoryObject> DirectoriesMissingLeft { get; private set; }

        public IEnumerable<DirectoryObject> DirectoriesMissingRight { get; private set; }

        public IEnumerable<DirectoryObjectDiff> SubdirectoryDiffs { get; set; }



        public bool IsEmpty()
        {
            return !(FilesMissingLeft.Any() || FilesMissingRight.Any() || ConflictedFiles.Any() ||
                    DirectoriesMissingLeft.Any() || DirectoriesMissingRight.Any() || SubdirectoryDiffs.Any());
        }


        private void getDifferences()
        {
 
            //compare files

            //files that only exist in left directory
            var filesLeftOnly = Left.Files.Where(x => !Right.Files.Any(y => y.Name.ToLower() == x.Name.ToLower()));

            //files that only exist in right directory
            var filesRightOnly = Right.Files.Where(x => !Left.Files.Any(y => y.Name.ToLower() == x.Name.ToLower()));

            FilesMissingLeft = filesRightOnly;
            FilesMissingRight = filesLeftOnly;
            
            
            //files that exist in both directories
            var filesLeftAndRight = Left.Files.Where(x => Right.Files.Any(y => y.Name.ToLower() == x.Name.ToLower()))
                             .Union(Right.Files.Where(x => Left.Files.Any(y => y.Name.ToLower() == x.Name.ToLower())))
                             .Select(x => x.Name);

            List<FileObjectDiff> fileDiffs = new List<FileObjectDiff>();
            foreach (var name in filesLeftAndRight)
            {
                var left = Left.GetFile(name);
                var right = Right.GetFile(name);

                var diff = new FileObjectDiff(left, right);
                
                if (!diff.IsEmpty())
                {
                    fileDiffs.Add(diff);
                }
            }

            ConflictedFiles = fileDiffs;


            //compare directories
            //directories that only exist in the left directory
            var dirsLeftOnly = Left.Directories.Where(l => Right.Directories.Any(r => r.Name.ToLower() == l.Name.ToLower()));

            //directories that only exist in the right directory
            var dirsRightOnly = Right.Directories.Where(r => Left.Directories.Any(l => l.Name.ToLower() == r.Name.ToLower()));


            DirectoriesMissingLeft = dirsRightOnly;
            DirectoriesMissingRight = dirsLeftOnly;

            //directories that exist in both left and right directory
            var dirsLeftAndRight = Left.Directories.Where(l => Right.Directories.Any(r=> r.Name.ToLower() == l.Name.ToLower()))
                            .Union(Right.Directories.Where(r => Left.Directories.Any(l => l.Name.ToLower() == r.Name.ToLower())))
                            .Select(x => x.Name);

            List<DirectoryObjectDiff> diffs = new List<DirectoryObjectDiff>();

            foreach (var name in dirsLeftAndRight)
            {
                var left = Left.GetDirectory(name);
                var right = Right.GetDirectory(name);

                var diff = new DirectoryObjectDiff(left, right);
                diff.getDifferences();

                if (!diff.IsEmpty())
                {
                    diffs.Add(diff);
                }
            }

            SubdirectoryDiffs = diffs;


        }

    }
}
