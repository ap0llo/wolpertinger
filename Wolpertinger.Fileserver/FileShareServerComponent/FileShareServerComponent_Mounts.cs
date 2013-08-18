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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Wolpertinger.Core;
using Wolpertinger.FileShareCommon;

namespace Wolpertinger.Fileserver
{
	public partial class FileShareServerComponent
	{

		/// <summary>
		/// Server implementation of the AddSharedDirectory RemoteMethod
		/// </summary>
		/// <param name="localPath">The local path of the directory that is to be shared</param>
		/// <param name="virtualPath">The virtual path to share the directory at</param>        
		[TrustLevel(4)]
		[MethodCallHandler(FileShareMethods.AddSharedDirectory)]
		public CallResult NewMount(string localPath, string virtualPath)
		{
			logger.Info("Adding Shared directory {0} at {1}", localPath, virtualPath);

			//check for previous mount errors
			if (mountErrorOccurred)
			{
				logger.Error("Mount error found");
				return new ErrorResult(RemoteErrorCode.MountError);
			}

			//check if directory exits
			if (!Directory.Exists(localPath))
			{
				return new ErrorResult(RemoteErrorCode.ItemNotFoundError);
			}


			MountInfo info = new MountInfo();
			info.MountPoint = virtualPath;
			info.LocalPath = localPath;

			mounts.Add(info);

			storageFile.SaveItem("mounts", mounts);

			scanDirectory(localPath);

			return new VoidResult();
		}


		/// <summary>
		/// Server implementation of the RemoveSharedDirectory RemoteMethod
		/// </summary>
		/// <param name="virtualPath">The virtual path of the directory that's to be un-shared.</param>
		[TrustLevel(4)]
		[MethodCallHandler(FileShareMethods.RemoveSharedDirectory)]
		public CallResult RemoveMount(string virtualPath)
		{

			logger.Info("Removing Shared directory {0}", virtualPath);

			//check for previous mount errors
			if (mountErrorOccurred)
			{
				logger.Error("Mount error found");
				return new ErrorResult(RemoteErrorCode.MountError);
			}

			if (!mounts.Any(x => x.MountPoint.ToLower() == virtualPath.ToLower()))
			{
				logger.Error("Could not remove folder {0}. Item is no Mount", virtualPath);
				return new ErrorResult(RemoteErrorCode.ItemNotFoundError);
			}
			else
			{
				MountInfo mount = mounts.First(x => x.MountPoint.ToLower() == virtualPath.ToLower());

				mounts.Remove(mount);
				storageFile.SaveItem("mounts", mounts);

				return new VoidResult();

			}

		}



		/// <summary>
		/// Server implementation of the GetMounts RemoteMethod
		/// </summary>
		[TrustLevel(4)]
		[MethodCallHandler(FileShareMethods.GetMounts)]
		public CallResult GetMounts()
		{
			logger.Info("Getting Mounts");
			return new ResponseResult(mounts);
		}


	}
}
