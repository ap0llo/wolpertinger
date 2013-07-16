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

	public partial class FileShareServerComponent
	{

		[TrustLevel(3)]
		[MethodCallHandler(FileShareMethods.GetSnapshots)]
		public CallResult GetSnapshots()
		{
			//Get the index of snapshot form the database
			IEnumerable<Guid> snapshotIndex;
			try
			{
				snapshotIndex = DB<List<Guid>>.Get(SNAPSHOTS_INDEX_DBKEY, SnapshotDbFolder);
			}
			catch (DirectoryNotFoundException)
			{
				//index not found => use an empty one instead
				snapshotIndex = new List<Guid>();
			}

			//Get all snapshots in the list from the database
			IEnumerable<SnapshotInfo> snapshots;
			if (snapshotIndex.Any())
			{
				try
				{
					snapshots = snapshotIndex.Select(x => x.ToString())
											 .Select(x => DB<Snapshot>.Get(x, SnapshotDbFolder))
											 .Select(x => x.Info);
				}
				catch (DirectoryNotFoundException)
				{
					logger.Warn("Invalid Snapshot id in database");
					snapshots = new List<SnapshotInfo>();                    
				}
			}
			else
			{
				snapshots = new List<SnapshotInfo>();
			}

			return new ResponseResult(snapshots);
		}

		[TrustLevel(3)]
		[MethodCallHandler(FileShareMethods.CreateSnapshot)]
		public CallResult CreateSnapshot()
		{
			var snapshot = new Snapshot();
			snapshot.Permissions = permissions.Values;

			if (mountErrorOccurred)
			{
				return new ErrorResult(RemoteErrorCode.MountError);
			}

			try
			{
				snapshot.FilesystemState = loadVirtualPath("/");
			}
			catch (DirectoryNotFoundException)
			{
				return new ErrorResult(RemoteErrorCode.FileserverError);
			}

			
			snapshot.Info.Time = DateTime.Now;

			//Get list of all snapshots from the database
			List<Guid> snapshotIndex;
			try 
			{	        
				snapshotIndex = BinaryRage.DB<List<Guid>>.Get(SNAPSHOTS_INDEX_DBKEY, SnapshotDbFolder);
			}
			catch (DirectoryNotFoundException)
			{
				//Snapshot-Index was not found in database => create index
				snapshotIndex = new List<Guid>();
			}

			//save new snapshot in the database
			var key = snapshot.Info.Id;
			BinaryRage.DB<Snapshot>.Insert(key.ToString(), snapshot, SnapshotDbFolder);

			//add the key to the snapshot index and save the updated index
			snapshotIndex.Add(key);
			BinaryRage.DB<List<Guid>>.Insert(SNAPSHOTS_INDEX_DBKEY, snapshotIndex, SnapshotDbFolder);
			

			return new ResponseResult(snapshot.Info.Id.ToString());
		}

		[TrustLevel(4)]
		[MethodCallHandler(FileShareMethods.DeleteSnapshot)]
		public CallResult DeleteSnapshot(Guid id)
		{
			//Get list of all snapshots from the database
			List<Guid> snapshotIndex;
			try
			{
				snapshotIndex = BinaryRage.DB<List<Guid>>.Get(SNAPSHOTS_INDEX_DBKEY, SnapshotDbFolder);
			}
			catch (DirectoryNotFoundException)
			{
				//Snapshot-Index was not found in database => create index
				snapshotIndex = new List<Guid>();
			}


			if (snapshotIndex.Contains(id))
			{
				snapshotIndex.Remove(id);
				BinaryRage.DB<List<Guid>>.Insert(SNAPSHOTS_INDEX_DBKEY, snapshotIndex, SnapshotDbFolder);
				
				BinaryRage.DB<Snapshot>.Remove(id.ToString(), SnapshotDbFolder);
				
				return new VoidResult();
			}
			else
			{
				return new ErrorResult(RemoteErrorCode.ItemNotFoundError);
			}

		}

		[TrustLevel(3)]
		[MethodCallHandler(FileShareMethods.CompareSnapshots)]
		public CallResult CompareSnapshots(Guid leftId, Guid rightId)
		{
			//Get list of all snapshots from the database
			List<Guid> snapshotIndex;
			try
			{
				snapshotIndex = BinaryRage.DB<List<Guid>>.Get(SNAPSHOTS_INDEX_DBKEY, SnapshotDbFolder);
			}
			catch (DirectoryNotFoundException)
			{
				//Snapshot-Index was not found in database => create index
				snapshotIndex = new List<Guid>();
			}


			if (snapshotIndex.Contains(leftId) && snapshotIndex.Contains(rightId))
			{
				var leftFolder = BinaryRage.DB<Snapshot>.Get(leftId.ToString(), SnapshotDbFolder).FilesystemState;
				var rightFolder = BinaryRage.DB<Snapshot>.Get(rightId.ToString(), SnapshotDbFolder).FilesystemState;

				leftFolder.Path = "/";
				rightFolder.Path = "/";


				var diff = DirectoryObjectDiff.GetDiff(leftFolder, rightFolder);
				return new ResponseResult(diff);
			}
			else
			{
				return new ErrorResult(RemoteErrorCode.ItemNotFoundError);
			}

		}
             


	}


 
}
