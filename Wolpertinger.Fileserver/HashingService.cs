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

		private static string hashDbFolder;

		//The only instance of HashingService (Singleton class)
		private static HashingService instance;

		private ILogger logger = LoggerService.GetLogger("HashingService");
		private SHA1CryptoServiceProvider sha1Provider = new SHA1CryptoServiceProvider();
		private PriorityBlockingQueue<string> queue = new PriorityBlockingQueue<string>();


		static HashingService()
		{
			instance = new HashingService();
			hashDbFolder = Path.Combine(Program.DatabaseFolder, "Hashes");
			//if (!Directory.Exists(hashDbFolder))
			//{
			//    Directory.CreateDirectory(hashDbFolder);
			//}
		}


		/// <summary>
		/// Gets the hashing service instance.
		/// </summary>
		public static IHashingService GetHashingService()
		{
			return instance;
		}


		/// <summary>
		/// Prevents a default instance of the <see cref="HashingService" /> class from being created.
		/// </summary>
		private HashingService()
		{
			//Start a new Thread to do the actual hashing
			var threadStart = new ThreadStart(delegate { processQueuedFiles(); });
			new Thread(threadStart).Start();
		}


		/// <summary>
		/// Gets the key used to store the hash in the database for the speicified filename
		/// </summary>
		/// <param name="fileName">Name of the file to genereate the key for</param>
		/// <returns>Returns a key for the speicified filename</returns>
		private string getKey(string fileName)
		{
			return BinaryRage.Key.GenerateMD5Hash(fileName);
		}

		/// <summary>
		/// Returns a SHA1-Hash of the file's content.
		/// </summary>
		/// <param name="fileName">The path of the file that's to be hashed.</param>
		/// <returns></returns>
		private string calculateHash(FileStream stream)
		{
			string sHash = "";
			using (StreamReader sr = new StreamReader(stream))
			{
				sHash = sha1Provider.ComputeHash(sr.BaseStream).ToHexString();
			}

			return sHash;
		}

		/// <summary>
		/// Gets the hash for the specified file from the cache and checks if the cached value is still valid
		/// </summary>
		/// <returns>
		/// Returns the file's hash from the cache or null if no hash or only an outdated hash has been found
		/// </returns>
		private string queryHash(string fileName)
		{
			//normalize filename
			fileName = fileName.Replace("/", "\\");

			var key = getKey(fileName);
			HashInfo query;

			try
			{
				query = BinaryRage.DB<HashInfo>.Get(key, hashDbFolder);
			}
			catch (DirectoryNotFoundException)
			{
				query = null;
			}

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


		/// <summary>
		/// Generates a new instance of HashInfo which stores information about the hash
		/// </summary>
		/// <param name="filename">The name of the file that has been hashed</param>
		/// <param name="hash">The file's hash</param>
		/// <returns>Returns a new instance of HashInfo containing information avout the specified file. On error returns null</returns>
		private HashInfo getHashInfo(string filename, string hash)
		{            
			//check if file exists and hash is valid
			if (!File.Exists(filename) || hash.IsNullOrEmpty())
				return null;

			//Initialize new instance of HashInfo
			var result = new HashInfo() { FileName = filename, Hash = hash };

			//Load information about the file from disk and set HashInfo properties
			var info = new FileInfo(filename);
			
			result.Created = info.CreationTimeUtc;
			result.LastEdited = info.LastWriteTimeUtc;
			result.Size = info.Length;

			return result;
		}


		/// <summary>
		/// Runs in an infinite loop and calculates hashes for all files that have been queued.
		/// If the queue is empty, waits until new items are enqueued
		/// </summary>
		private void processQueuedFiles()
		{
			while (true)
			{
				//get the next file to be hashed from the queue
				string file = queue.Take();

				//cannot calculate hash if file does not exist
				if (!File.Exists(file))
				{
					hashCompleted(file, null);
				}
				else
				{
					//check if a up-to-date hash is stored in the cache
					string hash = queryHash(file);
					if (hash != null)
					{
						hashCompleted(file, hash);
					}
					//calculate the hash for the file
					else
					{
						//Lock file to prevent modifications while hash is calculated and stored in the cache
						var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
						
						logger.Info("Calculating Hash for {0}", file);
						try
						{
							hash = calculateHash(stream);
						}
						catch (IOException)
						{
							//error calculating hash, re-enqueue file and try again later
							Thread.Sleep(1000);
							queue.Add(file, Priority.Low);
						}

						//store the file's hash for later use
						
						//wrap the hash into a HashInfo object
						var info = getHashInfo(file, hash);

						//store HashInfo in cache
						BinaryRage.DB<HashInfo>.Insert(getKey(file), info, hashDbFolder);

                        hashCompleted(file, hash);

						//Release lock on the file
						stream.Close(); 
					}
				}


			}
		}

		/// <summary>
		/// Raises the GetHashAsyncCompleted event with the specified values
		/// </summary>
		/// <param name="filename">The file which's hash has been calculated</param>
		/// <param name="hash">The file's hash</param>
		private void hashCompleted(string filename, string hash)
		{
			if (this.GetHashAsyncCompleted != null)
				this.GetHashAsyncCompleted(this, new GetHashEventArgs() { Path = filename, Hash = hash });
		}


		#region IHashingService Members

		/// <summary>
		/// Occurs when a file's hash value has been calculated
		/// </summary>
		public event EventHandler<GetHashEventArgs> GetHashAsyncCompleted;

		/// <summary>
		/// Queues the specified file for hashing and returns immediatelly
		/// </summary>
		/// <param name="filename">The name of the file to be hashed</param>
		/// <returns>Returns the file's hash. On errror returns null</returns>
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


            waitHandle.WaitOne();

			return hash;

		}

		/// <summary>
		/// Queues the specified file for hashing and waits for the hash to be finished
		/// </summary>
		/// <param name="filename">The name of the file nto be hashed</param>
		/// <returns>Returns the hash of the file's contents as string (base64-encoded)</returns>
		public void GetHashAsync(string filename, Priority priority)
		{
			if (!File.Exists(filename))
			{
				hashCompleted(filename.ToLower(), null);
			}
			else
			{
				queue.Add(filename, priority);
			}
		}


		#endregion

	}


	

	/// <summary>
	/// Class used to store hashes in the cache
	/// </summary>
	[Serializable]
	public class HashInfo
	{
		/// <summary>
		/// The name of the file that has been hashed
		/// </summary>
		public string FileName { get; set; }

		/// <summary>
		/// The Creation-Time in UTC of the file at the time the hash was calculated
		/// </summary>
		public DateTime Created { get; set; }

		/// <summary>
		/// The LastEdited time in UTC of the file at the time the hash was calculated
		/// </summary>
		public DateTime LastEdited { get; set; }

		/// <summary>
		/// The file's size at the time the hash was calculated
		/// </summary>
		public long Size { get; set;}

		/// <summary>
		/// The hash of the file 
		/// </summary>
		public string Hash { get; set; }
	}


}
