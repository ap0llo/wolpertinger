﻿/*

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
using System.Xml.Linq;

namespace Nerdcave.Common.Xml
{
    /// <summary>
    /// Interface that must be implemented by classes to be (de-)seraialized by XmlSerializer
    /// </summary>
    public interface ISerializable
    {

        /// <summary>
        /// Validates a piece of XML and determines wheter it can be deserialized
        /// </summary>
        /// <param name="xml">The XML to be validated</param>
        /// <returns>Returns true when the specified xml is valid and can be deserialzed into a object. Otherwise returns false</returns>
        bool Validate(XElement xml);


        /// <summary>
        /// Serializes the object into XML
        /// </summary>
        /// <returns>Returns a XML representation of the object</returns>
        XElement Serialize();

        /// <summary>
        /// Deserializes the given XML and sets the callee's Properties accordingly
        /// </summary>
        /// <param name="xmlData">The data to be deserialized</param>
        void Deserialize(XElement xmlData);
    }
}
