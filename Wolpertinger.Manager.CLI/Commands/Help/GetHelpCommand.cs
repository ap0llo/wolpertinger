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
using CommandLineParser.Attributes;
using CommandLineParser.CommandParser;
using CommandLineParser.Info;

namespace Wolpertinger.Manager.CLI.Commands.Help
{
	[Command(CommandVerb.Get, "Help", "Help")]
	class GetHelpCommand : CommandBase
	{

		[Parameter("CommandName", IsOptional=true, Position=1)]
		public string CommandName { get; set; }


		public override void Execute()
		{
			//Get a list of all commands known to the CommandParser
			var allCommands = Context.CommadParser.KnownCommands.OrderBy(x => getCommandName(x));

			//if CommandName has not been specified, print a list of all available command
			if (CommandName == null)
			{
				foreach (var cmd in allCommands)
				{
					printCommadName(cmd);
				}
			}
			//if CommandName has been specified, find all commands that match that name
			else
			{
				var cmds = allCommands.Where(x => (x.Module + "." + x.Verb + "-" + x.Noun).ToLower().Contains(CommandName.ToLower()));
				var resultCount = cmds.Count();

				//No command found => error
				if(resultCount == 0)
				{
					throw new CommandExecutionException("Command not found");
				}
				//if only one result was found, display detailed help for that command
				else if (resultCount == 1)
				{
					printCommandDetails(cmds.First());
				}
				//if more than 1 command has been found, display a list of all matching commands
				else
				{
					foreach (var cmd in cmds)
					{
						printCommadName(cmd);
					}
				}
			}

		}


		#region Private Implementation

		private void printCommadName(CommandInfo cmd)
		{
			Context.WriteOutput(getCommandName(cmd));
		}

		private string getCommandName(CommandInfo cmd)
		{
			return (cmd.Module.IsNullOrEmpty() ? "" : cmd.Module + ".") + cmd.Verb + "-" + cmd.Noun;
		}

		private void printCommandDetails(CommandInfo command)
		{
			Context.WriteOutput();
			Context.WriteOutput("\t" + getCommandName(command));
			Context.WriteOutput();

			Context.WriteOutput("\tParameters:");
			foreach (var paramter in command.Parameters)
			{
				Context.WriteOutput("\t\t" + paramter.Name + " : " + paramter.DataType.Name +(paramter.IsOptional ? "  (optional)" : ""));
			}
			Context.WriteOutput();
		}

		#endregion

	}
}
