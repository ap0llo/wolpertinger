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
using Nerdcave.Common;
using System.Reflection;
using Wolpertinger.Core;
using Nerdcave.Common.Extensions;
using System.IO;
using Slf;
using System.Threading.Tasks;
using System.Threading;
using System.Security;
using Nerdcave.Common.Xml;
using Wolpertinger.FileShareCommon;
using System.Diagnostics;
using CommandLineParser.CommandParser;
using CommandLineParser.Interfaces;

namespace Wolpertinger.Manager.CLI
{
	class Program
	{
		internal static IConnectionManager connectionManager;

		private static object outputLock = new object();
		private static bool waiting;

		static ICommandParser<CommandContext> commandParser;
		static CommandContext context = new CommandContext();

		public static void Main(string[] args)
		{

			Console.WindowWidth = 120;
			Console.WindowHeight = 50;
			printLogo();
			Console.Title = "Wolpertinger Manager";
			Console.WriteLine();
			ConsoleHelper.WriteLine(ConsoleColor.Cyan, " {0}", FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion);
			Console.WriteLine();

			//Set up XmlSerializer
			XmlSerializer.RegisterType(typeof(ClientInfo),"clientInfo");
			XmlSerializer.RegisterType(typeof(DirectoryObject),"directoryObject");
			XmlSerializer.RegisterType(typeof(FileObject),"fileObject");
			XmlSerializer.RegisterType(typeof(Permission),"permission");
			XmlSerializer.RegisterType(typeof(MountInfo), "mountInfo");
			XmlSerializer.RegisterType(typeof(SnapshotInfo), "snapshotInfo");
			XmlSerializer.RegisterType(typeof(DirectoryObjectDiff), "directoryObjectDiff");
			XmlSerializer.RegisterType(typeof(RemoteMethodCall),"remoteMethodCall");
			XmlSerializer.RegisterType(typeof(RemoteMethodResponse),"remoteMethodResponse");
			XmlSerializer.RegisterType(typeof(RemoteError), "remoteError");


			DefaultComponentFactory.RegisterComponentAssembly(Assembly.GetAssembly(typeof(AuthenticationComponent)));
			DefaultComponentFactory.RegisterComponentAssembly(Assembly.GetExecutingAssembly());


			string profileFolderArg = args.Any(x => x.ToLower().StartsWith("profilefolder=")) ? args.First(x => x.ToLower().StartsWith("profilefolder=")): null;
			profileFolderArg = (profileFolderArg == null) ? null : profileFolderArg.Replace("profilefolder=", "");
			profileFolderArg = (Directory.Exists(profileFolderArg)) ? profileFolderArg : null;

			string folder = profileFolderArg.IsNullOrEmpty() ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Wolpertinger", "Manager") : profileFolderArg;

			if (!Directory.Exists(folder))
			{
				Directory.CreateDirectory(folder);
			}

			connectionManager = new DefaultConnectionManager();
			//connectionManager.ComponentFactory = new DefaultComponentFactory();
			connectionManager.AcceptIncomingConnections = false;
			connectionManager.LoadSettings(folder);
			
			
			//connectionManager.Connect();
			XmppLogger.ConnectionManager = connectionManager;


			commandParser = new CommandParser<CommandContext>(context);
			context.ConnectionManager = connectionManager;
			
			commandParser.LoadCommandsAndParsersFromAssembly(Assembly.GetAssembly(typeof(CommandParser<CommandContext>)));
			commandParser.LoadCommandsAndParsersFromAssembly(Assembly.GetExecutingAssembly());
			commandParser.SetParser(typeof(IClientConnection), new ClientConnectionParser(context)); 



			//Console.WriteLine("Account: {0}@{1}", connectionManager.XmppUsername, connectionManager.XmppServer);
			Console.WriteLine();

			//ILogger consoleLogger = new Wolpertinger.Core.ConsoleLogger();
			//LoggerService.SetLogger(consoleLogger);

			waitForCommand();

		}

		private static void waitForCommand()
		{
			while (true)
			{
				string promptText = context.ActiveConnection == null ? "" : context.ActiveConnection.Target;
				Console.Write(promptText + "> ");

				lock (outputLock)
				{
					waiting = true;
				}
				string input = Console.ReadLine();

				if (!input.IsNullOrEmpty())
				{
					lock (outputLock)
					{
						waiting = false;
					}

					try
					{
						var command = commandParser.GetCommand(input);
						command.Execute();
					}
					catch (AggregateException ex)
					{
						handleException(ex);
					}
					catch (TimeoutException ex)
					{
						ErrorLine("Connection timed out");
					}
					catch (CommandExecutionException ex)
					{
						ErrorLine(ex.Message);
					}
					catch (CommandParserException ex)
					{
						ErrorLine(ex.Message);
					}
					catch (RemoteErrorException ex)
					{
						ErrorLine(ex.Error.ErrorCode.ToString());
					}


				


				}
			}
		}
	 

		#region Helpers

		private static void printLogo()
		{
			ConsoleColor color = ConsoleColor.Cyan;

			int spacescount = (Console.WindowWidth - 61) / 2;

			ConsoleHelper.WriteSpaces(spacescount);
			ConsoleHelper.Write(color, @"               _                 _   _                       ");
			Console.Write("\n");
			ConsoleHelper.WriteSpaces(spacescount);
			ConsoleHelper.Write(color, @"__      _____ | |_ __   ___ _ __| |_(_)_ __   __ _  ___ _ __ ");
			Console.Write("\n");
			ConsoleHelper.WriteSpaces(spacescount);
			ConsoleHelper.Write(color, @"\ \ /\ / / _ \| | '_ \ / _ \ '__| __| | '_ \ / _` |/ _ \ '__|");
			Console.Write("\n");
			ConsoleHelper.WriteSpaces(spacescount);
			ConsoleHelper.Write(color, @" \ V  V / (_) | | |_) |  __/ |  | |_| | | | | (_| |  __/ |   ");
			Console.Write("\n");
			ConsoleHelper.WriteSpaces(spacescount);
			ConsoleHelper.Write(color, @"  \_/\_/ \___/|_| .__/ \___|_|   \__|_|_| |_|\__, |\___|_|   ");
			Console.Write("\n");
			ConsoleHelper.WriteSpaces(spacescount);
			ConsoleHelper.Write(color, @"                |_|                          |___/           ");
			Console.Write("\n");

		}

		private static void writeLine(string text, ConsoleColor color)
		{
			lock (outputLock)
			{
				if (waiting)
				{
					ConsoleHelper.ClearLine();
				}
				
				ConsoleHelper.WriteLine(color, text);
			}
		}

		private static void handleException(Exception exception)
		{
			if (exception is AggregateException)
			{
				var ex = exception as AggregateException;
				ex.Flatten();
				foreach (var innerException in ex.InnerExceptions)
				{
					handleException(innerException);
				}
			}
			else if (exception is TimeoutException)
			{
			}
			else if (exception is CommandExecutionException || 
					exception is CommandParserException)
			{
				ErrorLine(exception.Message);
			}
			else if (exception is RemoteErrorException)
			{
				var ex = exception as RemoteErrorException;
				ErrorLine(ex.Error.ErrorCode.ToString());
			}
			else
			{
				throw exception;
			}
		}


		#endregion Helpers


		#region Output

		public static void OutputLine(string line)
		{
			writeLine(line, ConsoleColor.Yellow);
		}

		public static void ErrorLine(string line)
		{
			writeLine(line, ConsoleColor.Red);
		}

		public static void StatusLine(string line)
		{
			writeLine(line, ConsoleColor.White);
		}

		#endregion
	}
}
