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
using System.IO.Compression;
using System.Diagnostics;

namespace Nerdcave.Common.Extensions
{
    /// <summary>
    /// Class that contains extension methods for byte[]
    /// </summary>
    public static class ByteArrayExtensions
    {
        #region Private Fields
        
        private static Encoding asciiEncoding = new ASCIIEncoding();
        private static Encoding utfEncoding = new UTF8Encoding();
        private static Encoding iso88591encoding = Encoding.GetEncoding(28591);

        #endregion Private Fields


        #region Public Methods

        /// <summary>
        /// Gets a string of 2-digit hexadecimal numbers from the given bytes.
        /// </summary>
        public static string ToHexString(this byte[] array)
        {
            if (array == null) { return ""; }

            string str = "";
            
            foreach (byte b in array)
            {
                string value = Convert.ToString(b, 16);
                if (value.Length == 1) value = "0" + value;
                str += value;
            }

            return str;
        }

        /// <summary>
        /// Gets the string-representation of the given bytes in the ASCII-Standard
        /// </summary>
        public static string ToStringAscii(this byte[] bytes)
        {
            return asciiEncoding.GetString(bytes);
        }

        /// <summary>
        /// Gets the string-representation of the given bytes in the UTF8-Standard
        /// </summary>
        public static string ToStringUTF8(this byte[] bytes)
        {
            return utfEncoding.GetString(bytes);
        }

        /// <summary>
        /// Gets the string-representation of the given bytes in the ISO/IEC 8859-1-Standard
        /// </summary>
        public static string ToStringIso88591(this byte[] bytes)
        {
            return iso88591encoding.GetString(bytes);
        }

        /// <summary>
        /// Encodes the byte-array as base64-STring
        /// </summary>        
        /// <returns>Returns a base64 representation of the byte-array.</returns>
        public static string ToBase64String(this byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }


        /// <summary>
        /// Encrypts a byte-array using AES
        /// </summary>
        /// <param name="plainText">The data to encrypt</param>
        /// <param name="Key">The encryption key to be used</param>
        /// <param name="IV">The initialization vector to use</param>
        /// <returns>Returns the encrtypted data in a byte-array</returns>
        /// <remarks>Base on Code from CodeProject http://www.codeproject.com/Articles/5719/Simple-encrypting-and-decrypting-data-in-C </remarks>
        [DebuggerStepThrough()]
        public static byte[] Encrypt(this byte[] plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");

            // Create a MemoryStream to accept the encrypted bytes 
            MemoryStream ms = new MemoryStream();

            // Create a symmetric algorithm. 
            // We are going to use Rijndael because it is strong and
            // available on all platforms. 
            // You can use other algorithms, to do so substitute the
            // next line with something like 
            //      TripleDES alg = TripleDES.Create(); 
            AesCryptoServiceProvider alg = new AesCryptoServiceProvider();

            // Now set the key and the IV. 
            // We need the IV (Initialization Vector) because
            // the algorithm is operating in its default 
            // mode called CBC (Cipher Block Chaining).
            // The IV is XORed with the first block (8 byte) 
            // of the data before it is encrypted, and then each
            // encrypted block is XORed with the 
            // following block of plaintext.
            // This is done to make encryption more secure. 

            // There is also a mode called ECB which does not need an IV,
            // but it is much less secure. 
            alg.Key = Key;
            alg.IV = IV;

            // Create a CryptoStream through which we are going to be
            // pumping our data. 
            // CryptoStreamMode.Write means that we are going to be
            // writing data to the stream and the output will be written
            // in the MemoryStream we have provided. 
            CryptoStream cs = new CryptoStream(ms,
               alg.CreateEncryptor(), CryptoStreamMode.Write);

            // Write the data and make it do the encryption 
            cs.Write(plainText, 0, plainText.Length);

            // Close the crypto stream (or do FlushFinalBlock). 
            // This will tell it that we have done our encryption and
            // there is no more data coming in, 
            // and it is now a good time to apply the padding and
            // finalize the encryption process. 
            cs.Close();

            // Now get the encrypted data from the MemoryStream.
            // Some people make a mistake of using GetBuffer() here,
            // which is not the right way. 
            byte[] encryptedData = ms.ToArray();

            return encryptedData; 

        }

