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
using System.Threading.Tasks;
using Nerdcave.Common;
using Slf;
using Wolpertinger.Core;
using Wolpertinger.FileShareCommon;

namespace Wolpertinger.Manager.CLI
{
    /// <summary>
    /// A client implementation for the FileShare component.
    /// See Wolpertinger Documentation for details on the component.
    /// </summary>
    public class FileShareClientComponent : IComponent
    {

        protected static ILogger logger = LoggerService.GetLogger("FileShare");


        /// <summary>
        /// The IClientConnection used to communicate with the target-client
        /// </summary>
        public IClientConnection ClientConnection { get; set; }



        /// <summary>
        /// Asynchronously calls the "AddSharedDirectory" RemoteMethod on the target client
        /// </summary>
        public Task AddSharedDirectoryAsync(string internalPath, string virtualPath)
        {
            return Task.Factory.StartNew(delegate
            {
                ClientConnection.CallRemoteAction(ComponentNamesExtended.FileShare,
                    FileShareMethods.AddSharedDirectory, internalPath, virtualPath);
            });
        }

        /// <summary>
        /// Asynchronously calls the "RemoveSharedDirectory" RemoteMethod on the target client
        /// </summary>
        public Task RemoveSharedDirectoryAsync(string virtualPath)
        {
            return Task.Factory.StartNew(delegate
            {
                ClientConnection.CallRemoteAction(ComponentNamesExtended.FileShare,
                    FileShareMethods.RemoveSharedDirectory, virtualPath);
            });
        }

        /// <summary>
        /// Asynchronously calls the "GetDirectoryInfo" RemoteMethod on the target client and 
        /// raises the GetDirectoryInfoCompleted event when the response is receive
        /// </summary>
        public Task<DirectoryObject> GetDirectoryInfoAsync(string virtualPath, int depth)
        {
            return Task.Factory.StartNew<DirectoryObject>(delegate
            {
                return (DirectoryObject)ClientConnection.CallRemoteFunction(
                    ComponentNamesExtended.FileShare,
                    FileShareMethods.GetDirectoryInfo,
                    virtualPath,
                    depth);
            });
        }

        
        /// <summary>
        /// Asynchronously calls the "GetFileInfo" remoteMethod on the target client
        /// and raises the "GetFileInfoCompleted" event when a response is reqceived.
        /// </summary>
        public Task<FileObject> GetFileInfoAsync(string virtualPath)
        {
            return Task.Factory.StartNew<FileObject>(delegate
            {
                return (FileObject)ClientConnection.CallRemoteFunction(
                        ComponentNamesExtended.FileShare,
                        FileShareMethods.GetFileInfo,
                        virtualPath
                    );
            });
        }


        /// <summary>
        /// Asynchronously calls the "GetMounts" RemoteMethod on the target client and
        /// raises the GetMountsCompleted event when a response is received.
        /// </summary>
        public Task<List<MountInfo>> GetMountsAsync()
        {
            return Task.Factory.StartNew<List<MountInfo>>(delegate
            {
                return ((List<object>)ClientConnection.CallRemoteFunction(
                            ComponentNamesExtended.FileShare,FileShareMethods.GetMounts))
                        .Cast<MountInfo>()
                        .ToList<MountInfo>();
            });            
        }
        

        /// <summary>
        /// Asynchronously calls the "AddPermission" RemoteMethod on the target client
        /// </summary>
        public Task AddPermissionAsync(Permission p)
        {
            return Task.Factory.StartNew(delegate
            {
                ClientConnection.CallRemoteAction(
                    ComponentNamesExtended.FileShare,
                    FileShareMethods.AddPermission,
                    p);
            });            
        }

        /// <summary>
        /// Asynchronously calls the "GetPermission" RemoteMethod on the target client and 
        /// raises the GetPermissionCompleted event when a response is received.
        /// </summary>
        public Task<bool> GetPermissionAsync(string path)
        {
            return Task.Factory.StartNew<bool>(delegate
            {
                return (bool)ClientConnection.CallRemoteFunction(
                                ComponentNamesExtended.FileShare,
                                FileShareMethods.GetPermission,
                                path);
            });            
        }

        /// <summary>
        /// Asynchronously calls the "GetAddedPermission" RemoteMethod on the target client and
        /// raise3s the GetAddedPermissionsCompleted event when a response is received
        /// </summary>
        public Task<List<Permission>> GetAddedPermissionsAsync()
        {
            return Task.Factory.StartNew<List<Permission>>(delegate
            {
                return ((List<object>)ClientConnection.CallRemoteFunction(ComponentNamesExtended.FileShare, FileShareMethods.GetAddedPermissions))
                        .Cast<Permission>()
                        .ToList<Permission>();
            });
        }

        /// <summary>
        /// Asynchronously calls the "RemovePermission" RemoteMethod on the target client
        /// </summary>
        public Task RemovePermissionAsync(string path)
        {
            return Task.Factory.StartNew(delegate
            {
                ClientConnection.CallRemoteAction(
                    ComponentNamesExtended.FileShare,
                    FileShareMethods.RemovePermission,
                    path);
            });
        }

        /// <summary>
        /// Asynchronously calls the "SetRootDirectoryPath" RemoteMethod on the target client
        /// </summary>
        public Task SetRootDirectoryPathAsync(string internalPath)
        {
            return Task.Factory.StartNew(delegate
            {
                ClientConnection.CallRemoteAction(
                    ComponentNamesExtended.FileShare,
                    FileShareMethods.SetRootDirectoryPath,
                    internalPath);
            });
        }

        /// <summary>
        /// Asynchronously calls the "GetRootDirectoryPath" RemoteMethod on the target client and
        /// raises the GetRootDirectoryPathCompleted event once the response is received.
        /// </summary>
        public Task<string> GetRootDirectoryPathAsync()
        {
            return Task.Factory.StartNew<string>(delegate
            {
                return (string)ClientConnection.CallRemoteFunction(
                    ComponentNamesExtended.FileShare,
                    FileShareMethods.GetRootDirectoryPath);
            });
        }


        public Task<IEnumerable<SnapshotInfo>> GetSnapshotsAsync()
        {
            return Task.Factory.StartNew<IEnumerable<SnapshotInfo>>(delegate
            {
                return (ClientConnection.CallRemoteFunction(
                    ComponentNamesExtended.FileShare,
                    FileShareMethods.GetSnapshots) as IEnumerable<object>).Cast<SnapshotInfo>();
            });
        }


        public Task<Guid> CreateSnapshotAsync()
        {
            return Task.Factory.StartNew<Guid>(delegate
            {
                return Guid.Parse((string)ClientConnection.CallRemoteFunction(
                    ComponentNamesExtended.FileShare,
                    FileShareMethods.CreateSnapshot));
            });
        }

    }

}
