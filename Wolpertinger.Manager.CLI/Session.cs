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
using Wolpertinger.Core;
using Slf;
using System.Security;
using Nerdcave.Common;
using Nerdcave.Common.Extensions;
using Wolpertinger.FileShareCommon;

namespace Wolpertinger.Manager.CLI
{
    public class Session : Context
    {

        public string Target { get; private set; }

        IConnectionManager connectionManager;

        IClientConnection connection;
        private AuthenticationComponent authComponent;
        private ClientInfoClientComponent clientInfoComponent;
        private XmppLoggingConfiguratorComponent loggingConfigurator;
        private FileShareClientComponent fileShareComponent;

        ILogger logger = LoggerService.GetLogger("Session");



        public Session(string target, IConnectionManager connectionManager)
        {
            this.connectionManager = connectionManager;

            this.Target = target;
            this.Name = target;

            this.connection = connectionManager.GetClientConnection(target);
            if (connection == null)
            {
                connection = connectionManager.AddClientConnection(target);
                connection.ConnectionReset += connection_ConnectionReset;
                connection.ConnectionTimedOut += connection_ConnectionTimedOut;
                connection.RemoteErrorOccurred += connection_RemoteErrorOccurred;
            }
            //authComponent = (AuthenticationComponent)connection.GetClientComponent(ComponentNamesExtended.Authentication);
            //clientInfoComponent = (ClientInfoClientComponent)connection.GetClientComponent(ComponentNamesExtended.ClientInfoProvider);
            //loggingConfigurator = (XmppLoggingConfiguratorComponent)connection.GetClientComponent(ComponentNamesExtended.XmppLoggingConfigurator);
            //fileShareComponent = (FileShareClientComponent)connection.GetClientComponent(ComponentNamesExtended.FileShare);

            authComponent = new AuthenticationComponent() { ClientConnection = connection };
            clientInfoComponent = new ClientInfoClientComponent() { ClientConnection = connection };
            loggingConfigurator = new XmppLoggingConfiguratorComponent() { ClientConnection = connection };
            fileShareComponent = new FileShareClientComponent() { ClientConnection = connection };


            //CommandInfo info = new CommandInfo();

            commands.Add("get-status", new CommandInfo() { ParameterCount = 0, CommandMethod = getStatusCommand, CheckConnection = false});
            commands.Add("connect", new CommandInfo() { ParameterCount = 0, CommandMethod = connectCommand, CheckConnection = false });
            commands.Add("disconnect", new CommandInfo() { ParameterCount = 0, CommandMethod = disconnectCommand });
            commands.Add("userauth", new CommandInfo() { ParameterCount = 1, CommandMethod = userauthCommand });
            commands.Add("get-info", new CommandInfo() { ParameterCount = 0, CommandMethod = getInfoCommand });
            commands.Add("exit-session", new CommandInfo() { CheckConnection=false, ParameterCount = 0, CommandMethod = exitSessionCommand });


            commands.Add("logger.get-enabled", new CommandInfo() { ParameterCount = 0, CommandMethod = loggerGetEnabledCommand });
            commands.Add("logger.set-enabled", new CommandInfo() { ParameterCount = 1, CommandMethod = loggerSetEnabledCommand });
            commands.Add("logger.get-loglevel", new CommandInfo() { ParameterCount = 0, CommandMethod = loggerGetLogLevelCommand });
            commands.Add("logger.set-loglevel", new CommandInfo() { ParameterCount = 1, CommandMethod = loggerSetLogLevelCommand });
            commands.Add("logger.get-recipient", new CommandInfo() { ParameterCount = 0, CommandMethod = loggerGetRecipientCommand });
            commands.Add("logger.set-recipient", new CommandInfo() { ParameterCount = 1, CommandMethod = loggerSetRecipientCommand });
            commands.Add("logger.get-debuglogging", new CommandInfo() { ParameterCount = 0, CommandMethod = loggerGetDebugLoggingCommand });
            commands.Add("logger.set-debuglogging", new CommandInfo() { ParameterCount = 1, CommandMethod = loggerSetDebugLoggingCommand });
            commands.Add("logger.test-logging", new CommandInfo() { ParameterCount = 1, CommandMethod = loggerTestLoggingCommand });

            commands.Add("fileshare.add-directory", new CommandInfo() { ParameterCount = 2, CommandMethod = fileshareAddDirectoryCommand });
            commands.Add("fileshare.remove-directory", new CommandInfo() { ParameterCount = 1, CommandMethod = fileshareRemoveDirectoryCommand });
            commands.Add("fileshare.get-directory", new CommandInfo() { ParameterCount = 1, CommandMethod = fileshareGetDirectoryCommand });
            commands.Add("fileshare.get-file", new CommandInfo() { ParameterCount = 1, CommandMethod = fileshareGetFileCommand });
            commands.Add("fileshare.add-permission", new CommandInfo() { ParameterCount = -2, CommandMethod = fileshareAddPermissionCommand });
            commands.Add("fileshare.remove-permission", new CommandInfo() { ParameterCount = 1, CommandMethod = fileshareRemovePermissionCommand });
            commands.Add("fileshare.get-mounts", new CommandInfo() { ParameterCount = 0, CommandMethod = fileshareGetMountsCommand });
            commands.Add("fileshare.check-permission", new CommandInfo() { ParameterCount = 1, CommandMethod = fileshareCheckPermissionCommand });
            commands.Add("fileshare.get-permissions", new CommandInfo() { ParameterCount = 0, CommandMethod = fileshareGetPermissionsCommand });
            commands.Add("fileshare.get-rootdirectory", new CommandInfo() { ParameterCount = 0, CommandMethod = fileshareGetRootDirectoryCommand });
            commands.Add("fileshare.set-rootdirectory", new CommandInfo() { ParameterCount = 1, CommandMethod = fileshareSetRootDirectoryCommand });
        }





