/*

Licensed under the new BSD-License
 
Copyright (c) 2011-2013, Andreas Grünwald 
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

    Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
    Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other 
    materials provided with the distribution.
    Neither the name of the Wolpertinger project nor the names of its contributors may be used to endorse or promote products derived from this software without 
    specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS 
BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE 
GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, 
STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

using Nerdcave.Common;
using Nerdcave.Common.Extensions;
using Nerdcave.Common.Xml;
using Slf;
using System;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using Wolpertinger.Core;
using Wolpertinger.FileShareCommon;

namespace Wolpertinger.Fileserver
{
    class Program
    {

        static ILogger logger;
        static string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Wolpertinger", "Fileserver");
        static KeyValueStore settingsFile = new KeyValueStore(Path.Combine(folder, "ServiceSettings.xml"));

        public static string DatabaseFolder = Path.Combine(folder, "Database");

        static IConnectionManager connectionManager;

        static void Main(string[] args)
        {
            //Display general Info
            printLogo();

            Console.Title = "Wolpertinger Fileserver";
            Console.WriteLine();
            ConsoleHelper.WriteLine(ConsoleColor.Red, " Wolpertinger.FileServer {0}", Assembly.GetExecutingAssembly().GetName().Version.ToString());
            ConsoleHelper.WriteLine(ConsoleColor.Red, " Wolpertinger.Core       {0}", Assembly.GetAssembly(typeof(DefaultConnectionManager)).GetName().Version.ToString());
            Console.WriteLine();

            //Set up XmlSerializer
            XmlSerializer.RegisterType(typeof(ClientInfo), "clientInfo");
            XmlSerializer.RegisterType(typeof(DirectoryObject), "directoryObject");
            XmlSerializer.RegisterType(typeof(FileObject), "fileObject");
            XmlSerializer.RegisterType(typeof(Permission), "permission");
            XmlSerializer.RegisterType(typeof(MountInfo), "mountInfo");
            XmlSerializer.RegisterType(typeof(SnapshotInfo), "snapshotInfo");
            XmlSerializer.RegisterType(typeof(DirectoryObjectDiff), "directoryObjectDiff");
            XmlSerializer.RegisterType(typeof(RemoteMethodCall), "remoteMethodCall");
            XmlSerializer.RegisterType(typeof(RemoteMethodResponse), "remoteMethodResponse");
            XmlSerializer.RegisterType(typeof(RemoteError), "remoteError");


            //Set up logger            
            LoggerService.SetLogger(new CompositeLogger(new Wolpertinger.Core.ConsoleLogger(), new XmppLogger()));
            logger = LoggerService.GetLogger("Wolpertinger.Fileserver");


            FileObject.HashingService = HashingService.GetHashingService();

            AuthenticationComponent foo = new AuthenticationComponent();


            if (!Directory.Exists(Path.GetDirectoryName(folder)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(folder));
            }

            //Set up AppData directory
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            //Set up databasefolder
            if (!Directory.Exists(DatabaseFolder))
            {
                Directory.CreateDirectory(DatabaseFolder);
            }

   
            //Initalize ConnectionManager
            //TODO manager.AddProfile(Profile.FileServer);
            //manager.AddComponent(typeof(ClientInfoProvider), typeof(XmppLoggingConfigurator), typeof(FileShare));

            connectionManager = new DefaultConnectionManager();
            //connectionManager.ComponentFactory = new DefaultComponentFactory();
            connectionManager.LoadSettings(folder);            
            //connectionManager.Connect();
            connectionManager.AcceptIncomingConnections = true;
            XmppLogger.ConnectionManager = connectionManager;
            XmppLogger.LoadSettings();

            //Load setting specific to this role
            connectionManager.WolpertingerUsername = settingsFile.GetItem<string>("AdminUsername");
            connectionManager.WolpertingerPassword = settingsFile.GetItem<string>("AdminPassword").ToSecureString();


            FileShareServerComponent.Init();

            //Console.WriteLine("Account: " + connectionManager.XmppUsername + "@" + connectionManager.XmppServer);

            Console.ReadLine();
        }



        private static void printLogo()
        {


            ConsoleColor color = ConsoleColor.Red;

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

        internal static void createDirectorySymbolicLink(string link, string target)
        {
            string cmd = "create|" + link + "|" + target + "|";

            using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "Wolpertinger.SymlinkHelper", PipeDirection.Out))
            {
                // Connect to the pipe or wait until the pipe is available.
                pipeClient.Connect();

                using (StreamWriter sw = new StreamWriter(pipeClient))
                {
                    sw.WriteLine(cmd);
                }
            }
        }

    }
}
