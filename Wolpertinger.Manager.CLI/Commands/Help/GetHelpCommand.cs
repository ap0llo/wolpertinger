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
			var allCommands = Context.CommadParser.KnownCommands.OrderBy(x => getCommandName(x));
			if (CommandName == null)
			{
				foreach (var cmd in allCommands)
				{
					printCommadName(cmd);
				}
			}
			else
			{
				var cmds = allCommands.Where(x => (x.Module + "." + x.Verb + "-" + x.Noun).ToLower().Contains(CommandName.ToLower()));
				var resultCount = cmds.Count();

				if(resultCount == 0)
				{
					Context.WriteError("Command not found");
				}
				else if (resultCount == 1)
				{
					printCommandDetails(cmds.First());
				}
				else
				{
					foreach (var cmd in cmds)
					{
						printCommadName(cmd);
					}
				}
			}

		}


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

	}
}