        protected void getStatusCommand(IEnumerable<string> cmds)
        {
            Program.StatusLine(this, connection.Connected ? String.Format("Connected, TrustLevel {0}, My TrsutLevel {1}", connection.TrustLevel, connection.MyTrustLevel) : "Not Connected");
        }

        protected void connectCommand(IEnumerable<string> cmds)
        {

            if (connection == null)
            {
                Program.ErrorLine(this, "Connection not Initialized");
                return;
            }

            Program.StatusLine(this, "Connecting to {0}", this.Target);

            try
            {
                if (authComponent.EstablishConnectionAsync().Result)
                {
                    Program.StatusLine(this, "Exchanging keys");

                    authComponent.KeyExchangeAsync().Wait();

                    Program.StatusLine(this, "Key exchange completed");
                    Program.StatusLine(this, "Initiating Cluster Authentication");

                    string token = authComponent.ClusterAuthGetTokenAsync().Result;

                    if (authComponent.ClusterAuthVerifyAsync(token).Result)
                    {
                        Program.StatusLine(this, "Verified my Cluster Membership");
                        if (authComponent.ClusterAuthRequestVerificationAsync().Result)
                        {
                            Program.StatusLine(this, "Verified Cluster Membership of target client");
                            
                            //we're finished
                            Program.StatusLine(this, "Successfully connected to target");

                        }
                        else
                        {
                            Program.StatusLine(this, "Cluster Authentication of target client failed");
                        }
                    }
                    else
                    {
                        Program.ErrorLine(this, "Cluster Authentication failed");
                    }

                }
            }
            catch (TimeoutException)
            {              
            }
        }

        internal void disconnectCommand(IEnumerable<string> cmds)
        {

            if(connection != null)
                connection.ResetConnection(true);

            if (connection != null)
            {
                connectionManager.RemoveClientConnection(connection.Target);
            }

        }

        protected void userauthCommand(IEnumerable<string> cmds)
        {
                     
            SecureString securePassword = ConsoleHelper.GetPassword().ToSecureString();

            string token = authComponent.UserAuthGetTokenAsync().Result;

            Program.StatusLine(this, authComponent.UserAuthVerifyAsync(cmds.First(), token, securePassword).Result
                                        ? "User Authentication successful"
                                        : "User Authentication failed");                
        }

        protected void getInfoCommand(IEnumerable<string> cmds)
        {
            try
            {
                Program.OutputLine(this, clientInfoComponent.GetClientInfoAsync().Result.ToString());
            }
            catch (TimeoutException)
            { }
        }

        protected void exitSessionCommand(IEnumerable<string> cmds)
        {
            Program.activeContext = Program.mainContext;
        }


