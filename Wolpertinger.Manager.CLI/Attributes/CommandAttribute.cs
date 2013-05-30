using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nerdcave.Common.Extensions;

namespace Wolpertinger.Manager.CLI
{
    class CommandAttribute : Attribute
    {
        static HashSet<char> forbiddenCharacters = new HashSet<char>() { '.', '-' };


        public string Verb { get; set; }

        public string Noun { get; set; }

        public string Module { get; set; }


        public CommandAttribute(CommandVerb verb, string noun, string module = "") 
            : this(verb.ToString(), noun, module)
        {
        }

        public CommandAttribute(string verb, string noun, string module = "")
        {
            if (noun.IsNullOrEmpty() || verb.IsNullOrEmpty())
            {
                throw new ArgumentException("'Noun' and 'Verb' must not be empty");
            }

            if (verb.Any(x => forbiddenCharacters.Contains(x)) || noun.Any(x => forbiddenCharacters.Contains(x)))
            {
                throw new ArgumentException("'Noun' or 'Verb' contains illegal characters");
            }

            
            this.Verb = verb;
            this.Noun = noun;
            this.Module = module;
        }

    }


    public enum CommandVerb
    {
        Get, 
        Set,
        New
    }

}
