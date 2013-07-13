/*

Licensed under the new BSD-License
 
Copyright (c) 2011-2013, Andreas Grünwald 
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

    Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
    Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
    Neither the name of the Wolpertinger project nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Wolpertinger.Core
{

    /// <summary>
    /// A implementation of IComponentFactory that will search for components in all referenced assemblies. 
    /// Implementations of IComponent are mapped to component-names based on the <see cref="Wolpertinger.Core.ComponentAttribute" /> attribute
    /// </summary>
    public class DefaultComponentFactory : IComponentFactory
    {
        //Dictionary to store types of server-components
        private Dictionary<string, Type> serverComponents = new Dictionary<string, Type>();
        

        /// <summary>
        /// Initializes a new instance of <see cref="DefaultComponentFactory" />
        /// </summary>
        public DefaultComponentFactory()
        {
            loadTypes();        
        }




        #region IComponentFactory Members


        /// <summary>
        /// Gets a new instance of the IServerComponent that matches the specified component name.
        /// </summary>
        /// <param name="name">The name of the component</param>
        /// <returns>
        /// Returns a new IServerComponent if a matching implementation could be found, otherwise returns null
        /// </returns>
        public IComponent GetServerComponent(string name)
        {
            name = name.ToLower();
            //try to find a component matching the given name and create a new instance of it
            if (serverComponents.ContainsKey(name))
                return (IComponent)Activator.CreateInstance(serverComponents[name]);
            else
                return null;
        }


        #endregion IComponentFactory Members




        private void loadTypes()
        {
            //Get a list of all assemblies referenced by the entry assembly and add the entry-assembly itself to the list as well
            IEnumerable<Assembly> assemblies =  Assembly.GetEntryAssembly()
                                                    .GetReferencedAssemblies().Select(x => Assembly.Load(x)).Where(x=> !x.FullName.Contains("Raven"))
                                                    .Union(new Assembly[]{ Assembly.GetEntryAssembly()})
                                                .ToList<Assembly>();
            

            //load all types from all assemblies that implement IComponent and cache them in dictionaries to make Get*Component() faster
            foreach (Assembly a in assemblies)
            {
                Type[] types = a.GetTypes();
                foreach (Type t in types)
                {
                    if (!t.IsAbstract)
                    {
                        try
                        {
                            //try if instances of this type can be initialized
                            //IComponent instance = (IComponent)Activator.CreateInstance(t);

                            //get the class' ComponentAttribute attributes
                            var attributes = t.GetCustomAttributes(typeof(ComponentAttribute), false);
                            if (attributes.Any())
                            {
                                ComponentAttribute attribute = (ComponentAttribute)attributes.First();
                                
                                //add to list of server-components, if component is Server of ClientServer and no component of this name has been added already
                                if (!serverComponents.ContainsKey(attribute.Name.ToLower()))
                                {
                                    serverComponents.Add(attribute.Name.ToLower(), t);

                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            //NullReferenceExceptions and MissingMethodExceptions can be ignored (component will simply be ignored)
                            if (!(ex is NullReferenceException || ex is MissingMethodException))
                                throw;
                        }

                    }
                }
            }


        }

    }
}
