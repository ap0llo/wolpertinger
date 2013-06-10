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

namespace Wolpertinger.Manager.CLI.Commands.Connection
{
    [Command(CommandVerb.Get, "Connection", "Connection")]
    class GetConnectionCommand : CommandBase
    {

        [Parameter("ConnectionName", IsOptional = true, Position = 2)]
        public string ConnectionName { get; set; }


        public override void Execute()
        {

            var query = Context.ConnectionManager.GetClientConnections().Where(x => x.Target.ToLower() == ConnectionName.ToLower());

            int index = 1;
            if (ConnectionName == null)
            {
                foreach (var connection in Context.ConnectionManager.GetClientConnections())
                {
                    Context.WriteOutput("#{0} | {1}", index++, connection.Target);
                }    
            }
            else if (ConnectionName.StartsWith("#"))
            {
                int outValue;
                if (int.TryParse(ConnectionName.Substring(1), out outValue))
                {
                    if (outValue > 0 && Context.ConnectionManager.GetClientConnections().Count() >= outValue)
                    {
                        Context.WriteOutput(Context.ConnectionManager.GetClientConnections().Skip(outValue -1).First().Target);
                        return;
                    }
                }

                abort("Connection not found");
            }
            else if (query.Any())
            {
                Context.WriteOutput(query.First().Target);
            }
            else
            {
                abort("Connection not found");
            }
        }
    }
}
