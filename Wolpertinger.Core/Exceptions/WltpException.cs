using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.Core
{
    public class WtlpException : Exception
    {
        public WtlpException(Result error)
        {
            this.Error = error;
        }


        public Result Error { get; set; }
    }
}
