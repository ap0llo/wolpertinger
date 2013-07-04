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
using Wolpertinger.Core;
using Wolpertinger.FileShareCommon;
using Nerdcave.Common.Extensions;
using CommandLineParser.CommandParser;

namespace Wolpertinger.Manager.CLI.Commands.Fileshare
{
    abstract class FileShareCommand : CommandBase
    {
        protected FileShareClientComponent getFileShareComponent()
        {
            return new FileShareClientComponent() { ClientConnection = getClientConnection() };
        }

        protected IClientConnection getClientConnection()
        {
            var connection = Context.ActiveConnection;

            if (connection == null)
            {
                throw new CommandExecutionException("No active connection");
            }

            return connection;
        }


        protected void printDirectoryObject(DirectoryObject dir)
        {
            if (dir == null)
            {
                Context.WriteError("DirectoryObject NULL");
                return;
            }


            if (!dir.Directories.Any() && !dir.Files.Any())
            {
                Context.WriteOutput();
                return;
            }

            foreach (DirectoryObject item in dir.Directories)
            {
                Context.WriteOutput(" <DIR>\t{0}", item.Name.FormatEscape());
            }

            foreach (FileObject item in dir.Files)
            {
                Context.WriteOutput("      \t{0}", item.Name.FormatEscape());
            }
        }

        protected void printFileObject(FileObject file)
        {
            if (file == null)
            {
                Context.WriteError("FileObject NULL");
            }
            else
            {
                Context.WriteOutput("Name:         {0}", file.Name.FormatEscape());               
                Context.WriteOutput("LastEdited:   {0}", file.LastEdited);
                Context.WriteOutput("Created:      {0}", file.Created);
                Context.WriteOutput("Hash:         {0}", file.Hash);
            }
        }

        protected void printDirectoryObjectDiff(DirectoryObjectDiff diff)
        {
            if (diff.IsEmpty())
            {
                Context.WriteOutput("No differenes found");
                return;
            }


            //print files/directories missing left
            if (diff.DirectoriesMissingLeft.Any())
            {
                Context.WriteOutput("-----------------------------------------------------------");
                Context.WriteOutput("Directories missing left");
                Context.WriteOutput("-----------------------------------------------------------");
                printList(diff.DirectoriesMissingLeft);
                Context.WriteOutput("-----------------------------------------------------------");
            }

            if (diff.FilesMissingLeft.Any())
            {
                Context.WriteOutput("-----------------------------------------------------------");
                Context.WriteOutput("Files missing left");
                Context.WriteOutput("-----------------------------------------------------------");
                printList(diff.FilesMissingLeft);
                Context.WriteOutput("-----------------------------------------------------------");
            }

            //print files/directories missing right
            if (diff.DirectoriesMissingRight.Any())
            {
                Context.WriteOutput("-----------------------------------------------------------");
                Context.WriteOutput("Directories missing right");
                Context.WriteOutput("-----------------------------------------------------------");
                printList(diff.DirectoriesMissingRight);
                Context.WriteOutput("-----------------------------------------------------------");
            }

            if (diff.FilesMissingRight.Any())
            {
                Context.WriteOutput("-----------------------------------------------------------");
                Context.WriteOutput("Files missing right");
                Context.WriteOutput("-----------------------------------------------------------");
                printList(diff.FilesMissingRight);
                Context.WriteOutput("-----------------------------------------------------------");
            }

            //print conflicted files
            if (diff.FileConflicts.Any())
            {
                Context.WriteOutput("-----------------------------------------------------------");
                Context.WriteOutput("File Conflicts/Changed Files");
                Context.WriteOutput("-----------------------------------------------------------");
                printList(diff.FileConflicts);
                Context.WriteOutput("-----------------------------------------------------------");
            }
        }

        private void printList(IEnumerable<string> list)
        {
            foreach (var item in list)
            {
                Context.WriteOutput(item.Replace("{", "{{").Replace("}", "}}"));
            }
        }
    }
}
