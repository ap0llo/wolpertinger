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
using System.Security.Cryptography;
using System.IO;
using System.Diagnostics;
using System.Security;
using System.IO.Compression;

namespace Nerdcave.Common.Extensions
{

    /// <summary>
    /// Class that contains extension methods for string
    /// </summary>
    public static class StringExtensions
    {
        #region Private Fields
        
        private static Encoding asciiEncoding = new ASCIIEncoding();
        private static Encoding utf8Encoding = new UTF8Encoding();
        private static Encoding iso88591encoding = Encoding.GetEncoding(28591);

        #endregion Private Fields


        #region Public Methods
        
        /// <summary>
        /// Splits the indicated string at space characters. Elemtens will be kept together if they are put in quotes.
        /// The Quotes will be removed.
        /// </summary>
        /// <param name="inputString">The string that is to be splitted.</param>
        /// <returns>Returns a list of strings containing the result of the splitting.</returns>
        [DebuggerStepThrough()]
        public static List<string> SpaceSplitString(this string inputString)
        {

            if (inputString.Count(x => x == '"') % 2 != 0)
                throw new FormatException(); 

            List<string> cmds = inputString.Split(' ').ToList<string>();

            int maxIndex = cmds.Count;

            for (int i = 0; i < maxIndex; i++)
            {
                if ((cmds[i].Count(f => f == '"')).IsEven())
                {
                    cmds[i] = cmds[i].Replace("\"", "");
                }
                else if (i < cmds.Count)
                {
                    cmds[i] += " " + cmds[i + 1];
                    cmds.RemoveAt(i + 1);
                    i -= 1;
                    maxIndex -= 1;
                }
            }

            return new List<string>(cmds.Where(x=> !x.IsNullOrEmpty()));
        }

        /// <summary>
        /// Splits the string into the indicated amount of fragments (chunck sizes will be approximately the same)
        /// </summary>
        /// <param name="str"></param>
        /// <param name="fragmentsCount"></param>
        /// <returns></returns>
        public static string[] Split(this string str, int fragmentsCount)
        {
            if (fragmentsCount == 1)
            {
                return new string[] { str };
            }

            string[] result = new string[fragmentsCount];
            int length = (str.Length / fragmentsCount) + 1;           
            
            int chunckLength;

            for (int i = 0; i < result.Length; i++)
            {
                chunckLength = str.Length < length ? str.Length : length;                

                result[i] = str.Substring(0, chunckLength);
                str = str.Remove(0, chunckLength);
            }            

            return result;
        }

        /// <summary>
        /// Gets the bytes from a hex-encoded string
        /// </summary>
        [DebuggerStepThrough()]
        public static byte[] GetBytesHexString(this string str)
        {
            if (str.IsNullOrEmpty()) { return null; }

            byte[] arr = new byte[str.Length / 2];

            int posArr = 0;
            for (int i = 0; i <= str.Length - 2; i += 2)
            {
                arr[posArr] = Convert.ToByte(str.Substring(i, 2), 16);
                posArr++;
            }

            return arr;
        }

        /// <summary>
        /// Check whether the specified string is null or empty
        /// </summary>
        [DebuggerStepThrough()]
        public static bool IsNullOrEmpty(this string str)
        {
            return String.IsNullOrEmpty(str);
        }

        /// <summary>
        /// Removes the last character from the string (if it has any characters)
        /// </summary>
        [DebuggerStepThrough()]
        public static string RemoveLastChar(this string str)
        {
            if (str.IsNullOrEmpty())
                return str;
            else
                return str.Remove(str.Length - 1);
        }

        /// <summary>
        /// Removes the first character from the string (if it has any characters)
        /// </summary>
        [DebuggerStepThrough()]
        public static string RemoveFirstChar(this string str)
        {
            if (str.IsNullOrEmpty())
                return str;
            else
                return str.Remove(0, 1);
        }
        
