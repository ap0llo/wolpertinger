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
using System.Text;

namespace Wolpertinger.Core
{
    public interface IComponentFactory
    {
        /// <summary>
        /// Gets a new instance of the IServerComponent that matches the specified component name.
        /// </summary>
        /// <param name="name">The name of the component</param>
        /// <returns>Returns a new IServerComponent if a matching implementation could be found, otherwise returns null</returns>
        IComponent GetServerComponent(string name);

        /// <summary>
        /// Gets a new instance of the IClientComponent that matches the specified component name.
        /// </summary>
        /// <param name="name">The name of the component</param>
        /// <returns>Returns a new IClientComponent if a matching implementation could be found, otherwise returns null</returns>
        IComponent GetClientComponent(string name);

        /// <summary>
        /// Checks whether the specified component is a combined client- and server-component        
        /// </summary>
        /// <param name="component">The component to check</param>
        bool IsClientServerComponent(IComponent component);        


    }
}
