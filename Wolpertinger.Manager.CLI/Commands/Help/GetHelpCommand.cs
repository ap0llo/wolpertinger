using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nerdcave.Common.Extensions;

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
				Context.WriteOutput("\t\t" + paramter.Name + " : " + paramter.PropertyType.Name +(paramter.IsOptional ? "  (optional)" : ""));
			}
			Context.WriteOutput();
		}

		#endregion

	}
}
