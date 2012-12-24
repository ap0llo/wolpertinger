using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.Core
{
    public class DefaultConnectionFactory : IConnectionFactory
    {
        public virtual IClientConnection GetClientConnection()
        {
            var factory = new DefaultClientConnection();
            factory.ComponentFactory = new DefaultComponentFactory();

            return factory;
        }
    }
}
