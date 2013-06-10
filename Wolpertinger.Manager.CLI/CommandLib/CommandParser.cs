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
using System.Reflection;
using System.Security;
using Nerdcave.Common;

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
                        commands.Add(new CommandInfo()
                        {
                            Module = info.Module,
                            Verb = info.Verb,
                            Noun = info.Noun,
                            Type = type,
                            Parameters = loadParamters(type)
                        });
                    }
                }
            }
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
                            object value = parseValue(valueStr,parameterInfo.DataType);
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

                //find all paramters not already set that are positional parameters
                var unsetParamters = commandInfo.Parameters.Where(x => !setParameters.Contains(x) && x.Position > 0).OrderBy(x => x.Position);

                if (unsetParamters.Any())
                {
                    var nextParamter = unsetParamters.First();
                    object value = parseValue(args[i], nextParamter.DataType);

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
                Console.WriteLine("You need to supply the value for the following parameters: ");
            }

            
            foreach (var parameter in missingRequiredParamters )
            {
                string valueStr;
                if (parameter.DataType == typeof(SecureString))
                {
                    valueStr = ConsoleHelper.GetPassword(parameter.Name);                    
                }
                else
                {
                    Console.Write("{0}: ", parameter.Name);
                    valueStr = Console.ReadLine();
                }
                
                object value = parseValue(valueStr, parameter.DataType);

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
                throw new CommandParserException("Not all required parameters could be set (have you specified all of them?)");
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
            else if(toType == typeof(SecureString))
            {
                SecureString secStr = value.ToSecureString();
                return secStr;
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
                    paramInfo.DataType = property.PropertyType;
                    paramInfo.SetMethod = property.GetSetMethod();
                    result.Add(paramInfo);
                }
            }

            return result;
        }

    }


    

    
}
