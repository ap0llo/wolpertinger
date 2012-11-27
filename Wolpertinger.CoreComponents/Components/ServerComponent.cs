using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.Core
{
    public abstract class ServerComponent : IServerComponent
    {
        public IClientConnection ClientConnection { get; set; }
       
    }
}
