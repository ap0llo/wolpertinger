/*

Licensed under the new BSD-License
 
Copyright (c) 2011-2013, Andreas Grünwald 
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

	Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
	Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
	Neither the name of the Wolpertinger project nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

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

namespace Wolpertinger.Manager.CLI
{
	class Program
	{
		internal static IConnectionManager connectionManager;

		internal static Dictionary<string, Session> sessions = new Dictionary<string,Session>();
		internal static Context activeContext;
		internal static Context mainContext;


		private static object outputLock = new object();
		private static bool waiting;


		public static void Main(string[] args)
		{
			Console.WindowWidth = 120;
			Console.WindowHeight = 50;
			printLogo();
			Console.Title = "Wolpertinger Manager";
			Console.WriteLine();
			ConsoleHelper.WriteLine(ConsoleColor.Cyan, " Wolpertinger Manager {0}", Assembly.GetExecutingAssembly().GetName().Version.ToString());
			ConsoleHelper.WriteLine(ConsoleColor.Cyan, " Wolpertinger.Core    {0}", Assembly.GetAssembly(typeof(DefaultConnectionManager)).GetName().Version.ToString());
			Console.WriteLine();


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


            //Console.WriteLine("Account: {0}@{1}", connectionManager.XmppUsername, connectionManager.XmppServer);
			Console.WriteLine();

			ILogger consoleLogger = new Wolpertinger.Core.ConsoleLogger();
			LoggerService.SetLogger(consoleLogger);


			mainContext = new MainContext();
			activeContext = mainContext;

			waitForCommand();

		}

		private static void waitForCommand()
		{
			while (true)
			{
				
				Console.Write("{0}> ", activeContext.Name);

				lock (outputLock)
				{
					waiting = true;
				}
				string input = Console.ReadLine();

				lock (outputLock)
				{
					waiting = false;
				}

				IEnumerable<string> cmds;
				try
				{
					cmds = input.SpaceSplitString();
				}
				catch (FormatException)
				{
					UnknownCommand();
					continue;
				}


				if (cmds.Any())
					activeContext.ParseCommands(cmds);
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
				if(waiting)
					ConsoleHelper.ClearLine();
				
				ConsoleHelper.WriteLine(color, text);

				if (waiting)
				{
					Console.Write("\n");                    
					Console.Write("{0}> ", activeContext.Name);
				}

			}
		}	

		


		internal static Session getSession(string str)
		{
			str = str.ToLower();
			if (str.StartsWith("#"))
			{
				int value = 0;
				if (int.TryParse(str.RemoveFirstChar(), out value) && value >= 0 && value < sessions.Count)
				{
					return sessions.Values.ElementAt(value);
				}

			}
			else if(sessions.ContainsKey(str))
			{
				return sessions[str];
			}

			return null;
		}



		private static void UnknownCommand()
		{
			ErrorLine("Unknown Command or Invalid Parameters");
		}

		public static void UnknownCommand(Context context)
		{
			ErrorLine(context, "Unknown Command or Invalid Parameters");
		}



		#endregion Helpers


		#region Output 

		private static void OutputLine(string line)
		{
			writeLine(line, ConsoleColor.Yellow);
		}

		private static void OutputLine(string format, params object[] args)
		{
			OutputLine(String.Format(format, args));
		}

		public static void OutputLine(Context caller, string line)
		{
			if (activeContext == caller)
				OutputLine(line);
			else
				OutputLine(String.Format("{0}: {1}", caller.Name, line));
		}

		public static void OutputLine(Context caller, string format, params object[] args)
		{
			OutputLine(caller, String.Format(format, args));
		}


		private static void ErrorLine(string line)
		{
			writeLine(line, ConsoleColor.Red);
		}

		public static void ErrorLine(Context caller, string error)
		{
			if (activeContext == caller)
				ErrorLine(error);
			else
				ErrorLine(String.Format("{0}: {1}", caller.Name, error));
		}

		public static void ErrorLine(Context caller, string format, params object[] args)
		{
			ErrorLine(caller, String.Format(format, args));
		}


		private static void StatusLine(string line)
		{
			writeLine(line, ConsoleColor.White);
		}

		public static void StatusLine(Context caller,string line)
		{
			if (activeContext == caller)
				StatusLine(line);
			else
				StatusLine(String.Format("{0}: {1}", caller.Name, line));
		}

		public static void StatusLine(Context caller, string format, params object[] args)
		{
			StatusLine(caller, String.Format(format, args));
		}

		#endregion
	}
}
