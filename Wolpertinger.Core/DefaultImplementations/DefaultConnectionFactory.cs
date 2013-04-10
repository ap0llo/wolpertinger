using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.Core
{
    /// <summary>
    /// Default implementation  of <see cref="IConnectionFactory"/>
    /// </summary>
    public class DefaultConnectionFactory : IConnectionFactory
    {
        /// <summary>
        /// Gets a new instance of a implementation of IClientConnection
        /// </summary>
        /// <returns>Returns a new instance of implementation of IClientConnection or null if no implementation could be found</returns>
        public virtual IClientConnection GetClientConnection()
        {
            var factory = new DefaultClientConnection();
            factory.ComponentFactory = new DefaultComponentFactory();

            return factory;
        }
    }
}
