using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nerdcave.Common.Extensions;

namespace Wolpertinger.Manager.CLI
{
    public class ParameterAttribute : Attribute
    {
        public string Name { get; set; }

        public bool IsOptional { get; set; }

        public int Position { get; set; }


        public ParameterAttribute(string name)
            : this (name, false)
        {

        }

        public ParameterAttribute(string name, bool isOptional, int position = -1)
        {
            if (name.IsNullOrEmpty())
            {
                throw new ArgumentException("ParameterName may not be empty");
            }

            if (name.StartsWith("-") || name.StartsWith("/"))
            {
                throw new ArgumentException("ParameterName may not start with '-' or '/'");
            }

            this.Name = name;
            this.IsOptional = isOptional;
            this.Position = position;
        }


    }
}
