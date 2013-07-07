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

#if DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wolpertinger.Core;
using CommandLineParser.Attributes;
using CommandLineParser.CommandParser;

namespace Wolpertinger.Manager.CLI.Commands
{

    [Command(CommandVerb.Test, "CommandParser", "Debug")]
    class TestCommandParser : CommandBase<CommandContext>
    {


        [Parameter("Connection", IsOptional = true, Position = 1)]
        public IClientConnection Connection { get; set; }


        //[Parameter("String1", IsOptional = false, Position = 1)]
        //public string String1 { get; set; }


        //[Parameter("String2", IsOptional = true, Position = 2)]
        //public string String2 { get; set; }

        //[Parameter("Bool1", IsOptional = true, Position = 3)]
        //public bool Bool1 { get; set; }

        //[Parameter("String3", IsOptional = false, Position = 4)]
        //public string String3 { get; set; }

        //[Parameter("Bool2", IsOptional = true, Position = 5)]
        //public bool Bool2 { get; set; }


        public override void Execute()
        {
            Context.WriteOutput("Connection: {0}", Connection.Target);
            //Context.WriteOutput("String2: {0}", String2);
            //Context.WriteOutput("String3: {0}", String3);
            //Context.WriteOutput("Bool1: {0}", Bool1);
            //Context.WriteOutput("Bool2: {0}", Bool2);

        }
    }
}

#endif
