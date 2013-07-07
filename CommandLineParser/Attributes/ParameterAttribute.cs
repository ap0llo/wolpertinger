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
using System.Linq;
using System.Text;

namespace CommandLineParser.Attributes
{
    /// <summary>
    /// Attribute to identify a command's property as parameter
    /// </summary>
    public class ParameterAttribute : Attribute
    {
        /// <summary>
        /// The parameter's name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets whether the paramter is optional 
        /// </summary>
        public bool IsOptional { get; set; }

        /// <summary>
        /// The parameter's position if it can be used as positional parameter
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Initializes a new instance of ParameterAttribute
        /// </summary>
        /// <param name="name">The parameter's name</param>
        public ParameterAttribute(string name)
            : this (name, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of ParameterAttribute
        /// </summary>
        /// <param name="name">The parameter's name</param>
        /// <param name="isOptional">Specifies whether the parameter is optional</param>
        /// <param name="position">The parameter's position if it can be used as positional parameter</param>
        public ParameterAttribute(string name, bool isOptional, int position = -1)
        {
            //check if specified name is valid
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException("ParameterName may not be empty");
            }
            else if (name.StartsWith("-") || name.StartsWith("/"))
            {
                throw new ArgumentException("ParameterName may not start with '-' or '/'");
            }

            this.Name = name;
            this.IsOptional = isOptional;
            this.Position = position;
        }


    }
}
