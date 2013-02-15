/*
 * 
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.FileShareCommon
{
    /// <summary>
    /// Interface for the service for creating hash values of files
    /// </summary>
    public interface IHashingService
    {
        /// <summary>
        /// Occurs when a file's hash value has been calculated
        /// </summary>
        event EventHandler<GetHashEventArgs> GetHashAsyncCompleted;

        /// <summary>
        /// Queues the specified file for hashing and waits for the hash to be finished
        /// </summary>
        /// <param name="filename">The name of the file nto be hashed</param>
        /// <returns>Returns the hash of the file's contents as string (base64-encoded)</returns>
        /// <returns>Returns the file's hash. On errror returns null</returns>
        string GetHash(string filename, Priority priority);

        /// <summary>
        /// Queues the specified file for hashing and returns immediatelly
        /// </summary>
        /// <param name="filename">The name of the file to be hashed</param>
        void GetHashAsync(string filename, Priority priority);

    }


    /// <summary>
    /// EventArgs for the GetHashAsyncCompleted event
    /// </summary>
    public class GetHashEventArgs : EventArgs
    {
        /// <summary>
        /// The name of the file that has been hashed
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The calculated hash value
        /// </summary>
        public string Hash { get; set; }
    }
}
