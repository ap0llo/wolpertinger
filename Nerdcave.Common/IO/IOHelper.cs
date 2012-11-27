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
using System.Runtime.InteropServices;
using Nerdcave.Common.Extensions;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;
using ReparsePoints;

namespace Nerdcave.Common.IO
{
    public static class IOHelper
    {
        /// <summary>
        /// Creates a symbolic link.
        /// Imported Windows API call. Use with care
        /// </summary>
        [DllImport("kernel32.dll")]
        public static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, int dwFlags);

        /// <summary>
        /// Checks whether the indicated path refers to a file or a directory
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsDirectory(string path)
        {
            // get the file attributes for file or directory
            FileAttributes attr = File.GetAttributes(path);

            //detect whether its a directory or file
            return ((attr & FileAttributes.Directory) == FileAttributes.Directory);
        }

        /// <summary>
        /// Determines whether the specified path os a ReparsePoint
        /// </summary>
        /// <param name="path">The path to check</param>
        public static bool IsReparsePoint(string path)
        {
            return File.GetAttributes(path).HasFlag(FileAttributes.ReparsePoint);
        }

        /// <summary>
        /// Determines whether the specified path is a symbolic link
        /// </summary>
        /// <param name="path">The path to check</param>
        public static bool IsSymbolicLink(string path)
        {
            if (File.Exists(path) || Directory.Exists(path))
            {
                ReparsePoint p = new ReparsePoint(path);
                return (p.Tag == ReparsePoint.TagType.SymbolicLink);
            }
            else
            {
                throw new DirectoryNotFoundException(String.Format("The item at '{0}' could not be found", path));
            }
        }

        /// <summary>
        /// Determines whether the specified path is a Junction Point
        /// </summary>
        /// <param name="path">The path to check</param>
        public static bool IsJunctionPoint(string path)
        {
            ReparsePoint p = new ReparsePoint(path);
            return (p.Tag == ReparsePoint.TagType.JunctionPoint);
        }

        /// <summary>
        /// Gets the target of a Symbloic Link or Junction Point
        /// </summary>
        /// <param name="path">The path of the Symbolic link or Junction Point</param>
        /// <returns>Returns the path the Reparse Point points to</returns>
        public static string GetLinkTarget(string path)
        {
            ReparsePoint p = new ReparsePoint(path);
            return p.Target;
        }

    }
}
