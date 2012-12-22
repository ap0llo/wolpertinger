using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.Core
{
    public class DefaultConnectionFactory : IConnectionFactory
    {
        public IClientConnection GetClientConnection()
        {
            return new DefaultClientConnection();
        }
    }
}
