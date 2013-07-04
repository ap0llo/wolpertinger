using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommandLineParser.Attributes
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
