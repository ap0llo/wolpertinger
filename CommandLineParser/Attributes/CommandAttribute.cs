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
using Nerdcave.Common.Extensions;

namespace CommandLineParser.Attributes
{
	public class CommandAttribute : Attribute
	{
		static HashSet<char> forbiddenCharacters = new HashSet<char>() { '.', '-' };


		public string Verb { get; set; }

		public string Noun { get; set; }

		public string Module { get; set; }


		public CommandAttribute(CommandVerb verb, string noun, string module = "") 
			: this(verb.ToString(), noun, module)
		{
		}

		public CommandAttribute(string verb, string noun, string module = "")
		{
			if (noun.IsNullOrEmpty() || verb.IsNullOrEmpty())
			{
				throw new ArgumentException("'Noun' and 'Verb' must not be empty");
			}

			if (verb.Any(x => forbiddenCharacters.Contains(x)) || noun.Any(x => forbiddenCharacters.Contains(x)))
			{
				throw new ArgumentException("'Noun' or 'Verb' contains illegal characters");
			}

			
			this.Verb = verb;
			this.Noun = noun;
			this.Module = module;
		}

	}


	public enum CommandVerb
	{
		Get, 
		Set,
		New,
		Remove,
		Enter, 
		Exit,
		Start,
		Close,
		Authenticate,
		Test,
		Clear,
		Compare
	}

}