        /// <summary>
        /// Decrypts a byte-Array encrpyted using AES
        /// </summary>
        /// <param name="cipherText">The data to be decrypted</param>
        /// <param name="Key">The encryption key to use</param>
        /// <param name="IV">The initialisation vector used to encrypt the data</param>
        /// <returns>Returns a byte-array contianing the decrypted data</returns>
        /// <remarks>Based on Code from CodeProject http://www.codeproject.com/Articles/5719/Simple-encrypting-and-decrypting-data-in-C </remarks>
        [DebuggerStepThrough()]       
        public static byte[] DecryptAES(this byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");

            // Create a MemoryStream that is going to accept the
            // decrypted bytes 
            MemoryStream ms = new MemoryStream();

            // Create a symmetric algorithm. 
            // We are going to use Rijndael because it is strong and
            // available on all platforms. 
            // You can use other algorithms, to do so substitute the next
            // line with something like 
            //     TripleDES alg = TripleDES.Create(); 
            AesCryptoServiceProvider alg = new AesCryptoServiceProvider();

            // Now set the key and the IV. 
            // We need the IV (Initialization Vector) because the algorithm
            // is operating in its default 
            // mode called CBC (Cipher Block Chaining). The IV is XORed with
            // the first block (8 byte) 
            // of the data after it is decrypted, and then each decrypted
            // block is XORed with the previous 
            // cipher block. This is done to make encryption more secure. 
            // There is also a mode called ECB which does not need an IV,
            // but it is much less secure. 
            alg.Key = Key;
            alg.IV = IV;

            // Create a CryptoStream through which we are going to be
            // pumping our data. 
            // CryptoStreamMode.Write means that we are going to be
            // writing data to the stream 
            // and the output will be written in the MemoryStream
            // we have provided. 
            CryptoStream cs = new CryptoStream(ms,
                alg.CreateDecryptor(), CryptoStreamMode.Write);

            // Write the data and make it do the decryption 
            cs.Write(cipherText, 0, cipherText.Length);

            // Close the crypto stream (or do FlushFinalBlock). 
            // This will tell it that we have done our decryption
            // and there is no more data coming in, 
            // and it is now a good time to remove the padding
            // and finalize the decryption process. 
            cs.Close();

            // Now get the decrypted data from the MemoryStream. 
            // Some people make a mistake of using GetBuffer() here,
            // which is not the right way. 
            byte[] decryptedData = ms.ToArray();

            return decryptedData; 
        }




        /// <summary>
        /// Compresses the byte array using GZip
        /// </summary>
        /// <param name="source">The data to be compressed</param>
        /// <returns>Returns the compressed data as byte-array</returns>
        public static byte[] Compress(this byte[] source)
        {
            MemoryStream stream = new MemoryStream();
            GZipStream zipStream = new GZipStream(stream, CompressionMode.Compress);

            zipStream.Write(source, 0, source.Length);
            zipStream.Close();


            return stream.ToArray();
        }


        /// <summary>
        /// Decompresses a unicode-string compressed using String.Compress()
        /// </summary>
        /// <param name="compressed">The compressed data.</param>
        /// <returns>Returns the decompressed data interpreted as Unicode-String</returns>
        public static string DecompressToString(this byte[] compressed)
        {
            return compressed.Decompress().ToStringUTF8();
        }

        public static byte[] Decompress(this byte[] compressed)
        {
            MemoryStream stream = new MemoryStream(compressed);
            GZipStream zipStream = new GZipStream(stream, CompressionMode.Decompress);
            MemoryStream outStream = new MemoryStream();

            zipStream.CopyTo(outStream);
            zipStream.Close();

            return outStream.ToArray();
        }

        #endregion Public Methods
    }
}