        protected void loggerGetEnabledCommand(IEnumerable<string> cmds)
        {
            try
            {
                bool enabled = loggingConfigurator.GetEnableAsync().Result;
                Program.OutputLine(this, enabled.ToString());
            }
            catch (TimeoutException)
            {
            }
        }

        protected void loggerSetEnabledCommand(IEnumerable<string> cmds)
        {
            bool value;
            if (bool.TryParse(cmds.First(), out value))
                loggingConfigurator.SetEnableAsync(value);
            else
                Program.UnknownCommand(this);
        }

        protected void loggerGetLogLevelCommand(IEnumerable<string> cmds)
        {
            try
            {
                Program.OutputLine(this, loggingConfigurator.GetLoglevelAsync().Result.ToString());
            }
            catch (TimeoutException)
            {   }
        }

        protected void loggerSetLogLevelCommand(IEnumerable<string> cmds)
        {
            Core.LogLevel? lvl = getLogLevel(cmds.First());

            if (lvl != null)
                loggingConfigurator.SetLogLevelAsync(lvl.Value);
            else
                Program.UnknownCommand(this);
        }

        protected void loggerGetRecipientCommand(IEnumerable<string> cmds)
        {
            try
            {
                Program.OutputLine(this, loggingConfigurator.GetRecipientAsync().Result.ToString());
            }
            catch (TimeoutException)
            {   }
        }

        protected void loggerSetRecipientCommand(IEnumerable<string> cmds)
        {
            loggingConfigurator.SetRecipientAsync(cmds.First());
        }

        protected void loggerGetDebugLoggingCommand(IEnumerable<string> cmds)
        {
            try
            {
                Program.OutputLine(this, loggingConfigurator.GetEnableDebugLoggingAsync().Result.ToString());
            }
            catch (TimeoutException)
            {   }
        }

        protected void loggerSetDebugLoggingCommand(IEnumerable<string> cmds)
        {
            
            try
            {
                bool? value = getBool(cmds.First());
                if (value != null)
                    loggingConfigurator.SetEnableDebugLoggingAsync(value.Value);
                else
                    Program.UnknownCommand(this);
            }
            catch (TimeoutException)
            {   }

        }

        protected void loggerTestLoggingCommand(IEnumerable<string> cmds)
        {
            Core.LogLevel? lvl = getLogLevel(cmds.First());
            if (lvl != null)
                loggingConfigurator.TestLoggingAsync(lvl.Value);
            else
                Program.UnknownCommand(this);
        }


        protected void fileshareAddDirectoryCommand(IEnumerable<string> cmds)
        {
            string localPath = cmds.First();
            string mountpoint = cmds.Last();

            fileShareComponent.AddSharedDirectoryAsync(localPath, mountpoint);
        }

        protected void fileshareRemoveDirectoryCommand(IEnumerable<string> cmds)
        {
            fileShareComponent.RemoveSharedDirectoryAsync(cmds.First());
        }

        protected void fileshareGetDirectoryCommand(IEnumerable<string> cmds)
        {
            try
            {
                DirectoryObject dir = fileShareComponent.GetDirectoryInfoAsync(cmds.First(), 1).Result;
                printDirectoryObject(dir);
            }
            catch(RemoteErrorException)
            {   }
            catch (TimeoutException)
            {   }

        }

        protected void fileshareGetFileCommand(IEnumerable<string> cmds)
        {
            try
            {
                FileObject file = fileShareComponent.GetFileInfoAsync(cmds.First()).Result;
                printFileObject(file);
            }
            catch (RemoteErrorException)
            {   }
            catch (Exception)
            {   }
        }

        protected void fileshareAddPermissionCommand(IEnumerable<string> cmds)
        {
            Permission p = new Permission();
            p.Path = cmds.First();
            p.PermittedClients = cmds.Skip(1).ToList<string>();

            fileShareComponent.AddPermissionAsync(p);
        }

        protected void fileshareRemovePermissionCommand(IEnumerable<string> cmds)
        {
            fileShareComponent.RemovePermissionAsync(cmds.First());
        }

