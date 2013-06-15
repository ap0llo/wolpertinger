using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.Manager.CLI.CommandLib
{
    public class ParameterParserAttribute : Attribute
    {
        public Type ParameterType { get; set; }

        public ParameterParserAttribute(Type parameterType)
        {
            this.ParameterType = parameterType;
        }
    }
}
