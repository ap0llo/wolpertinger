using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Nerdcave.Common.Extensions
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Determines whether a new instance of the specified type can be created
        /// </summary>
        [DebuggerStepThrough]
        public static bool CanCreateInstance(this Type type)
        {
            try
            {
                object test = Activator.CreateInstance(type);
            }
            catch (MissingMethodException)
            {
                return false;
            }
            return true;
        }

    }
}
