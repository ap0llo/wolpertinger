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
using Nerdcave.Common;
using Slf;
using Wolpertinger.Core;

namespace Wolpertinger.Manager.CLI
{
    /// <summary>
    /// A client implementation of the FileShare component.
    /// See Wolpertinger Documentation for details on the component.
    /// </summary>
    [Component(ComponentNamesExtended.FileShare, ComponentType.Client)]
    public class FileShareClientComponent : ClientComponent
    {

        protected static ILogger logger = LoggerService.GetLogger("FileShare");


        #region Events

        public event EventHandler<ObjectEventArgs<DirectoryObject>> GetDirectoryInfoCompleted;
        public event EventHandler<ObjectEventArgs<FileObject>> GetFileInfoCompleted;
        public event EventHandler<ObjectEventArgs<List<MountInfo>>> GetMountsCompleted;
        public event EventHandler<ObjectEventArgs<bool>> GetPermissionCompleted;
        public event EventHandler<ObjectEventArgs<List<Permission>>> GetAddedPermissionsCompleted;
        public event EventHandler<ObjectEventArgs<string>> GetRootDirectoryPathCompleted;        

        #endregion Events


        /// <summary>
        /// Asynchronously calls the "AddSharedDirectory" RemoteMethod on the target client
        /// </summary>
        public void AddSharedDirectoryAsync(string internalPath, string virtualPath)
        {
            callRemoteMethodAsync(FileShareMethods.AddSharedDirectory, false, internalPath, virtualPath);
        }

        /// <summary>
        /// Asynchronously calls the "RemoveSharedDirectory" RemoteMethod on the target client
        /// </summary>
        public void RemoveSharedDirectoryAsync(string virtualPath)
        {
            callRemoteMethodAsync(FileShareMethods.RemoveSharedDirectory, false, virtualPath);
        }

        /// <summary>
        /// Asynchronously calls the "GetDirectoryInfo" RemoteMethod on the target client and 
        /// raises the GetDirectoryInfoCompleted event when the response is receive
        /// </summary>
        public void GetDirectoryInfoAsync(string virtualPath, int depth)
        {
            callRemoteMethodAsync(FileShareMethods.GetDirectoryInfo, true, virtualPath, depth);
        }

        /// <summary>
        /// Synchronously calls the "GetDirectoryInfo" RemoteMethod on the target client and
        /// returns the response value when it is received.
        /// </summary>
        public DirectoryObject GetDirectoryInfo(string virtualPath, int depth)
        {
            return (DirectoryObject)callRemoteMethod(FileShareMethods.GetDirectoryInfo, virtualPath, depth);
        }

        /// <summary>
        /// Asynchronously calls the "GetFileInfo" remoteMethod on the target client
        /// and raises the "GetFileInfoCompleted" event when a response is reqceived.
        /// </summary>
        public void GetFileInfoAsync(string virtualPath)
        {
            callRemoteMethodAsync(FileShareMethods.GetFileInfo, true, virtualPath);
        }

        /// <summary>
        /// Synchronously call the "GetFileInfo" RemoteMethod on the target client and
        /// returns the response value once it is received. (Blocking)
        /// </summary>
        public FileObject GetFileInfo(string virtualPath)
        {
            return (FileObject)callRemoteMethod(FileShareMethods.GetFileInfo,  virtualPath);
        }

        /// <summary>
        /// Asynchronously calls the "GetMounts" RemoteMethod on the target client and
        /// raises the GetMountsCompleted event when a response is received.
        /// </summary>
        public void GetMountsAsync()
        {
            callRemoteMethodAsync(FileShareMethods.GetMounts, true);
        }

        /// <summary>
        /// Synchronously calls the "GetMounts" RemoteMethod on the target client and
        /// returns the response value once it is received. (Blocking)
        /// </summary>
        public List<MountInfo> GetMounts()
        {
            return ((List<object>)callRemoteMethod(FileShareMethods.GetMounts))
                    .Cast<MountInfo>()
                    .ToList<MountInfo>();
        }

        /// <summary>
        /// Asynchronously calls the "AddPermission" RemoteMethod on the target client
        /// </summary>
        public void AddPermissionAsync(Permission p)
        {
            callRemoteMethodAsync(FileShareMethods.AddPermission, false, p);
        }

        /// <summary>
        /// Asynchronously calls the "GetPermission" RemoteMethod on the target client and 
        /// raises the GetPermissionCompleted event when a response is received.
        /// </summary>
        public void GetPermissionAsync(string path)
        {
            callRemoteMethodAsync(FileShareMethods.GetPermission, true, path);
        }

        /// <summary>
        /// Synchronously calls the "GetPermission" RemoteMethod on the target client and
        /// returns the response value once it is received. (Blocking)
        /// </summary>
        public bool GetPermission(string path)
        {
            return (bool)callRemoteMethod(FileShareMethods.GetPermission, path);
        }
        
