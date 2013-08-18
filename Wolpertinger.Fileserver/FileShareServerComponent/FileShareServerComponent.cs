/*

Licensed under the new BSD-License
 
Copyright (c) 2011-2013, Andreas Grünwald 
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

	Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
	Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer 
	in the documentation and/or other materials provided with the distribution.
	Neither the name of the Wolpertinger project nor the names of its contributors may be used to endorse or promote products 
	derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS 
BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, 
STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/
using BinaryRage;
using Nerdcave.Common;
using Nerdcave.Common.Extensions;
using Nerdcave.Common.IO;
using Nerdcave.Common.Xml;
using Slf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Wolpertinger.Core;
using Wolpertinger.FileShareCommon;

namespace Wolpertinger.Fileserver
{
	/// <summary>
	/// A server implementation of the FileShare component.
	/// See Wolpertinger API Documentation for details on the component.
	/// </summary>
	[Component(ComponentNamesExtended.FileShare)]
	public partial class FileShareServerComponent : IComponent
	{

		static ILogger logger = LoggerService.GetLogger("FileShareServerComponent");

		protected static bool mountErrorOccurred = false;

		protected static KeyValueStore storageFile = new KeyValueStore(Path.Combine(DefaultConnectionManager.SettingsFolder, "FileShare.xml"));

		protected static readonly string SnapshotDbFolder;
		
		protected const string SNAPSHOTS_INDEX_DBKEY = "Snapshots";

		protected static string rootPath;
		protected static FileSystemWatcher fsWatcher;
		protected static List<MountInfo> mounts = new List<MountInfo>();

		protected static Dictionary<string, Permission> permissions = new Dictionary<string, Permission>();



		public IClientConnection ClientConnection { get; set; }




		static FileShareServerComponent()
		{
			SnapshotDbFolder = Path.Combine(Program.DatabaseFolder, "FileShareSnapshots");
		}

		internal static void Init()
		{

			Task t = new Task(delegate
			{

				logger.Info("Initializing FileShare");

				rootPath = storageFile.GetItem<string>("RootPath");

				//load mounts
				List<object> myMounts = storageFile.GetItem<List<object>>("mounts");
				myMounts = (myMounts == null) ? new List<object>() : myMounts;
				mounts = new List<MountInfo>();
				foreach (Object item in myMounts)
				{
					mounts.Add((MountInfo)item);
				}


				if (rootPath.IsNullOrEmpty() || !Directory.Exists(rootPath))
				{
					mountErrorOccurred = true;
					logger.Error("Root directory not found");
				}

				//load permissions
				List<object> _permissions = storageFile.GetItem<List<object>>("permissions");
				_permissions = (_permissions == null) ? new List<object>() : _permissions;

				foreach (object item in _permissions)
				{
					if (!permissions.ContainsKey((item as Permission).Path.ToLower()))
						permissions.Add(((Permission)item).Path.ToLower(), (Permission)item);
				}


				//scan local directories to make sure hashes are available fast when requested
				foreach (var item in mounts)
				{
					scanDirectory(item.LocalPath);
				}

				logger.Info("FileShare initialization completed");
			});
			t.Start();
		}




		/// <summary>
		/// Server implementation of the GetDirectoryInfo RemoteMethod
		/// </summary>
		/// <param name="virtualPath">The virtual path of the directory to get info on</param>
		/// <param name="depth">Specifies how many hierarchy-levels of the file-system to include in the response</param>
		[TrustLevel(3)]
		[MethodCallHandler(FileShareMethods.GetDirectoryInfo)]
		public CallResult GetDirectoryInfo(string virtualPath, int depth, Guid snapshotId)
		{
			//check for previous mount-errors
			if (mountErrorOccurred)
			{
				logger.Error("Mount error found");
				return new ErrorResult(RemoteErrorCode.MountError);
			}



			if (snapshotId == Guid.Empty)
			{
				logger.Info("Getting Directory-Info for {0}, depth {1}", virtualPath, depth);

				try
				{
					if (!(checkPermission(virtualPath, permissions) || anyChildPermitted(virtualPath)))
					{
						logger.Error("Access violation found");
						return new ErrorResult(RemoteErrorCode.NotAuthorizedError);
					}


					var dir = loadVirtualPath(virtualPath, depth);

					//remove directories from the result the caller is not permitted to see
					removeUnpermittedDirs(dir, permissions);

					if (virtualPath == "/")
						dir.Name = "";


					logger.Info("Returning directory information");

					return new ResponseResult(dir);

				}
				catch (DirectoryNotFoundException)
				{
					logger.Error("Directory or Sub-Directory not found");
					return new ErrorResult(RemoteErrorCode.ItemNotFoundError);
				}
			}
			else 
			{
				try
				{
					var snapshot = BinaryRage.DB<Snapshot>.Get(snapshotId.ToString(), SnapshotDbFolder);
					
					var dir = snapshot.FilesystemState.Clone(depth);

					dir.Path = "/";

					if (!(checkPermission(virtualPath, snapshot.Permissions.ToDictionary(x => x.Path.ToLower())) || anyChildPermitted(virtualPath)))
					{
						logger.Error("Access violation found");
						return new ErrorResult(RemoteErrorCode.NotAuthorizedError);
					}

					removeUnpermittedDirs(dir, snapshot.Permissions.ToDictionary(x => x.Path.ToLower()));

					if (virtualPath == "/")
					{
						dir.Name = "";
					}

					return new ResponseResult(dir);
				}
				catch (DirectoryNotFoundException)
				{
					return new ErrorResult(RemoteErrorCode.ItemNotFoundError);
				}
			}
		}

		/// <summary>
		/// Server implementation of the GetFileInfo RemoteMethod
		/// </summary>
		/// <param name="virtualPath">The virtual path of the file to get information on</param>
		[TrustLevel(3)]
		[MethodCallHandler(FileShareMethods.GetFileInfo)]
		public CallResult GetFileInfo(string virtualPath)
		{

			logger.Info("Getting FileInfo for {0}", virtualPath);

			if (mountErrorOccurred)
			{
				logger.Error("Mount error found");
				return new ErrorResult(RemoteErrorCode.MountError);
			}

			if (mountErrorOccurred)
			{
				logger.Error("Mount error found");
				return new ErrorResult(RemoteErrorCode.MountError);
			}

			try
			{
				if (!checkPermission(virtualPath, permissions))
				{
					logger.Error("Permission denied");
					return new ErrorResult(RemoteErrorCode.NotAuthorizedError);
				}

				var localPath = getLocalPath(virtualPath);
			
				if (localPath.IsNullOrEmpty())
					throw new FileNotFoundException();


				var file = new FileObject();
				file.LocalPath = localPath;
				file.LoadFromDisk();

				return new ResponseResult(file);

			}
			catch (Exception ex)
			{
				if (ex is FileNotFoundException || ex is DirectoryNotFoundException)
				{
					logger.Error("Item not found");
					return new ErrorResult(RemoteErrorCode.ItemNotFoundError);
				}
				else
				{
					throw ex;
				}
			}
		}


		/// <summary>
		/// Server implementation of the SetRootDirectoryPath RemoteMethod
		/// </summary>
		/// <param name="localPath">The path on the local machine to be used as root-directory</param>
		[TrustLevel(4)]
		[MethodCallHandler(FileShareMethods.SetRootDirectoryPath)]
		public CallResult SetRootDirectoryPath(string localPath)
		{
			mountErrorOccurred = false;

			logger.Info("Setting root directory");

			if (localPath == rootPath)
			{
				logger.Info("Root directory already set to new value");
				return new VoidResult();
			}

			if (!Directory.Exists(localPath) || mountErrorOccurred)
			{
				logger.Error("Could not set new root directory");
				return new ErrorResult(RemoteErrorCode.FileserverError);
			}

			rootPath = localPath;
			storageFile.SaveItem("RootPath", localPath);
			Init();

			if (mountErrorOccurred)
				return new ErrorResult(RemoteErrorCode.FileserverError);
			else
				return new VoidResult();
		}

		/// <summary>
		/// Server implementation of the GetRootDirectoyPath RemoteMethod
		/// </summary>
		[TrustLevel(4)]
		[MethodCallHandler(FileShareMethods.GetRootDirectoryPath)]
		public CallResult GetRootDirectoryPath()
		{
			if (rootPath.IsNullOrEmpty())
				return new ResponseResult("");
			else
				return new ResponseResult(rootPath);
		}
		
		




		protected static void scanDirectory(string path, bool newThread = true)
		{

			logger.Info("Scanning directory {0}", path);

			var t = new ThreadStart(delegate
				{
					var hashingService = HashingService.GetHashingService();

					if (!Directory.Exists(path))
						return;

					foreach (var file in Directory.GetFiles(path))
					{
						hashingService.GetHashAsync(file, Priority.Low);
					}

					foreach (var dir in Directory.GetDirectories(path))
					{
						scanDirectory(dir, false);
					}                
				});

			if (newThread)
			{
				new Thread(t).Start();
			}
			else
			{
				t.Invoke();
			}

			

		}

		private DirectoryObject loadVirtualPath(string virtualPath, int depth = -1)
		{
			var localPath = getLocalPath(virtualPath);
			if (localPath.IsNullOrEmpty())
			{
				throw new DirectoryNotFoundException();
			}

			var dir = new DirectoryObject();
			dir.LocalPath = localPath;
			dir.LoadFromDisk(depth);

			//load files and directories from other mounts that match the specified path
			var subMounts = mounts.Where(x => x.MountPoint.ToLower().StartsWith(virtualPath.ToLower()));
			foreach (var item in subMounts)
			{
				var itemMountPoint = item.MountPoint.Remove(0, virtualPath.Length);

				if (itemMountPoint.IsNullOrEmpty())
					continue;

				if (itemMountPoint.StartsWith("/"))
					itemMountPoint = itemMountPoint.RemoveFirstChar();

				if (itemMountPoint.Split('/').Length > depth && depth >= 0)
				{
					continue;
				}

				var subDir = new DirectoryObject();
				subDir.LocalPath = item.LocalPath;


				int scanDepth = (depth < 0) ? -1 : Math.Max(0, (depth - itemMountPoint.Split('/').Length));

				subDir.LoadFromDisk(scanDepth);
                subDir.Name = Path.GetFileName(itemMountPoint);
				var parent = dir.GetDirectory(Path.GetDirectoryName(itemMountPoint));

				if (parent.Directories.Any(x => x.Name.ToLower() == subDir.Name.ToLower()))
					parent.RemoveDirectory(subDir.Name);

				parent.AddDirectory(subDir);
			}

			dir.Path = virtualPath;

			return dir;
		}

		private void removeUnpermittedDirs(DirectoryObject dir, Dictionary<string, Permission> permissions)
		{
			bool childPermitted = false;
			foreach (DirectoryObject item in dir.Directories.ToList<DirectoryObject>())
			{
				bool itemPermitted = checkPermission(item.Path, permissions) || anyChildPermitted(item.Path);

				childPermitted = childPermitted || itemPermitted;

				if (!itemPermitted)
				{
					dir.RemoveDirectory(item.Name);
				}
				else
				{
					removeUnpermittedDirs(item, permissions);
				}
			}

			if (!checkPermission(dir.Path, permissions))
			{
				dir.ClearFiles();
			}
		}

		private bool anyChildPermitted(string path)
		{
			return permissions.Keys.Where(x => x.StartsWith(path.ToLower())).Select(x => permissions[x]).Any(x => x.PermittedClients.Contains(ClientConnection.Target));
		}

		private string getLocalPath(string virtualpath)
		{
			var orderedMounts = mounts.Union<MountInfo>(new List<MountInfo>() { new MountInfo() { LocalPath = rootPath, MountPoint = "/"}})
                .OrderByDescending(x => x.MountPoint);

			var mountsQuery = orderedMounts.Where(x => virtualpath.ToLower().StartsWith(x.MountPoint.ToLower()));

			var matchingMount = mountsQuery.Any() ? mountsQuery.First() : null;

			if (matchingMount == null)
			{
				return null;
			}
			else
			{
				virtualpath = virtualpath.Remove(0, matchingMount.MountPoint.Length);

                if (virtualpath.StartsWith("/") || virtualpath.StartsWith("\\"))
                {
					virtualpath = virtualpath.RemoveFirstChar();
                }

				return Path.Combine(matchingMount.LocalPath, virtualpath);
			}
		}

		protected void savePermissions()
		{
			storageFile.SaveItem("permissions", permissions.Values);
		}

		protected Permission getPermissionForPath(string path, Dictionary<string, Permission> permissions)
		{
			path = path.ToLower();
			if (permissions.ContainsKey(path))
			{
				return permissions[path];
			}
			else
			{
				foreach (string item in permissions.Keys.OrderByDescending(x => x.Length))
				{
					if (path.StartsWith(item))
					{
						return permissions[item];
					}
				}
			}
			return null;
		}

		protected bool checkPermission(string path, Dictionary<string, Permission> permissions)
		{
			if (ClientConnection.TrustLevel == 4)
			{
				return true;
			}
			else
			{
				Permission p = getPermissionForPath(path, permissions);
				if (p != null && p.PermittedClients.Contains(removeResource(this.ClientConnection.Target)))
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}


		private string removeResource(string jid)
		{
			if (!jid.Contains('/'))
			{
				return jid;
			}
			else
			{
				return jid.Remove(jid.IndexOf('/'));
			}

		}



	}


 
}
