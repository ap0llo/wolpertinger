using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nerdcave.Common.Extensions;
using System.Reflection;

namespace Wolpertinger.Manager.CLI
{
    class CommandParser
    {

        static Dictionary<string, Dictionary<string, Dictionary<string, Type>>> types = new Dictionary<string, Dictionary<string, Dictionary<string, Type>>>();

        static List<CommandInfo> commands = new List<CommandInfo>();


        public static CommandBase GetCommand(string input)
        {
            var args = input.SpaceSplitString();

            if (!args.Any())
            {
                throw new CommandParserException("Empty input-string");
            }


            var commandParts = args.First().Split(new char[] { '-', '.'});

            string module = "";
            string verb = "";
            string noun = "";

            if (commandParts.Length == 3)
            {
                module = commandParts[0];
                verb = commandParts[1];
                noun = commandParts[2];
            }
            else if (commandParts.Length == 2)
            {
                verb = commandParts[0];
                noun = commandParts[1];
            }
            else
            {
                throw new CommandParserException("Could not parse first argument: " + args.First());
            }


            var type = findType(module, verb, noun);

            return (CommandBase)Activator.CreateInstance(type);
        }


        private static Type findType(string module, string verb, string noun)
        {
            module = module.ToLower();
            verb = verb.ToLower();
            noun = noun.ToLower();

            var commandsQuery = module.IsNullOrEmpty() ? commands.Where(x => x.Module.ToLower() == module) : commands;

            commandsQuery = commands.Where(x => x.Verb.ToLower() == verb)
                                    .Where(x => x.Noun.ToLower() == noun);

            var count = commandsQuery.Count();
            if (count == 1)
            {
                return commandsQuery.First().Type;
            }
            else if (count > 1)
            {
                throw new CommandParserException("Ambiguous command. Specify module, too");
            }
            else
            {
                throw new CommandParserException("Command not found");
            }

        }



      

        public static void LoadCommandsFromAssembly(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                //check if type is derived from CommandBase
                if (type.IsSubclassOf(typeof(CommandBase)))
                {
                    //check if instances of the type can be created
                    try
                    {
                        object test = Activator.CreateInstance(type);
                    }
                    catch (MissingMethodException)
                    {
                        continue;
                    }

                    var attributes = type.GetCustomAttributes(typeof(CommandAttribute), false);
                    if (attributes == null || !attributes.Any())
                    {
                        continue;
                    }

                    foreach (var item in attributes)
                    {
                        var info = item as CommandAttribute;
                        commands.Add(new CommandInfo() { 
                                Module = info.Module, 
                                Verb = info.Verb, 
                                Noun = info.Noun, 
                                Type = type });
                    }
                }
            }
        }


        private class CommandInfo
        {
            public string Module { get; set; }

            public string Verb { get; set; }

            public string Noun { get; set; }

            public Type Type { get; set; }
        }


    }


    public class CommandParserException : Exception
    {
        public CommandParserException(string message)
            : base(message)
        {

        }
    }
}
