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

using Nerdcave.Common;
using Nerdcave.Common.Extensions;
using Raven.Client;
using Raven.Client.Embedded;
using Slf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wolpertinger.Core;
using Wolpertinger.FileShareCommon;

namespace Wolpertinger.Fileserver
{
	/// <summary>
	/// Implementation of IHashingService used by Wolpertinger Fileserver (singleton class)
	/// </summary>
	public class HashingService : IHashingService
	{
		/*
		private ILogger logger;

		private static Task hashingTask;
		private static PriorityBlockingQueue<string> queuedHashes = new PriorityBlockingQueue<string>();

		private static ConcurrentDictionary<string, EventWaitHandle> waitHandles = new ConcurrentDictionary<string, EventWaitHandle>();
		private static ConcurrentDictionary<string, string> hashCache = new ConcurrentDictionary<string, string>();

		private static SHA1CryptoServiceProvider sha1h;

		private static HashingService instance;


		public event EventHandler<GetHashEventArgs> GetHashAsyncCompleted;


		
		static HashingService()
		{
			sha1h = new SHA1CryptoServiceProvider();

			instance = new HashingService();

			hashingTask = new Task(instance.calculateHashes);
			hashingTask.Start();

		}


		public static IHashingService GetHashingService()
		{
			return instance;
		}


		//Private constructor (singleton class)
		private HashingService()
		{
			 logger = LoggerService.GetLogger("HashingService");
		}




		public void GetHashAsync(string filename, Priority priority)
		{

			filename = filename.ToLower();

			if(!File.Exists(filename))
			{
				onGetHashAsyncCompleted(filename, null);
				return;
			}

			using (var session = Program.Database.OpenSession())
			{

#if DEBUG
		 
			   
#endif

				var info = session.Load<HashInfo>(getKeyForFileName(filename));
				FileInfo fInfo = new FileInfo(filename);

				if (info != null)
				{
					if (info.FileName == filename.ToLower() &&
						info.Created == fInfo.CreationTimeUtc &&
						info.LastEdited == fInfo.LastWriteTimeUtc &&
						info.Size == fInfo.Length)
					{
						onGetHashAsyncCompleted(filename, info.Hash);
						return;
					}
				}
			}

			//no cached value found or file has been modified => queue file for hashing
			queuedHashes.Add(filename, priority);

		}


		private string getKeyForFileName(string filename)
		{
			return "filehash/" + filename.ToLower().GetHashSHA1();
		}



		public string GetHash(string filename, Priority priority)
		{
			filename = filename.ToLower();

			EventWaitHandle waitHandle;

			lock (this)
			{
				waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
				waitHandles.AddOrUpdate(filename, waitHandle, (k, v) => v);
				waitHandle = waitHandles[filename];
			}

			GetHashAsync(filename, priority);

			waitHandle.WaitOne();

			return hashCache[filename];

		}


		private void calculateHashes()
		{
			while (true)
			{
				string file = queuedHashes.Take();

				if (File.Exists(file))
				{
					FileInfo fInfo = new FileInfo(file);
					HashInfo info = new HashInfo()
						{
							FileName = file.ToLower(),
							Created = fInfo.CreationTimeUtc,
							LastEdited = fInfo.LastWriteTimeUtc,
							Size = fInfo.Length
						};

					logger.Info("Calculating Hash for {0}", file);
					info.Hash = getHashSHA1(file);

					using (var session = Program.Database.OpenSession())
					{
						session.Store(info, getKeyForFileName(file));
						session.SaveChanges();
					}
					onGetHashAsyncCompleted(file, info.Hash);
				}
				else
				{
					//raise event to release threads that may be waiting for hasing to finish
					onGetHashAsyncCompleted(file, null);
				}
			}
		}

	



		private void onGetHashAsyncCompleted(string filename, string hash)
		{
			if (waitHandles.ContainsKey(filename))
			{
				var waitHandle = waitHandles[filename];
				hashCache.AddOrUpdate(filename, hash, (k, v) => hash);
				waitHandle.Set();
			}

			if (GetHashAsyncCompleted != null)
				GetHashAsyncCompleted(null, new GetHashEventArgs() { Path = filename, Hash = hash });
		}

		*/

		static HashingService instance;

		ILogger logger = LoggerService.GetLogger("HashingService");
		SHA1CryptoServiceProvider sha1Provider = new SHA1CryptoServiceProvider();
		PriorityBlockingQueue<string> queue = new PriorityBlockingQueue<string>();


		static HashingService()
		{
			instance = new HashingService();
		}

		public static IHashingService GetHashingService()
		{
			return instance;
		}

		private HashingService()
		{
			var threadStart = new ThreadStart(delegate { processQueuedFiles(); });
			new Thread(threadStart).Start();
		}


