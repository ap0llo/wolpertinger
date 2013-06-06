using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wolpertinger.Core;
using Wolpertinger.FileShareCommon;
using Nerdcave.Common.Extensions;

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
