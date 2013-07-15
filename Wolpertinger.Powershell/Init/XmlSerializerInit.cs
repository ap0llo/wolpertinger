using Nerdcave.Common.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wolpertinger.Core;
using Wolpertinger.FileShareCommon;

namespace Wolpertinger.Powershell.Init
{
    class XmlSerializerInit
    {
        public static void Initialize()
        {
            XmlSerializer.RegisterType(typeof(ClientInfo), "clientInfo");
            XmlSerializer.RegisterType(typeof(DirectoryObject), "directoryObject");
            XmlSerializer.RegisterType(typeof(FileObject), "fileObject");
            XmlSerializer.RegisterType(typeof(Permission), "permission");
            XmlSerializer.RegisterType(typeof(MountInfo), "mountInfo");
            XmlSerializer.RegisterType(typeof(SnapshotInfo), "snapshotInfo");
            XmlSerializer.RegisterType(typeof(DirectoryObjectDiff), "directoryObjectDiff");
            XmlSerializer.RegisterType(typeof(RemoteMethodCall), "remoteMethodCall");
            XmlSerializer.RegisterType(typeof(RemoteMethodResponse), "remoteMethodResponse");
            XmlSerializer.RegisterType(typeof(RemoteError), "remoteError");
        }
    }
}