        /// <summary>
        /// Performs an AES encryptions of a string using the given key and IV. Result is base64-encoded
        /// </summary>
        /// <param name="text">The text that is to be encrypted</param>
        /// <param name="key">The key that is to be used</param>
        /// <param name="IV">The Initialization Vector that is to be used.</param>
        /// <returns>Returns the encrypted data, base64 encoded</returns>
        [DebuggerStepThrough()]
        public static string EncryptAES(this string text, byte[] key, byte[] IV)
        {
            return text.GetBytesUTF8().Encrypt(key, IV).ToStringBase64();            
        }

        /// <summary>
        /// AES-decrypts a base64-encoded string using the indicated key and IV
        /// </summary>
        /// <param name="ciphertext">The cipher-text that is to be decrypted</param>
        /// <param name="key">The key that is to be used</param>
        /// <param name="IV">The Initialization Vector that is to be used.</param>
        /// <returns>Returns the decrypted string. (Interpreted as UTF8)</returns>
        [DebuggerStepThrough()]
        public static string DecryptAES(this string ciphertext, byte[] key, byte[] IV)
        {
            return ciphertext.GetBytesBase64().DecryptAES(key, IV).ToStringUTF8();            
        }


        /// <summary>
        /// Gets the bytes of the given text in the ASCII-Standard
        /// </summary>
        [DebuggerStepThrough()]
        public static byte[] GetBytesASCII(this string text)
        {
            return asciiEncoding.GetBytes(text);
        }

        /// <summary>
        /// Gets the bytes of the given string in the UTF8-Standard
        /// </summary>
        [DebuggerStepThrough()]
        public static byte[] GetBytesUTF8(this string text)
        {
            return utf8Encoding.GetBytes(text);
        }

        /// <summary>
        /// Gets the bytes of the given string in the ISO/IEC 8859-1 Standard (extended ASCII)
        /// </summary>
        [DebuggerStepThrough()]
        public static byte[] GetBytesIso88591(this string text)
        {
            return iso88591encoding.GetBytes(text);
        }

        /// <summary>
        /// Gets the byte-array from a base64-string
        /// </summary>
        /// <param name="text">The base64 string</param>
        /// <returns>Returns the bytes taht were encoded in the source string or null if the string was invalid.</returns>
        public static byte[] GetBytesBase64(this string text)
        {
            //return null if text is empty
            if (text.IsNullOrEmpty())
                return null;

            try
            {
                return Convert.FromBase64String(text);
            }
            catch (FormatException)
            {
                //return null if text was no base64-string 
                return null;
            }
        }

        /// <summary>
        /// Computes the hash of the given string using the SHA1-algorithm
        /// </summary>
        /// <param name="text">The string that is to be hashed.</param>
        /// <returns>Returns the hash-code encoded as string of 2-digit hexadecimal numbers.</returns>
        [DebuggerStepThrough()]
        public static string GetHashSHA1(this string text)
        {
            SHA1CryptoServiceProvider sha1h = new SHA1CryptoServiceProvider();
            string hash = sha1h.ComputeHash(text.GetBytesUTF8()).ToHexString();

            return hash;
        }


        /// <summary>
        /// Wraps a managed string into a <see cref="SecureString"/> 
        /// instance.
        /// </summary>
        /// <param name="value">A string or char sequence that 
        /// should be encapsulated.</param>
        /// <returns>A <see cref="SecureString"/> that encapsulates the
        /// submitted value.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="value"/>
        /// is a null reference.</exception>
        /// <remarks>Based on code from Philipp Sumi. See http://www.hardcodet.net/2009/0
        public static SecureString ToSecureString(this IEnumerable<char> value)
        {
            if (value == null)
                return null;

            var secured = new SecureString();

            var charArray = value.ToArray();
            for (int i = 0; i < charArray.Length; i++)
            {
                secured.AppendChar(charArray[i]);
            }

            secured.MakeReadOnly();
            return secured;
        }

        /// <summary>
        /// Compresses the string using GZip
        /// </summary>
        /// <param name="source">The string to be compressed</param>
        /// <returns>Returns the compressed data as byte-array</returns>
        public static byte[] Compress(this string source)
        {
            return source.GetBytesUTF8().Compress();
        }



        public static string FormatEscape(this string source)
        {
            return source
                    .Replace("{", "{{")
                    .Replace("}", "}}");
        }

        #endregion Public Methods     

    }
}
