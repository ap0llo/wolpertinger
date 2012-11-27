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
    public interface IMessageProcessor
    {
        /// <summary>
        /// Specifies whether outgoing messages are to be encrypted
        /// </summary>
        bool EncryptMessages { get; set; }

        /// <summary>
        /// Specifies whether outgoing messages are to be compressed
        /// </summary>
        bool CompressMessages { get; set; }

        /// <summary>
        /// Specifies whether outgoing messages are to be signed
        /// </summary>
        bool SignMessages { get; set; }

        /// <summary>
        /// The key used for encryption
        /// </summary>
        byte[] EncryptionKey { get; set; }

        /// <summary>
        /// The Initialization Vector used for encryption
        /// </summary>
        byte[] EncryptionIV { get; set; }


        /// <summary>
        /// Parses, decrypts, decompresses and checks the signature of an incoming message and returns it as MessageProcessingResult
        /// </summary>
        /// <param name="message">The (text-)message to be processed</param>
        /// <returns>Returns a MessageProcessingResult with the message and information about how it was delivered</returns>
        MessageProcessingResult ProcessIncomingMessage(string message);

        /// <summary>
        /// Compresses, encrypts and signs an outgoing messages
        /// </summary>
        /// <param name="msg">The Message to process</param>
        /// <returns>Returns the processed message as text ready to be sent</returns>
        string ProcessOutgoingMessage(Message msg);

    }


    public struct MessageProcessingResult
    {
        public bool WasEncrypted { get; set; }
        
        public bool WasCompressed { get; set; }
        
        public bool WasSigned { get; set; }
        
        public bool SignatureWasValid { get; set; }

        
        public Message Message { get; set; }
    }

}
