using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.Manager.CLI
{
    class ActionAttribute : Attribute
    {

        public string Verb { get; set; }



        public ActionAttribute(ActionVerb verb)
        {
            this.Verb = verb.ToString().ToLower();
        }


        public ActionAttribute(string verb)
        {
            this.Verb = verb.ToLower();
        }

    }


    public enum ActionVerb
    {
        Get, 
        Set,
        New
    }
}
