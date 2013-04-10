using Nerdcave.Common.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using Wolpertinger.Core;
using System.Globalization;

namespace Wolpertinger.FileShareCommon
{
    [Serializable]
    public class Snapshot
    {

        public Snapshot()
        {
            this.Info = new SnapshotInfo();
        }


        public SnapshotInfo Info { get; set; }

        public DirectoryObject FilesystemState { get; set; }

        public IEnumerable<Permission> Permissions { get; set; }

    }


    
}
