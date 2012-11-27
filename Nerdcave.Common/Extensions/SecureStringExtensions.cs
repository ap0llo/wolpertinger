/* Copyright (c) 2010, Andreas Grünwald
 * All rights reserved.
 *
 * The SecureStringExtensions class in this file is based on code from Philipp Sumi
 * See http://www.hardcodet.net/2009/04/dpapi-string-encryption-and-extension-methods
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace Nerdcave.Common.Extensions
{
    public static class SecureStringExtensions
    {     
        /// <summary>
        /// Unwraps the contents of a secured string and
        /// returns the contained value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks>Be aware that the unwrapped managed string can be
        /// extracted from memory.</remarks>
        /// <exception cref="ArgumentNullException">If <paramref name="value"/>
        /// is a null reference.</exception>
        public static string Unwrap(this SecureString value)
        {
            if (value == null) throw new ArgumentNullException("value");

            IntPtr ptr = Marshal.SecureStringToCoTaskMemUnicode(value);
            try
            {
                return Marshal.PtrToStringUni(ptr);
            }
            finally
            {
                Marshal.ZeroFreeCoTaskMemUnicode(ptr);
            }
        }

        /// <summary>
        /// Checks whether a <see cref="SecureString"/> is either
        /// null or has a <see cref="SecureString.Length"/> of 0.
        /// </summary>
        /// <param name="value">The secure string to be inspected.</param>
        /// <returns>True if the string is either null or empty.</returns>
        public static bool IsNullOrEmpty(this SecureString value)
        {
            return value == null || value.Length == 0;
        }

        /// <summary>
        /// Performs bytewise comparison of two secure strings.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="other"></param>
        /// <returns>True if the strings are equal.</returns>
        public static bool Matches(this SecureString value, SecureString other)
        {
            if (value == null && other == null) return true;
            if (value == null || other == null) return false;
            if (value.Length != other.Length) return false;
            if (value.Length == 0 && other.Length == 0) return true;

            IntPtr ptrA = Marshal.SecureStringToCoTaskMemUnicode(value);
            IntPtr ptrB = Marshal.SecureStringToCoTaskMemUnicode(other);
            try
            {
                //parse characters one by one - doesn't change the fact that
                //we have them in memory however...
                byte byteA = 1;
                byte byteB = 1;

                int index = 0;
                while (((char)byteA) != '\0' && ((char)byteB) != '\0')
                {
                    byteA = Marshal.ReadByte(ptrA, index);
                    byteB = Marshal.ReadByte(ptrB, index);
                    if (byteA != byteB) return false;
                    index += 2;
                }

                return true;
            }
            finally
            {
                Marshal.ZeroFreeCoTaskMemUnicode(ptrA);
                Marshal.ZeroFreeCoTaskMemUnicode(ptrB);
            }
        }
    }
}
