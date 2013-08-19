/*

Licensed under the new BSD-License
 
Copyright (c) 2011-2013, Andreas Grünwald 
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

    Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
    Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer 
	in the documentation and/or other materials provided with the distribution.
    Neither the name of the Wolpertinger project nor the names of its contributors may be used to endorse or promote products 
	derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS 
BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, 
STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.Core.DefaultImplementations
{
    /// <summary>
    /// IAddress implementation for XMPP clients
    /// </summary>
    public class XmppAddress
        : IAddress
    {

        public string Username { get; set; }

        public string Server { get; set; }


        #region IAddress members

        public string Name 
        {
            get { return String.Format("{0}@{1}", Username, Server); }
        }

        public string Resource { get; private set; }

        #endregion
        


        public override string ToString()
        {
            return Name + "/" + Resource;
        }

        public static XmppAddress Parse(string value)
        {
            //example: foo@example.com/resource


            var result = new XmppAddress();

            //check if value is a valid JId
            if (value.Count(x => x == '@') != 1 || value.Count(x => x == '/') > 1)
            {
                throw new ArgumentException("{0} isn't a valid XmppAddress", value);
            }

            //if value contains resource, set the result's resource and remove resource from 'value'
            if (value.Contains("/"))
            {
                if (value.EndsWith("/"))
                {
                    result.Resource = "";
                }
                else
                {
                    result.Resource = value.Substring(value.IndexOf('/'));
                }
                
                value = value.Remove(value.IndexOf('/'));                
            }


            var split = value.Split('@');
            result.Username = split[0];
            result.Server = split[1];

            return result;
        }
    }
}