		private string getKey(string fileName)
		{
			return "filehash/" + fileName.Trim().ToLower().GetHashSHA1();
		}

		/// <summary>
		/// Returns a SHA1-Hash of the file's content.
		/// </summary>
		/// <param name="fileName">The path of the file that's to be hashed.</param>
		/// <returns></returns>
		private string calculateHash(string fileName)
		{
			//Console.WriteLine("Generating Hash for " + fileName);

			string sHash = "";
			using (StreamReader sr = new StreamReader(fileName))
			{
				sHash = sha1Provider.ComputeHash(sr.BaseStream).ToHexString();
			}

			//Console.WriteLine("Done.");

			return sHash;
		}

		/// <summary>
		/// Gets the hash for the specified file from the cache
		/// </summary>
		private string queryHash(string fileName)
		{
			//normalize filename
			fileName = fileName.Replace("/", "\\");

			var key = getKey(fileName);

			using (var session = Program.Database.OpenSession())
			{
				var query = session.Load<HashInfo>(key);

				if (query == null)
				{
					//hash not found
					return null;
				}
				else
				{
					var currentFile = new FileInfo(fileName);

					if (currentFile.CreationTimeUtc == query.Created &&
						fileName.ToLower() == query.FileName.ToLower() &&
						currentFile.LastWriteTimeUtc == query.LastEdited &&
						currentFile.Length == query.Size)
					{
						//all attributes match
						return query.Hash;
					}
					else
					{
						//hash not up to date
						return null;
					}
				}
			}
		}


		private HashInfo getHashInfo(string filename, string hash)
		{            
			if (!File.Exists(filename) || hash == null)
				return null;

			var result = new HashInfo();
			result.FileName = filename;
			result.Hash = hash;

			var info = new FileInfo(filename);

			result.Created = info.CreationTimeUtc;
			result.LastEdited = info.LastWriteTimeUtc;
			result.Size = info.Length;

			return result;
		}

		private void processQueuedFiles()
		{
			while (true)
			{
				string file = queue.Take();

				if (!File.Exists(file))
				{
					hashCompleted(file, null);
				}
				else
				{
					string hash = queryHash(file);
					if (hash != null)
					{
						hashCompleted(file, hash);
					}
					else
					{
						logger.Info("Calculating Hash for {0}", file);
						//calcualte the hash for the file
                        try
                        {
                            hash = calculateHash(file);
                        }
                        catch (IOException)
                        {
                            Thread.Sleep(1000);
                            queue.Add(file, Priority.Low);
                        }

						//store the file's hash for later use
						var info = getHashInfo(file, hash);


						
						using (var session = Program.Database.OpenSession())
						{
							//var results =
							//(
							//    from item in session.Query<HashInfo>()
							//    select item
							//)
							//.ToArray();

							//logger.Debug("ResultCount before Store: " + results.Length);

							session.Store(info, getKey(file));
							session.SaveChanges();


							//results =
							//(
							//    from item in session.Query<HashInfo>()
							//    select item
							//)
							//.ToArray();

							//logger.Debug("ResultCount after Store: " + results.Length);
						}
					}
				}


			}
		}


		private void hashCompleted(string filename, string hash)
		{
			if (this.GetHashAsyncCompleted != null)
			{
				this.GetHashAsyncCompleted(this, new GetHashEventArgs() { Path = filename, Hash = hash });
			}
		}

		#region IHashingService Members

		public event EventHandler<GetHashEventArgs> GetHashAsyncCompleted;

		public string GetHash(string filename, Priority priority)
		{
			if (!File.Exists(filename))
				return null;

			var hash = queryHash(filename);
			if (hash != null)
			{
				logger.Info("Found hash for {0}", filename);
				return hash;
			}


			EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
			this.GetHashAsyncCompleted += (s, e) => 
			{
				if (e.Path.ToLower() == filename.ToLower())
				{
					hash = e.Hash;
					waitHandle.Set();
				}
			};

			GetHashAsync(filename, priority);

			return hash;

		}
		
		public void GetHashAsync(string filename, Priority priority)
		{
			if (!File.Exists(filename))
			{
				hashCompleted(filename.ToLower(), null);
			}
			else
			{
				string hash = queryHash(filename);
				if (hash != null)
				{
					logger.Info("found hash for {0} in cache", filename);
					hashCompleted(filename, hash);
				}
				else
				{
					queue.Add(filename, priority);
				}
			}
		}


		#endregion

	}


	


	public class HashInfo
	{
		public string FileName { get; set; }

		public DateTime Created { get; set; }

		public DateTime LastEdited { get; set; }

		public long Size { get; set;}

		public string Hash { get; set; }
	}


}
