using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wolpertinger.Core;

namespace Wolpertinger.Manager.CLI
{
    class CommandContext
    {

        public IConnectionManager ConnectionManager { get; set; }

        public IEnumerable<IClientConnection> AllConnections
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IClientConnection CurrentConnection { get; set; }



        public void AddClientConnection(IClientConnection connection)
        {

        }

        public bool ConnectionExists(string target)
        {
            throw new NotImplementedException();
        }






        public void WriteError(string message)
        {
            throw new NotImplementedException();
        }

        public void WriteError(string format, params object[] args)
        {
            WriteError(String.Format(format, args));
        }

        public void WriteInfo(string message)
        {
            throw new NotImplementedException();
        }

    }
}
