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
using CommandLineParser.Attributes;
using CommandLineParser.Info;
using CommandLineParser.ParameterParsers;
using CommandLineParser.Interfaces;

namespace CommandLineParser.CommandParser
{
	public class CommandParser<T> : ICommandParser<T> where T: ICommandContext<T>
	{
		#region Fields

		//The CommandContext in whcih the commands will be executed
		T context;
		//The commands knwon to the CommandParser
		List<CommandInfo> commands = new List<CommandInfo>();
		//The parsers known to the commandparser
		Dictionary<Type, IParameterParser> parameterParsers = new Dictionary<Type, IParameterParser>();

		#endregion

		/// <summary>
		/// The list of commands knwon to the CommandParser
		/// </summary>
		public IEnumerable<CommandInfo> KnownCommands
		{
			get
			{
				return commands;
			}
		}


		/// <summary>
		/// Initializes a new instance of CommandParser
		/// </summary>
		/// <param name="context">The <see cref="CommandContext"/> to use for executing commands</param>
		public CommandParser(T context)
		{
			this.context = context;
			context.CommandParser = this;            
		}


		/// <summary>
		/// Scans the specified assembly for command and parameter parsers and adds them to the CommandParser'S list of commands/parsers
		/// </summary>
		/// <param name="assembly">The assembly to scan</param>
		public void LoadCommandsAndParsersFromAssembly(Assembly assembly)
		{
			foreach (var type in assembly.GetTypes())
			{
				if (type.IsSubclassOf(typeof(CommandBase<T>))
					|| typeof(IParameterParser).IsAssignableFrom(type))
				{
					//check if instances of the type can be created
					if (!canCreateInstance(type))
					{
						continue;
					}
				}


				//check if type is derived from CommandBase
				if (type.IsSubclassOf(typeof(CommandBase<T>)))
				{
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

				if (typeof(IParameterParser).IsAssignableFrom(type))
				{
					var attributes = type.GetCustomAttributes(typeof(ParameterParserAttribute), false);
					if (attributes == null || !attributes.Any())
					{
						continue;
					}

					var parameterType = (attributes.First() as ParameterParserAttribute).ParameterType;


					IParameterParser instance = Activator.CreateInstance(type) as IParameterParser;					
					parameterParsers.Add(parameterType, instance);

				}
			}
		}

		/// <summary>
		/// Parses the supplied command string, creates an instance of the apropriate command and sets its parameters based on the parsed input
		/// </summary>
		/// <param name="input">The command string to parse</param>
		/// <returns>
		/// Returns a new instance of a class derived from CommandBase with all the paramters set.
		/// On Error <see cref="CommandParserException"/> is thrown
		/// </returns>
		public CommandBase<T> GetCommand(string input)
		{
			var args = input.SpaceSplitString();

			if (!args.Any())
			{
				throw new CommandParserException("Empty input-string");
			}


			var commandInfo =  getCommandInfo(args);
			var commandInstance = (CommandBase<T>)Activator.CreateInstance(commandInfo.Type);
			commandInstance.Context = this.context;


			//set properties

			HashSet<CommandParameterInfo> setParameters = new HashSet<CommandParameterInfo>();      //List of all parameters that have already been set 
			bool[] processedArgs = new bool[args.Count];                                            //array that stores a bool for each element of args that indicates whether that paramter is done

			processedArgs[0] = true;        //first paramter has alredy been processed (for finding the apropriate command)


			//first, set named parameters
			setNamedParameters(args, commandInfo, commandInstance, processedArgs, setParameters);


			//check if there are any args left to process
			if (processedArgs.Any(x => !x))
			{
				setPositionalParameters(args, commandInfo, commandInstance, processedArgs, setParameters);
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

			//iterate over all missing paramters
			foreach (var parameter in missingRequiredParamters )
			{
				//request input
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
				
				//try to parse the supplied string as value for the paramter                
				if (!canParse(valueStr, parameter.DataType))
				{
					throw new CommandParserException("Invalid value");
				}
				else
				{
					parameter.SetMethod.Invoke(commandInstance, new object[] { parseValue(valueStr, parameter.DataType) });
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



		public void SetParser(Type type, IParameterParser parser)
		{
			if (parameterParsers.ContainsKey(type))
			{
				parameterParsers[type] = parser;
			}
			else
			{
				parameterParsers.Add(type, parser);
			}
		}


		/// <summary>
		/// Helper method for GetCommand(). Parses the supplied command args and tries to find the right command
		/// </summary>
		private CommandInfo getCommandInfo(List<string> args)
		{
			var commandParts = args.First().Split(new char[] { '-', '.' });

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

			return commandInfo;
		}

		/// <summary>
		/// Helper method for GetCommand(). Searches all the named parameters found in args and sets the apropriate properties of commandInstance
		/// </summary>
		private void setNamedParameters(List<string> args, CommandInfo commandInfo, CommandBase<T> commandInstance, 
			bool[] processedArgs, HashSet<CommandParameterInfo> setParameters)
		{
			//first get named paramters
			for (int i = 1; i < args.Count; i++)
			{
				string arg = args[i];
				//check if current arg is the name of a parameter
				if (arg.StartsWith("-"))
				{
					var paramterNameQuery = commandInfo.Parameters.Where(x => "-" + x.Name.ToLower() == arg.ToLower());
					var count = paramterNameQuery.Count();

					if (count == 1)
					{
						if (i + 1 < args.Count)
						{
							var parameterInfo = paramterNameQuery.First();
							string valueStr = args[++i];
							object value = parseValue(valueStr, parameterInfo.DataType);
							parameterInfo.SetMethod.Invoke(commandInstance, new object[] { value });

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
		}

		/// <summary>
		/// Helper method for setPositionalParameters(). Build a list of all possible parameter-combinatin for the paramters not already set
		/// </summary>
		private List<CommandParameterInfo>[] getPostionalParamterCombinations(CommandInfo commandInfo, HashSet<CommandParameterInfo> setParameters)
		{
			//build a list of all possible combinations of parameters
			var unsetParameters = commandInfo.Parameters.Where(x => !setParameters.Contains(x))
											 .Where(x => x.Position > 0)
											 .OrderBy(x => x.Position)
											 .ToArray<CommandParameterInfo>();

			var optionalParameterCount = unsetParameters.Count(x => x.IsOptional);
			List<CommandParameterInfo>[] possibleCombinations = new List<CommandParameterInfo>[(int)Math.Pow(2, optionalParameterCount)];


			if (optionalParameterCount == 0)
			{
				possibleCombinations[0] = unsetParameters.ToList();
			}
			else
			{

				for (int i = 0; i < possibleCombinations.Length; i++)
				{
					possibleCombinations[i] = new List<CommandParameterInfo>();

					int bitNumber = 0;
					for (int p = 0; p < unsetParameters.Length; p++)
					{
						if (!unsetParameters[p].IsOptional)
						{
							possibleCombinations[i].Add(unsetParameters[p]);
						}
						else
						{
							if (((i >> bitNumber) & 1) != 0)
							{
								possibleCombinations[i].Add(unsetParameters[p]);
							}
							bitNumber++;
						}
					}
				}
			}

			return possibleCombinations;

		}

		/// <summary>
		/// Helper method for GetCommand(). Tries to determine the right combination of paramters for the args that have not already been processed based on their positon
		/// </summary>
		private void setPositionalParameters(List<string> args, CommandInfo commandInfo, CommandBase<T> commandInstance, 
			bool[] processedArgs, HashSet<CommandParameterInfo> setParameters)
		{
			//first, build a list of all possible combinations
			var possibleCombinations = getPostionalParamterCombinations(commandInfo, setParameters);


			//we now have a list of all possible combinations of optional parameters that have not already been set
			//we now have to determine which possibility mathes our actual parmeters


			//first, remove all combinations that have a different parameter count
			var unprocessedArgsCount = processedArgs.Count(x => !x);
			possibleCombinations = possibleCombinations.Where(c => c.Count == unprocessedArgsCount).ToArray<List<CommandParameterInfo>>();


			var unprocessedArgs = args.Where(x => !processedArgs[args.IndexOf(x)]).ToArray<string>();


			//check every of the remaining possible combinations
			for (int i = 0; i < possibleCombinations.Length; i++)
			{
				for (int a = 0; a < unprocessedArgs.Length; a++)
				{
					if (!canParse(unprocessedArgs[a], possibleCombinations[i][a].DataType))
					{
						possibleCombinations[i] = null;
						break;
					}
				}
			}

			possibleCombinations = possibleCombinations.Where(x => x != null).ToArray<List<CommandParameterInfo>>();


			if (possibleCombinations.Length == 0)
			{
				throw new CommandParserException("No applicabale combination of positional parameters could be found");
			}
			else if (possibleCombinations.Length == 1)
			{
				var combination = possibleCombinations.First();
				int lastSetIndex = 0;

				for (int a = 0; a < unprocessedArgs.Length; a++)
				{
					object value = parseValue(unprocessedArgs[a], combination[a].DataType, true);
					combination[a].SetMethod.Invoke(commandInstance, new object[] { value });

					//update set parameters and processed args lists
					int index = args.IndexOf(unprocessedArgs[a], lastSetIndex);
					lastSetIndex = index;
					processedArgs[index] = true;
					setParameters.Add(combination[a]);
				}
			}
			if (possibleCombinations.Length > 1)
			{
				throw new CommandParserException("Could not determine parameters based on their position, there are multiple possible combinations");
			}

		}

		/// <summary>
		/// Determines whether a new instace of the specified type can be created
		/// </summary>
		private bool canCreateInstance(Type type)
		{
			try
			{
				object test = Activator.CreateInstance(type);
			}
			catch (MissingMethodException)
			{
				return false;
			}
			return true;
		}


		/// <summary>
		/// Checks whether the specified string can be parsed into a object of the specified type
		/// </summary>
		/// <param name="value">The string to be parsed</param>
		/// <param name="toType">The type of the object to parse the string into</param>
		/// <returns>Returns true if the string can be parsed, otherwise returns false</returns>
		private bool canParse(string value, Type toType)
		{
			if (parameterParsers.ContainsKey(toType))
			{
				var parser = parameterParsers[toType];

				return parser.CanParse(value);                
			}

			throw new CommandParserException("No parser found for type {0}", toType.Name);
		}

		/// <summary>
		/// Parses the specified string into a instance of the specified type
		/// </summary>
		/// <param name="value">The value to be parsed</param>
		/// <param name="toType">The target type to parse the string as</param>
		/// <param name="noCheck">When set to true, the parser's CanParse() method will not be called before calling Parse()
		/// Only set this to true if you called CanParse() manually</param>
		private object parseValue(string value, Type toType, bool noCheck = false)
		{
			if (parameterParsers.ContainsKey(toType))
			{
				var parser = parameterParsers[toType];


				if (noCheck || parser.CanParse(value))
				{
					return parser.Parse(value);

				}
				else
				{
					throw new CommandParserException("Cannot parse '{0}' as {1}", value, toType.Name);
				}
				
			}

			throw new CommandParserException("No parser found for type {0}", toType.Name);
	  
		}

		/// <summary>
		/// Gets the type of the command associated with the given verb, noun and module
		/// </summary>
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
			
		/// <summary>
		/// Helper method for LoadCommandsAndParsersFromAssembly(): Gets all the parameters of the given command 
		/// </summary>
		/// <param name="commandType">The command to load parameters from</param>
		/// <returns>Returns a list of all of the command's parameters</returns>
		private List<CommandParameterInfo> loadParamters(Type commandType)
		{
			var result = new List<CommandParameterInfo>();

			//iterate over all of the command's properties
			foreach (var property in commandType.GetProperties())
			{
				var attributes = property.GetCustomAttributes(typeof(ParameterAttribute), true);

				if (attributes.Any())
				{
					var paramInfo = new CommandParameterInfo(attributes.First() as ParameterAttribute);
					paramInfo.DataType = property.PropertyType;
					paramInfo.SetMethod = property.GetSetMethod();
					result.Add(paramInfo);
				}
			}

			//check for duplicate paramters
			if (result.GroupBy(x => x.Name.ToLower()).Count() != result.Count)
			{
				throw new CommandParserException("Could not load paramters because of duplicate parameter names");
			}

			return result;
		}

	}
}
