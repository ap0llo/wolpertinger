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
using System.Security;
using Nerdcave.Common;
using Nerdcave.Common.Extensions;
using CommandLineParser.Attributes;
using CommandLineParser.Info;
using CommandLineParser.ParameterParsers;
using CommandLineParser.Interfaces;

namespace CommandLineParser.CommandParser
{
	public class CommandParser<T> : ICommandParser<T> where T: ICommandContext<T>
	{
		#region Fields

		//The CommandContext in which the commands will be executed
		T context;
		//The commands known to the CommandParser
		List<CommandInfo> commands = new List<CommandInfo>();
		//The parsers known to the command-parser
		Dictionary<Type, IParameterParser> parameterParsers = new Dictionary<Type, IParameterParser>();

		#endregion

		/// <summary>
		/// The list of commands known to the CommandParser
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
		 
			ParameterArgumentMapping.ParameterParsers = this.parameterParsers;
		}

		/// <summary>
		/// Scans the specified assembly for command and parameter parsers and adds them to the CommandParser'S list of commands/parsers
		/// </summary>
		/// <param name="assembly">The assembly to scan</param>
		public void LoadCommandsAndParsersFromAssembly(System.Reflection.Assembly assembly)
		{
			foreach (var type in assembly.GetTypes())
			{
				if (type.IsSubclassOf(typeof(CommandBase<T>))
					|| typeof(IParameterParser).IsAssignableFrom(type))
				{
					//check if instances of the type can be created
					if (!type.CanCreateInstance())
					{
						continue;
					}
				}


				//check if type is derived from CommandBase
				if (type.IsSubclassOf(typeof(CommandBase<T>)))
				{
					commands.Add(loadCommandInfo(type));
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
		/// Parses the supplied command string, creates an instance of the appropriate command and sets its parameters based on the parsed input
		/// </summary>
		/// <param name="input">The command string to parse</param>
		/// <returns>
		/// Returns a new instance of a class derived from CommandBase with all the parameters set.
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

			//array that stores a bool for each element of args that indicates whether that parameter is done
			bool[] processedArgs = new bool[args.Count];     
			processedArgs[0] = true;    
			//set properties


			List<ParameterArgumentMapping> parameterArgumentMappings = new List<ParameterArgumentMapping>();

			foreach (var set in commandInfo.ParameterSets)
			{
				ParameterArgumentMapping mapping = null;

				try
				{
					mapping = new ParameterArgumentMapping(args.ToArray(), processedArgs, set);
				}
				catch (CommandParserException)
				{
					continue;
				}

				parameterArgumentMappings.Add(mapping);
			}



			int count = parameterArgumentMappings.Count();

			if (count == 0)
			{
				throw new CommandParserException("No ParameterSet found that matches the given parameters");
			}
			else if (count == 1)
			{
				//verify the parameter set can be applied
				if (parameterArgumentMappings.First().ProcessedArgs.Any(x => !x))
				{
					throw new CommandParserException("Not all arguments were processed");
				}

				applyParameters(parameterArgumentMappings.First(), commandInstance);
			}
			else
			{
				//try to rule out parameter sets
				parameterArgumentMappings = parameterArgumentMappings.Where(x => !x.ProcessedArgs.Any(y => !y)).ToList();              

				if (parameterArgumentMappings.Count() == 1)
				{
					applyParameters(parameterArgumentMappings.First(), commandInstance);
				}
				else
				{
					var unsetRequierdParametersCounts = parameterArgumentMappings.Select(m => m.ParameterSet.Parameters.Count(p => !p.IsOptional && !m.Mapping.ContainsKey(p)));
					var min = unsetRequierdParametersCounts.Min();

					parameterArgumentMappings = parameterArgumentMappings.Where(m => m.ParameterSet.Parameters.Count(p => !p.IsOptional && !m.Mapping.ContainsKey(p)) == min).ToList();

					if (parameterArgumentMappings.Count() == 1)
					{
						applyParameters(parameterArgumentMappings.First(), commandInstance);
					}
					else
					{

						var unsetParametersCount = parameterArgumentMappings.Select(m => m.ParameterSet.Parameters.Count(p=> !m.Mapping.ContainsKey(p)));
						var min2 = unsetParametersCount.Min();
						parameterArgumentMappings = parameterArgumentMappings.Where(m => m.ParameterSet.Parameters.Count(p=> !m.Mapping.ContainsKey(p)) == min).ToList();

						if (parameterArgumentMappings.Count() == 1)
						{
							applyParameters(parameterArgumentMappings.First(), commandInstance);
						}
						else
						{
							throw new CommandParserException("Could not determine the correct parameter-set to apply. Try using named parameters or specifying more parameters");
						}
					}
				}

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





		private void applyParameters(ParameterArgumentMapping mapping, CommandBase<T> commandInstance)
		{
			foreach (var item in mapping.Mapping)
			{
				var value = parseValue(item.Value, item.Key.DataType, true);
				item.Key.SetMethod.Invoke(commandInstance, new object[] { value });
			}

            commandInstance.ParameterSetName = mapping.ParameterSet.Name;


			//get values for unset required parameters 
			var unsetParameters = mapping.ParameterSet.Parameters
											.Where(p => !p.IsOptional)
											.Where(p => !mapping.Mapping.ContainsKey(p))
											.ToList();

			foreach (var parameter in unsetParameters)
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

				//try to parse the supplied string as value for the parameter                
				if (!ParameterArgumentMapping.CanParse(valueStr, parameter.DataType))
				{
					throw new CommandParserException("Invalid value");
				}
				else
				{
					parameter.SetMethod.Invoke(commandInstance, new object[] { parseValue(valueStr, parameter.DataType) });                    
				}
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

		private CommandInfo loadCommandInfo(Type type)
		{
			var attributes = type.GetCustomAttributes(typeof(CommandAttribute), false);

			if (!attributes.Any())
			{
				return null;
			}

			CommandInfo result = new CommandInfo(attributes.First() as CommandAttribute) { Type = type };
			


			/*
			 * load parameters
			 */
			var properties = type.GetProperties().Where(x => x.GetCustomAttributes(typeof(ParameterAttribute), true).Any());

			Dictionary<string, ParameterSet> parameterSets = new Dictionary<string, ParameterSet>();
			HashSet<ParameterInfo> parametersWithoutSet = new HashSet<ParameterInfo>();
			foreach (var parameter in properties)
			{
				var parameterAttributes = parameter.GetCustomAttributes(typeof(ParameterAttribute), true).Cast<ParameterAttribute>();

				foreach (var attribute in parameterAttributes)
				{
					var parameterInfo = new ParameterInfo(attribute) { SetMethod = parameter.GetSetMethod(), DataType = parameter.PropertyType };
					if (String.IsNullOrEmpty(attribute.ParameterSet))
					{
						parametersWithoutSet.Add(parameterInfo);
					}
					else
					{
						var key = attribute.ParameterSet.ToLower();
						if (!parameterSets.ContainsKey(key))
						{
							var set = new ParameterSet() { Name = attribute.ParameterSet};
							parameterSets.Add(key, set);
						}
						parameterSets[key].AddParameter(parameterInfo);
					}
				}
			}


			if (parameterSets.Any())
			{
				result.ParameterSets = parameterSets.Values.ToList();
			}
			else
			{
				result.ParameterSets = new List<ParameterSet>() {new ParameterSet("", parametersWithoutSet)};
			}			
			

			return result;
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
		private List<ParameterInfo> loadParameters(Type commandType)
		{
			var result = new List<ParameterInfo>();

			//iterate over all of the command's properties
			foreach (var property in commandType.GetProperties())
			{
				var attributes = property.GetCustomAttributes(typeof(ParameterAttribute), true);

				if (attributes.Any())
				{
					var paramInfo = new ParameterInfo(attributes.First() as ParameterAttribute);
					paramInfo.DataType = property.PropertyType;
					paramInfo.SetMethod = property.GetSetMethod();
					result.Add(paramInfo);
				}
			}

			//check for duplicate parameters
			if (result.GroupBy(x => x.Name.ToLower()).Count() != result.Count)
			{
				throw new CommandParserException("Could not load parameters because of duplicate parameter names");
			}

			return result;
		}





		private class ParameterArgumentMapping
		{
			public static Dictionary<Type, IParameterParser> ParameterParsers;
			
			private Dictionary<ParameterInfo, string> _parameterArgumentMapping = new Dictionary<ParameterInfo,string>();

		
			public string[] Args { get; private set; }

			public bool[] ProcessedArgs { get; private set; }


			public ParameterSet ParameterSet { get; private set; }

			public Dictionary<ParameterInfo, string> Mapping
			{
				get 
				{
					return _parameterArgumentMapping;
				}
			}



			public ParameterArgumentMapping(string[] args, bool[] processedArgs, 
				ParameterSet parameterSet)
			{                
				this.Args = args;

				//copy the processedArgs array
				this.ProcessedArgs = processedArgs.ToArray<bool>();
				this.ParameterSet = parameterSet;

				assignNamedParameters();
				assignPositionalParameters();
			}


			private void assignNamedParameters()
			{                 
				for (int i = 0; i < Args.Length - 1; i++)
				{
					if(ProcessedArgs[i])
					{
						continue;
					}

					if (Args[i].StartsWith("-") && Args[i].Length > 1)
					{
						var query = this.ParameterSet.Parameters.Where(x =>
										!_parameterArgumentMapping.ContainsKey(x) &&
										x.Name.ToLower() == Args[i].Substring(1).ToLower() &&
										CanParse(Args[i + 1], x.DataType));
						if (query.Any())
						{
							_parameterArgumentMapping.Add(query.First(), Args[i + 1]);

							ProcessedArgs[i] = true;
							ProcessedArgs[i + 1] = true;

							i += 1;
						}
					}
				}
			}

			private void assignPositionalParameters()
			{
				var positionalParameters = this.ParameterSet.Parameters.Where(x => !_parameterArgumentMapping.ContainsKey(x) && x.Position > 0)
											   .OrderBy(x => x.Position)
											   .ToList();
				
				int parameterIndex = 0;
				for (int i = 0; i < Args.Length && parameterIndex < positionalParameters.Count; i++)
				{
					if(ProcessedArgs[i])
					{
						continue;
					}

					if(CanParse(Args[i], positionalParameters[parameterIndex].DataType))
					{
						_parameterArgumentMapping.Add(positionalParameters[parameterIndex], Args[i]);
						ProcessedArgs[i] = true;

						parameterIndex++;
					}
					else if(!positionalParameters[parameterIndex].IsOptional)
					{
						break;                        
					}                    
				}
			}

			public static bool CanParse(string value, Type toType)
			{
				if (ParameterParsers.ContainsKey(toType))
				{
					var parser = ParameterParsers[toType];

					return parser.CanParse(value);                
				}

				throw new CommandParserException("No parser found for type {0}", toType.Name);
			}

		}


	}
}
