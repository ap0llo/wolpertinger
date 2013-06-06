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
        CommandContext context;
        List<CommandInfo> commands = new List<CommandInfo>();


        public IEnumerable<CommandInfo> KnownCommands
        {
            get
            {
                return commands;
            }
        }


        public CommandParser(CommandContext context)
        {
            this.context = context;
            context.CommadParser = this;
        }


        public CommandBase GetCommand(string input)
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


            var commandInfo = findType(module, verb, noun);
            var commandInstance = (CommandBase)Activator.CreateInstance(commandInfo.Type);
            commandInstance.Context = this.context;


            //set properties

            HashSet<CommandParamterInfo> setParameters = new HashSet<CommandParamterInfo>();
            bool[] processedArgs = new bool[args.Count];
            //first paramter has alredy been processed (for finding the apropriate command)
            processedArgs[0] = true;            


            //first get named paramters
            for (int i = 1; i < args.Count; i++)
            {
                string arg = args[i];                
                //check if current arg is the name of a parameter
                if(arg.StartsWith("-"))
                {
                    var paramterNameQuery = commandInfo.Parameters.Where(x => "-" + x.Name.ToLower() == arg.ToLower());
                    var count = paramterNameQuery.Count();

                    if (count == 1)
                    {
                        if (i + 1 < args.Count)
                        {
                            var parameterInfo = paramterNameQuery.First();
                            string valueStr = args[++i];
                            object value = parseValue(valueStr,parameterInfo.PropertyType);
                            parameterInfo.SetMethod.Invoke(commandInstance, new object[] { value});

                            processedArgs[i - 1] = true;
                            processedArgs[i] = true;

                            setParameters.Add(parameterInfo);
                        }
                        else 
                        {
                            throw new CommandParserException("Parameter {0} has been declared without value", arg);
                        }
                    }
                    else if (count > 1)
                    {
                        throw new CommandParserException("Parameter {0} has been declaed more than once", paramterNameQuery.First().Name);
                    }
                }
            }



            //find missing paramters (positional)
            for (int i = 1; i < args.Count; i++)
            {
                //if argument has already been processed, skip 
                if (processedArgs[i])
                {
                    continue;
                }

                //find all paramters not already set
                var unsetParamters = commandInfo.Parameters.Where(x => !setParameters.Contains(x)).OrderBy(x => x.Position);

                if (unsetParamters.Any())
                {
                    var nextParamter = unsetParamters.First();
                    object value = parseValue(args[i], nextParamter.PropertyType);

                    nextParamter.SetMethod.Invoke(commandInstance, new object[] { value });

                    setParameters.Add(nextParamter);
                    processedArgs[i] = true;
                }
            }


            //check if all args have ben processed
            if (processedArgs.Any(x => !x))
            {
                throw new CommandParserException("Not all arguments could were processed");
            }



            var missingRequiredParamters = commandInfo.Parameters.Where(x => !x.IsOptional && !setParameters.Contains(x));

            //ask the user to supply values for missing paramters
            if (missingRequiredParamters.Any())
            {
                Console.WriteLine("You need to supply the value for the follwing parameters: ");
            }

            
            foreach (var parameter in missingRequiredParamters )
            {
                Console.Write("{0}: ", parameter.Name);
                string valueStr = Console.ReadLine();

                object value = parseValue(valueStr, parameter.PropertyType);

                if (value == null)
                {
                    throw new CommandParserException("Invalid value");
                }
                else
                {
                    parameter.SetMethod.Invoke(commandInstance, new object[] { value });
                    setParameters.Add(parameter);
                }
            }

            //check if all required parameters have been set
            if (missingRequiredParamters.Any())
            {                
                throw new CommandParserException("Not all required parameters could be set");
            }
            return commandInstance;
        }


        private object parseValue(string value, Type toType)
        {
            if (toType == typeof(string))
            {
                return value;
            }
            else if (toType == typeof(bool))
            {
                bool outValue;
                if (bool.TryParse(value, out outValue))
                {
                    return outValue;
                }
                else
                {
                    throw new CommandParserException("Cannot parse '{0}' as bool", value);
                }
            }
            else if (toType == typeof(Guid))
            {
                Guid outValue;
                if (Guid.TryParse(value, out outValue))
                {
                    return outValue;
                }
                else
                {
                    throw new CommandParserException("Cannot parse '{0}' as Guid", value);
                }
            }
            else if (toType == typeof(Core.LogLevel))
            {
                Core.LogLevel lvl;

                if (Enum.TryParse<Core.LogLevel>(value, true, out lvl))
                {
                    return lvl;
                }
                else
                {
                    throw new CommandParserException("Cannot parse '{0}' as LogLevel", value);
                }
            }
            else
            {
                throw new CommandParserException("Cannot parse argument to type {0}", toType.FullName);
            }
        }

        private CommandInfo findType(string module, string verb, string noun)
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
                return commandsQuery.First();
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
     
        public void LoadCommandsFromAssembly(Assembly assembly)
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
                                Type = type,
                                Parameters = loadParamters(type)});
                    }
                }
            }
        }

        private List<CommandParamterInfo> loadParamters(Type commandType)
        {
            var result = new List<CommandParamterInfo>();

            //iterate over all of the command's properties
            foreach (var property in commandType.GetProperties())
            {
                var attributes = property.GetCustomAttributes(typeof(ParameterAttribute), true);

                if (attributes.Any())
                {
                    var paramInfo = new CommandParamterInfo(attributes.First() as ParameterAttribute);
                    paramInfo.PropertyType = property.PropertyType;
                    paramInfo.SetMethod = property.GetSetMethod();
                    result.Add(paramInfo);
                }
            }

            return result;
        }



       
        

    }


    public class CommandInfo
    {
        public string Module { get; set; }

        public string Verb { get; set; }

        public string Noun { get; set; }

        public Type Type { get; set; }

        public List<CommandParamterInfo> Parameters { get; set; }
    }

    public class CommandParamterInfo
    {
        public string Name { get; set; }

        public bool IsOptional { get; set; }

        public int Position { get; set; }

        public Type PropertyType { get; set; }

        public MethodInfo SetMethod { get; set; }

        public CommandParamterInfo()
        {

        }

        public CommandParamterInfo(ParameterAttribute attribute)
        {
            this.Name = attribute.Name;
            this.IsOptional = attribute.IsOptional;
            this.Position = attribute.Position;
        }

    }

    public class CommandParserException : Exception
    {
        public CommandParserException(string message)
            : base(message)
        {

        }


        public CommandParserException(string format, params object[] args)
            : this(String.Format(format, args))
        {
        }
    }
}