        /// <summary>
        /// Asynchronously calls the "GetAddedPermission" RemoteMethod on the target client and
        /// raise3s the GetAddedPermissionsCompleted event when a response is received
        /// </summary>
        public void GetAddedPermissionsAsync()
        {
            callRemoteMethodAsync(FileShareMethods.GetAddedPermissions, true);
        }

        /// <summary>
        /// Synchronously calls the "GetAddedPermissions" RemoteMethod on the target client and 
        /// returns the response value once it is received. (Blocking)
        /// </summary>
        public List<Permission> GetAddedPermissions()
        {
            return ((List<object>)callRemoteMethod(FileShareMethods.GetAddedPermissions))
                    .Cast<Permission>()
                    .ToList<Permission>();
        }

        /// <summary>
        /// Asynchronously calls the "RemovePermission" RemoteMethod on the target client
        /// </summary>
        public void RemovePermissionAsync(string path)
        {
            callRemoteMethodAsync(FileShareMethods.RemovePermission, false, path);
        }

        /// <summary>
        /// Asynchronously calls the "SetRootDirectoryPath" RemoteMethod on the target client
        /// </summary>
        public void SetRootDirectoryPathAsync(string internalPath)
        {
            callRemoteMethodAsync(FileShareMethods.SetRootDirectoryPath, false, internalPath);
        }

        /// <summary>
        /// Synchronously calls the "GetRootDirectoryPath" RemoteMethod on the target client and
        /// returns the response value once it is received. (Blocking)
        /// </summary>
        public string GetRootDirectoryPath()
        {
            return (string)callRemoteMethod(FileShareMethods.GetRootDirectoryPath);
        }
        
        /// <summary>
        /// Asynchronously calls the "GetRootDirectoryPath" RemoteMethod on the target client and
        /// raises the GetRootDirectoryPathCompleted event once the response is received.
        /// </summary>
        public void GetRootDirectoryPathAsync()
        {
            callRemoteMethodAsync(FileShareMethods.GetRootDirectoryPath, true);
        }



        [ResponseHandler(FileShareMethods.GetDirectoryInfo)]
        protected virtual void responseHandlerGetDirectoryInfo(DirectoryObject dir)
        {
            if (dir != null)
            {
                onGetDirectoryInfoCompleted(dir);
            }
        }

        [ResponseHandler(FileShareMethods.GetFileInfo)]
        protected virtual void responseHandlerGetFileInfo(FileObject file)
        {
            if (file != null)
            {
                onGetFileInfoCompleted(file);
            }
        }

        [ResponseHandler(FileShareMethods.GetMounts)]
        protected virtual void responseHandlerGetMounts(List<object> mounts)
        {
            if (mounts != null)
                onGetMountsCompleted(new List<MountInfo>(mounts.Cast<MountInfo>()));
        }

        [ResponseHandler(FileShareMethods.GetPermission)]
        protected virtual void responseHandlerGetPermission(bool permitted)
        {
            onGetPermissionCompleted(permitted);         
        }

        [ResponseHandler(FileShareMethods.GetAddedPermissions)]
        protected virtual void responseHandlerGetAddedPermissions(List<object> permissions)
        {
            if (permissions == null)
                logger.Warn("FileShare.GetAddedPermissions(): Received repsonse was null");
            else
                onGetAddedPermissionsCompleted(new List<Permission>(permissions.Cast<Permission>()));
        }

        [ResponseHandler(FileShareMethods.GetRootDirectoryPath)]
        protected virtual void responseHandlerGetRootDirectoryPath(string path)
        {
            onGetRootDirectoryPathCompleted(path);
        }

        




        #region Event Raisers


        protected virtual void onGetDirectoryInfoCompleted(DirectoryObject dir)
        {
            if (this.GetDirectoryInfoCompleted != null)
            {
                this.GetDirectoryInfoCompleted(this, dir);
            }
        }

        protected virtual void onGetFileInfoCompleted(FileObject file)
        {
            if (this.GetFileInfoCompleted != null)
            {
                this.GetFileInfoCompleted(this, file);
            }
        }

        protected virtual void onGetMountsCompleted(List<MountInfo> mounts)
        {
            if (this.GetMountsCompleted != null)
            {
                this.GetMountsCompleted(this, mounts);
            }
        }
        
        protected virtual void onGetPermissionCompleted(bool permitted)
        {
            if (this.GetPermissionCompleted != null)
                this.GetPermissionCompleted(this, permitted);
            
        }

        protected virtual void onGetAddedPermissionsCompleted(List<Permission> permissions)
        {
            if (this.GetAddedPermissionsCompleted != null)
                this.GetAddedPermissionsCompleted(this, permissions);
        }

        protected virtual void onGetRootDirectoryPathCompleted(string path)
        {
            if (this.GetRootDirectoryPathCompleted != null)
                this.GetRootDirectoryPathCompleted(this, path);
        }


        #endregion

    }

}
