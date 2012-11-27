using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Nerdcave.Common
{
    //Source originated from http://stackoverflow.com/questions/5113013/raise-an-event-via-reflection-in-c-sharp
    public static class ReflectionHelper
    {
        /// <summary>
        /// Gets method that will , when invoked raise the event.
        /// </summary>
        /// <remarks>
        /// May not work in all cases, esspecially when working with Windows Forms.
        /// Originally found here http://stackoverflow.com/questions/5113013/raise-an-event-via-reflection-in-c-sharp
        /// </remarks>
        /// <param name="obj">Object that contains the event.</param>
        /// <param name="eventName">Event Name.</param>
        /// <returns></returns>
        public static MethodInfo GetEventInvoker(object obj, string eventName)
        {
            // prepare current processing type
            Type currentType = obj.GetType();

            // try to get special event decleration
            while (true)
            {
                FieldInfo fieldInfo = currentType.GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.GetField);

                if (fieldInfo == null)
                {
                    if (currentType.BaseType != null)
                    {
                        // move deeper
                        currentType = currentType.BaseType;
                        continue;
                    }

                    return null;
                }

                // found
                return ((MulticastDelegate)fieldInfo.GetValue(obj)).Method;
            }
        }
    }
}
