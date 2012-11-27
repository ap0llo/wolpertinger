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

namespace Wolpertinger.Fileserver
{
    /// <summary>
    /// Implementation of IHashingService used by Wolpertinger Fileserver (singleton class)
    /// </summary>
    public class HashingService : IHashingService
    {

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

                List<HashInfo> all = session.Query<HashInfo>().ToList<HashInfo>();

                var info = session.Load<HashInfo>("filehash/" + filename.ToLower().GetHashSHA1());
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
                        session.Store(info, "filehash/" + file.ToLower().GetHashSHA1());
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

        /// <summary>
		/// Returns a SHA1-Hash of the file's content.
		/// </summary>
		/// <param name="fileName">The path of the file that's to be hashed.</param>
		/// <returns></returns>
		private string getHashSHA1(string fileName)
		{
			//Console.WriteLine("Generating Hash for " + fileName);

			string sHash = "";
			using (StreamReader sr = new StreamReader(fileName))
			{
				sHash = sha1h.ComputeHash(sr.BaseStream).ToHexString();
			}

            //Console.WriteLine("Done.");

			return sHash;
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
