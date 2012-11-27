using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.Core
{
    public interface IClientComponent
    {
        /// <summary>
        /// The IClientConnection used to communicate with the target-client
        /// </summary>
        public IClientConnection ClientConnection { get; set; }

        public string ComponentName { get; }
    }
}
