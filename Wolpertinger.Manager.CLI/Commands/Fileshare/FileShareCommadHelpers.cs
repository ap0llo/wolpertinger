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

    internal static class FileShareCommadHelpers
    {
        public static void PrintDirectoryObject(DirectoryObject dir, CommandContext context)
        {
            if (dir == null)
            {
                context.WriteError("DirectoryObject NULL");
                return;
            }


            if (!dir.Directories.Any() && !dir.Files.Any())
            {
                context.WriteOutput();
                return;
            }

            foreach (DirectoryObject item in dir.Directories)
            {
                context.WriteOutput(" <DIR>\t{0}", item.Name.FormatEscape());
            }

            foreach (FileObject item in dir.Files)
            {
                context.WriteOutput("      \t{0}", item.Name.FormatEscape());
            }
        }

        public static void PrintFileObject(FileObject file, CommandContext context)
        {
            if (file == null)
            {
                context.WriteError("FileObject NULL");
            }
            else
            {
                context.WriteOutput("Name:         {0}", file.Name.FormatEscape());
                context.WriteOutput("LastEdited:   {0}", file.LastEdited);
                context.WriteOutput("Created:      {0}", file.Created);
                context.WriteOutput("Hash:         {0}", file.Hash);
            }
        }

        public static void PrintList(IEnumerable<string> list, CommandContext context)
        {
            foreach (var item in list)
            {
                context.WriteOutput(item.Replace("{", "{{").Replace("}", "}}"));
            }
        }


        public static void PrintDirectoryObjectDiff(DirectoryObjectDiff diff, CommandContext context)
        {
            if (diff.IsEmpty())
            {
                context.WriteOutput("No differences found");
                return;
            }


            //print files/directories missing left
            if (diff.DirectoriesMissingLeft.Any())
            {
                context.WriteOutput("-----------------------------------------------------------");
                context.WriteOutput("Directories missing left");
                context.WriteOutput("-----------------------------------------------------------");
                PrintList(diff.DirectoriesMissingLeft, context);
                context.WriteOutput("-----------------------------------------------------------");
            }

            if (diff.FilesMissingLeft.Any())
            {
                context.WriteOutput("-----------------------------------------------------------");
                context.WriteOutput("Files missing left");
                context.WriteOutput("-----------------------------------------------------------");
                PrintList(diff.FilesMissingLeft, context);
                context.WriteOutput("-----------------------------------------------------------");
            }

            //print files/directories missing right
            if (diff.DirectoriesMissingRight.Any())
            {
                context.WriteOutput("-----------------------------------------------------------");
                context.WriteOutput("Directories missing right");
                context.WriteOutput("-----------------------------------------------------------");
                PrintList(diff.DirectoriesMissingRight, context);
                context.WriteOutput("-----------------------------------------------------------");
            }

            if (diff.FilesMissingRight.Any())
            {
                context.WriteOutput("-----------------------------------------------------------");
                context.WriteOutput("Files missing right");
                context.WriteOutput("-----------------------------------------------------------");
                PrintList(diff.FilesMissingRight, context);
                context.WriteOutput("-----------------------------------------------------------");
            }

            //print conflicted files
            if (diff.FileConflicts.Any())
            {
                context.WriteOutput("-----------------------------------------------------------");
                context.WriteOutput("File Conflicts/Changed Files");
                context.WriteOutput("-----------------------------------------------------------");
                PrintList(diff.FileConflicts,context);
                context.WriteOutput("-----------------------------------------------------------");
            }
        }

    }
}
