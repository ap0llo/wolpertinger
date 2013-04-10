using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wolpertinger.FileShareCommon;
using Nerdcave.Common.Extensions;

namespace Wolpertinger.Testing
{
    public class TestHashingService : IHashingService
    {
        public event EventHandler<GetHashEventArgs> GetHashAsyncCompleted;

        public string GetHash(string filename, Nerdcave.Common.Priority priority)
        {
            return "SomeString".GetHashSHA1();
        }

        public void GetHashAsync(string filename, Nerdcave.Common.Priority priority)
        {
            if (this.GetHashAsyncCompleted != null)
                this.GetHashAsyncCompleted(this, new GetHashEventArgs() { Path = filename, Hash = GetHash(filename, priority) });
        }
    }
}
