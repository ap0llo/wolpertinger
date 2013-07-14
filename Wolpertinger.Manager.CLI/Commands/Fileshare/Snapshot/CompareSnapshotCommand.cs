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
using CommandLineParser.Attributes;
using CommandLineParser.CommandParser;
using System.IO;
using System.Xml.Linq;
using Wolpertinger.FileShareCommon;

namespace Wolpertinger.Manager.CLI.Commands.Fileshare.Snapshot
{
	[Command(CommandVerb.Compare, "Snapshot","FileShare")]
	class CompareSnapshotCommand : ConnectionDependentCommand
	{
        [Parameter("SnapshotIdLeft", Position = 1, ParameterSet = "ImplicitConnection")]
        [Parameter("SnapshotIdLeft", Position = 2, ParameterSet = "ExplicitConnection")]
		public Guid SnapshotIdLeft { get; set; }

        [Parameter("SnapshotIdRight", Position = 2, ParameterSet = "ImplicitConnection")]
        [Parameter("SnapshotIdRight", Position = 3, ParameterSet = "ExplicitConnection")]
		public Guid SnapshotIdRight { get; set; }

        [Parameter("OutFile", Position = 3, IsOptional = true, ParameterSet = "ImplicitConnection")]
        [Parameter("OutFile", Position = 4, IsOptional = true, ParameterSet = "ExplicitConnection")]
		public string OutFile { get; set; }

		public override void Execute()
		{
			if (SnapshotIdLeft == SnapshotIdRight)
			{
				throw new CommandExecutionException("Tried to compare snapshot to itself");
			}

            var client = new FileShareClientComponent() { ClientConnection = getClientConnection() };


			var task = client.CompareSnapshots(SnapshotIdLeft, SnapshotIdRight);
			var diff = task.Result;

			if (OutFile != null)
			{
				try
				{
					var document = new XDocument(diff.Serialize());

					using (var writer = new StreamWriter(File.Open(OutFile, FileMode.Create,FileAccess.ReadWrite)))
					{
						document.Save(writer);
					}
				}
				catch (IOException ex)
				{
					throw new CommandExecutionException("IOException: " + ex.Message, ex);                    
				}
			}

            FileShareCommadHelpers.PrintDirectoryObjectDiff(diff, this.Context);			
		}


       
    

	}
}
