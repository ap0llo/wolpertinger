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
using System.Xml;
using System.Xml.Linq;
using Nerdcave.Common.Extensions;
using Slf;

namespace Wolpertinger.Core
{
    public class DefaultMessageProcessor : IMessageProcessor
    {
        ILogger logger = LoggerService.GetLogger("DefaultMessageProcessor");

        private byte[] _encryptionKey;
        private byte[] _encryptionIV;

        /// <summary>
        /// Specifies whether outgoing messages are to be encrypted
        /// </summary>
        public bool EncryptMessages { get; set; }

        /// <summary>
        /// Specifies whether outgoing messages are to be compressed
        /// </summary>
        public bool CompressMessages { get; set; }

        /// <summary>
        /// Specifies whether outgoing messages are to be signed
        /// </summary>
        public bool SignMessages { get; set; }

        /// <summary>
        /// The key used for encryption
        /// </summary>
        public byte[] EncryptionKey 
        {
            get { return _encryptionKey; }
            set 
            {
                _encryptionKey = value;
            }
        }

        /// <summary>
        /// The Initialization Vector used for encryption
        /// </summary>
        public byte[] EncryptionIV 
        {
            get { return _encryptionIV; }
            set
            {
                _encryptionIV = value;
            }
        }



        /// <summary>
        /// Parses, decrypts, decompresses and checks the signature of an incoming message and returns it as MessageProcessingResult
        /// </summary>
        /// <param name="message">The (text-)message to be processed</param>
        /// <returns>
        /// Returns a MessageProcessingResult with the message and information about how it was delivered
        /// </returns>
        /// <exception cref="System.FormatException"></exception>
        public MessageProcessingResult ProcessIncomingMessage(string message)
        {
            MessageProcessingResult result = new MessageProcessingResult();

            string body = message.Split(';').Last();

            //check if preprocessing is necessary
            if (message.Contains(";"))
            {                
                byte[] messageBytes = body.GetBytesBase64();

                //Process all of the message's flags
                foreach (string item in message.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Where(x => x.Contains(":")))
                {
                    MessageFlag flag = getFlag(item);

                    if (flag == MessageFlag.Signed)
                    {
                        result.WasSigned = true;
                        //TODO check signature
                    }
                    else if (flag == MessageFlag.Encrypted)
                    {
                        //check if key and IV for encryption ahve been set
                        if (this.EncryptionKey == null || this.EncryptionIV == null)
                        {
                            logger.Error("Encounterd encrypted message, but key and/or IV have not been set");
                            continue;
                        }


                        result.WasEncrypted = true;

                        //check if the encryption-algorithm is supported
                        if (item.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries).Last().ToLower() == "aes")
                        {
                            //decrypt the message
                            messageBytes = messageBytes.DecryptAES(EncryptionKey, EncryptionIV);
                        }
                        else 
                        {
                            throw new FormatException("Unsupported Encryption algorithm encountered");
                        }
                    }
                    else if (flag == MessageFlag.Compressed)
                    {
                        result.WasCompressed = true;

                        //read the data's original length from the compressed flag
                        string lengthValue = item.Split(new char[] {':'}, StringSplitOptions.RemoveEmptyEntries).Last() ;
                        int length = -1;

                        if (int.TryParse(lengthValue, out length))
                        {
                            //decompress the data
                            messageBytes = messageBytes.Take(length).ToArray<byte>();
                            messageBytes = messageBytes.Decompress();
                        }
                        else 
                        {
                            throw new FormatException("Invalid Compression flag");
                        }
                    }
                }

                //Get the message's body from the now processed bytes
                body = messageBytes.ToStringUTF8();
            }

            //Try to parse the message
            Message msg;
            try
            {
                //try to parse it as XML
                XElement xmlMessage = XElement.Parse(body);
                switch (xmlMessage.Name.LocalName.ToLower())
                {
                    case "remotemethodcall":
                        msg = (Message)new RemoteMethodCall().Deserialize(xmlMessage);
                        break;
                    case "remotemethodresponse":
                        msg = (Message)new RemoteMethodResponse().Deserialize(xmlMessage);
                        break;
                    case "remoteerror":
                        msg = (Message)new RemoteError(RemoteErrorCode.UnspecifiedError).Deserialize(xmlMessage);
                        break;
                    default:
                        throw new FormatException("Unknown message format");
                }            
            }
            //Catch XmlException and wrap it into a format exception (makes catching errors in the caller easier)
            catch (XmlException ex)
            {
                throw new FormatException("Message wasn't valid XML", ex);
            }

            result.Message = msg;

            return result;
        }


        /// <summary>
        /// Compresses, encrypts and signs an outgoing messages
        /// </summary>
        /// <param name="msg">The Message to process</param>
        /// <returns>
        /// Returns the processed message as text ready to be sent
        /// </returns>
        public string ProcessOutgoingMessage(Message msg)
        {
            //if neither Compression nor Encryption is enabled return the message as string
            if (!CompressMessages && !EncryptMessages)
            {
                if (SignMessages)
                {
                    //TODO: Sign message
                }

                return msg.Serialize().ToString();
            }
            else
            {
                //serialize the message, get it as bytes (necessary for compression and encryption)
                byte[] message = msg.Serialize().ToString().GetBytesUTF8();
                string flags = "";
                
                //compress message, add compression flag
                if (CompressMessages)
                {
                    message = message.Compress();
                    flags = String.Format("{0}:{1};", "gzip", message.Length) + flags;
                }

                //encrpyt message, add encryption flag
                if (EncryptMessages)
                {
                    message = message.Encrypt(EncryptionKey, EncryptionIV);
                    flags = String.Format("{0}:{1};", "secure", "aes") + flags;
                }

                if (SignMessages)
                {
                    //TODO: Sign message
                }

                //append message flags in front of message
                return flags + message.ToBase64String();
            }


        }



        private enum MessageFlag
        {
            None = 0,
            Compressed,
            Encrypted,
            Signed
        }

        private MessageFlag getFlag(string text)
        {
            text = text.ToLower();

            if (text.StartsWith("gzip"))
                return MessageFlag.Compressed;
            else if (text.StartsWith("secure"))
                return MessageFlag.Encrypted;
            else if (text.StartsWith("signature"))
                return MessageFlag.Signed;
            else
                return MessageFlag.None;
        }
        

    }
}
