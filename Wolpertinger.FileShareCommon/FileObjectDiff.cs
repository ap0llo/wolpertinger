using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.FileShareCommon
{
    public class FileObjectDiff
    {


        private List<FileObjectProperty> _differences = new List<FileObjectProperty>();


        public FileObject Left { get; private set; }

        public FileObject Right { get; private set; }

        public IEnumerable<FileObjectProperty> Differences { get { return _differences; } }


        public FileObjectDiff()
        {

        }


        public FileObjectDiff(FileObject left, FileObject right)
        {
            this.Left = left;
            this.Right = right;

            getDifferences();
        }



        public bool IsEmpty()
        {
            return !Differences.Any();
        }


        private void getDifferences()
        {
            if (Left.Created != Right.Created)
            {
                _differences.Add(FileObjectProperty.Created);
            }

            if (Left.Hash != Right.Hash)
            {
                _differences.Add(FileObjectProperty.Hash);
            }

            if (Left.LastEdited != Right.LastEdited)
            {
                _differences.Add(FileObjectProperty.LastEdited);
            }

            if (Left.Name != Right.Name)
            {
                _differences.Add(FileObjectProperty.Name);
            }

            if (Left.Path != Right.Path)
            {
                _differences.Add(FileObjectProperty.Path);
            }
        }

    }



    public enum FileObjectProperty
    {
        Created ,
        Hash,
        LastEdited,
        Name, 
        Path
    }

}