        protected void fileshareGetMountsCommand(IEnumerable<string> cmds)
        {
            try
            {
                List<MountInfo> mounts = fileShareComponent.GetMountsAsync().Result;

                if (mounts == null)
                {
                    Program.ErrorLine(this, "Return-value for Mounts is null");
                    return;
                }
                else if (!mounts.Any())
                {
                    Program.OutputLine(this, "No Mounts found");
                    return;
                }

                int lineLength = Math.Min(mounts.Max(x => Math.Max(x.LocalPath.Length, x.MountPoint.Length)) + 12, Console.WindowWidth);
                string hl = new String('-', lineLength);

                foreach (MountInfo item in mounts)
                {
                    Program.OutputLine(this,"LocalPath:  {0}", item.LocalPath);
                    Program.OutputLine(this,"MountPoint: {0}", item.MountPoint);
                    Program.OutputLine(this, hl);
                }
            }
            catch (RemoteErrorException)
            { }
            catch (TimeoutException)
            { }

        }

        protected void fileshareCheckPermissionCommand(IEnumerable<string> cmds)
        {
            try
            {
                bool permitted = fileShareComponent.GetPermissionAsync(cmds.First()).Result;

                Program.OutputLine(this, "{0}", permitted);
            }
            catch (RemoteErrorException)
            { }
            catch (TimeoutException)
            { }
        }

        protected void fileshareGetPermissionsCommand(IEnumerable<string> cmds)
        {
            try
            {
                IEnumerable<Permission> permissions = fileShareComponent.GetAddedPermissionsAsync().Result;

                foreach (Permission p in permissions)
                {
                    Program.OutputLine(this, "Permission for " + p.Path);
                    foreach (string str in p.PermittedClients)
                    {
                        Program.OutputLine(this, "     " + p);
                    }
                    Program.OutputLine(this, "");
                }
            }
            catch (RemoteErrorException)
            { }
            catch (TimeoutException)
            { }
        }

        protected void fileshareGetRootDirectoryCommand(IEnumerable<string> cmds)
        {
            try
            {
                string root = fileShareComponent.GetRootDirectoryPathAsync().Result;
                Program.OutputLine(this, root);
            }
            catch (RemoteErrorException)
            { }
            catch (TimeoutException)
            { }
        }

        protected void fileshareSetRootDirectoryCommand(IEnumerable<string> cmds)
        {
            fileShareComponent.SetRootDirectoryPathAsync(cmds.First());
        }



        #region Event Handlers

        private void connection_ConnectionTimedOut(object sender, EventArgs e)
        {
            Program.ErrorLine(this, "Connection timed out");
        }

        private void connection_ConnectionReset(object sender, EventArgs e)
        {
            Program.ErrorLine(this, "Connection reset");
            connection = null;
        }

        private void connection_RemoteErrorOccurred(object sender, ObjectEventArgs<RemoteError> e)
        {
            if (!e.Handled)
            {
                Program.ErrorLine(this, "RemoteError: {0} in {1}", e.Value.ErrorCode.ToString(), e.Value.TargetName);
                e.Handled = true;
            }
        }

        #endregion Event Handlers






        protected override bool checkConnection()
        {
            if (connection == null)
            {
                Program.ErrorLine(this, "Client connection not initialized");
                return false;
            }
            else if (!connection.Connected)
            {
                Program.ErrorLine(this, "Not Connected");
                return false;
            }
            else
            {
                return true;
            }
        }



        private void printDirectoryObject(DirectoryObject dir)
        {
            if (dir == null)
            {
                Program.ErrorLine(this, "DirectoryObject NULL");
                return;
            }


            if (!dir.Directories.Any() && !dir.Files.Any())
            {
                Program.OutputLine(this, "");
                return;
            }

            foreach (DirectoryObject item in dir.Directories)
            {
                Program.OutputLine(this, " <DIR> {0}", item.Name.FormatEscape());
            }

            foreach (FileObject item in dir.Files)
            {
                Program.OutputLine(this, "       {0}", item.Name.FormatEscape());
            }
        }

        private void printFileObject(FileObject file)
        {
            if (file == null)
            {
                Program.ErrorLine(this, "FileObject NULL");
            }
            else
            {
                Program.OutputLine(this, file.Name.FormatEscape());
                //Program.OutputLine(this, "Path:         {0}", file.Path);
                Program.OutputLine(this, "LastEdited:   {0}", file.LastEdited);
                Program.OutputLine(this, "Created:      {0}", file.Created);
                Program.OutputLine(this, "LastAccessed: {0}", file.LastAccessed);
                Program.OutputLine(this, "Hash:         {0}", file.Hash);

            }
        }





    }



}
