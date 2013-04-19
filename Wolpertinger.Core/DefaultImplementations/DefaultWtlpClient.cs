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
using Nerdcave.Common;
using Nerdcave.Common.Extensions;
using Slf;
using System.Timers;
using System.Threading;

namespace Wolpertinger.Core
{
    /// <summary>
    /// Default implementation of the transmission layer of the protocol
    /// </summary>
    public class DefaultWtlpClient : IWtlpClient
    {

        #region Constants
        
        /*XMPP spec says limit on server side must not be smaller than 10K, googling shows 65k is common
        Limiting to 30500 characters should result in message sizes of about 30K (splitting messages adds a little overhead) which seems like a good compromise */
        private const int MESSAGELENGTHLIMIT = 30500;
        private const int TIMEOUTINTERVAL = 2000000;//20000;      //interval after which a sent message times out (in milliseconds)

        #endregion

        #region Fields
        private IMessagingClient _messagingClient;
        private string _recipient;
        private bool _compressMessages;
        private bool _encryptMessages;
        private byte[] _encryptionKey;
        private byte[] _encryptionIV;

        private int sentMessagesCount = 0;


        private ILogger logger = LoggerService.GetLogger("DefaultTransmissionClient");

        private Dictionary<int, Semaphore> waitingThreads = new Dictionary<int, Semaphore>();
        private Dictionary<int, Result> messageResults = new Dictionary<int, Result>();

        private Dictionary<int, string[]> messageFragments = new Dictionary<int, string[]>();
        private Dictionary<int, System.Timers.Timer> timeoutTimers = new Dictionary<int, System.Timers.Timer>();

        #endregion

        #region Events

        /// <summary>
        /// Event that is raised each time a message has been received and parsed.
        /// </summary>
        public event EventHandler<ObjectEventArgs<ParsingResult>> MessageReceived;

        public event EventHandler MessageFragmentReceived;

        /// <summary>
        /// Event that is raised when the connection timed out
        /// </summary>
        public event EventHandler TransmissionTimedOut;

        #endregion


        /// <summary>
        /// Initializes a new instance of <see cref="DefaultWtlpClient" />
        /// </summary>
        /// <param name="messagingClient">The underlying messaging client used for communication</param>
        /// <param name="recipient">The recipient to receive messages from and send messages to</param>
        public DefaultWtlpClient(IMessagingClient messagingClient, string recipient)
        {
            //set messagingClient and recipient
            this.Recipient = recipient;
            this.MessagingClient = messagingClient;            
        }



        /// <summary>
        /// The underlying messaging client used for sending and receiving messages
        /// </summary>
        public IMessagingClient MessagingClient
        {
            get 
            {
                lock (this)
                {
                    return _messagingClient;
                }
            }
            private set 
            {
                lock (this)
                {
                    if (value != _messagingClient)
                    {
                        if (_messagingClient != null)
                        {
                            _messagingClient.MessageReceived -= MessagingClient_MessageReceived;    
                        }
                        
                        _messagingClient = value;

                        if (_messagingClient != null)
                        {
                            _messagingClient.MessageReceived += MessagingClient_MessageReceived;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The address of the WltpClient's target
        /// </summary>
        public string Recipient
        {
            get
            {
                lock (this)
                {
                    return _recipient;
                }
            }
            set
            {
                lock (this)
                {
                    _recipient= value;
                }
            }
        }

        /// <summary>
        /// Specifies whether messages will be compressed before being sent
        /// </summary>
        public bool CompressMessages 
        { 
            get
            {
                lock (this)
                {
                    return _compressMessages;
                }
            }
            set
            {
                lock (this)
                {
                    _compressMessages = value;
                }
            }
        }

        /// <summary>
        /// Specifies whether messages will be encrypted before being sent
        /// </summary>
        public bool EncryptMessages
        {
            get
            {
                lock (this)
                {
                    return _encryptMessages;
                }
            }
            set
            {
                lock (this)
                {
                    _encryptMessages = value;
                }
            }
        }

        /// <summary>
        /// The key used for encryption
        /// </summary>
        public byte[] EncryptionKey
        {
            get
            {
                lock (this)
                {
                    return _encryptionKey;
                }
            }
            set
            {
                lock (this)
                {
                    _encryptionKey= value;
                }
            }
        }

        /// <summary>
        /// The Initialization Vector used for encryption
        /// </summary>
        public byte[] EncryptionIV
        {
            get
            {
                lock (this)
                {
                    return _encryptionIV;
                }
            }
            set
            {
                lock (this)
                {
                    _encryptionIV = value;
                }
            }
        }


        /// <summary>
        /// Sends the specified data to the target client specified in the 'Recipient' property
        /// </summary>
        /// <param name="message">The message to be sent</param>
        /// <exception cref="WtlpException"></exception>
        public void Send(byte[] message)
        {
            //the message's Id
            int messageId;
            //the message's payload  
            byte[] payload = message;

            //get the next message Id 
            lock (this)
            {
                messageId = ++sentMessagesCount;
            }

            //add message Id to the metadata string
            string metadata = String.Format("message_id:{0};", messageId);

            //Compress message if compression is enabled
            if (this.CompressMessages)
            {
                payload = payload.Compress();
                metadata += String.Format("gzip:{0};", payload.Length);
            }

            //Encrypt message if encryption is enabled
            if (this.EncryptMessages)
            {
                payload = payload.Encrypt(this.EncryptionKey, this.EncryptionIV);
                metadata += "encryption:aes;";
            }

            //base64-encode the message's payload as string
            string payloadStr = payload.ToStringBase64();

            //split the message's payload into multplie parts if necessary
            //the number of fragments the message will be split in (if length limit is not exceeded, it's 1, so message will not be split)
            int fragmentsCount = (payloadStr.Length > MESSAGELENGTHLIMIT) ? ((payloadStr.Length / MESSAGELENGTHLIMIT) + 1) : 1;

            string[] fragments = payloadStr.Split(fragmentsCount);       //split message
            
            //add metadata in front of the message
            for (int i = 0; i < fragments.Length; i++)
            {
                fragments[i] = metadata + fragments[i];

                //if message was split, add metadata necessary to restore the original message 
                if (fragmentsCount > 1)
                {
                    fragments[i] =  String.Format("fragment_count:{0};", fragments.Length) + fragments[i];
                    fragments[i] = String.Format("fragment_index:{0};", i) + fragments[i];                 
                }
            }

            //Semaphore used to wait for until a delivery confirmation is received for the sent message
            Semaphore sem = new Semaphore(0, 1);

            //Register the semaphore so it is triggerd when the confirmation for the message is received
            lock (this)
            {
                waitingThreads.Add(messageId, sem);
            }

            //Send all message fragments
            foreach (var item in fragments)
            {
                this.MessagingClient.SendMessage(this.Recipient, item);                
            }

            //Set up a timer that relases the semaphor after the timeout interval to prevent the thread from blocking indefinitely
            object objectLock = this;
            System.Timers.Timer timeoutTimer = new System.Timers.Timer(TIMEOUTINTERVAL * fragmentsCount);
            timeoutTimer.Elapsed += (s,e) => 
                    {
                        lock (objectLock)
                        {
                            if (!messageResults.ContainsKey(messageId))
                            {
                                messageResults.Add(messageId, Result.Timeout);
                            }
                        }
                        sem.Release();

                        lock (objectLock)
                        {
                            waitingThreads.Remove(messageId);
                        }
                        timeoutTimer.Stop();
                    };

            timeoutTimer.Start();


            logger.Info("Sending Message, Id {0} and waiting for delivery confimation", messageId);

            //Wait for the message transmission to complete
            sem.WaitOne();

            //check whether the message was sent sucessfully, otherwise throw a exception with the error
            Result result = messageResults[messageId];

            if (result == Result.Success)
            {
                return;
            }
            else
            {
                throw new WtlpException(result);
            }
        }

        /// <summary>
        /// Processes the specified message
        /// </summary>
        /// <param name="message">The message to be processed</param>
        public void HandleMessage(string message) 
        {
            parseMessage(message);
        }

        /// <summary>
        /// Unsubscribes from all events of the underlying IMessagingClient the WtlpClient is subscribed to
        /// </summary>
        public void Detach()
        {
            this.MessagingClient = null;
        }

        /// <summary>
        /// Verfies a message's format is valid.
        /// </summary>
        /// <param name="message">The message to be validates</param>
        /// <returns>Returns true if the message is valid and can be parsed. Otherwise returns false</returns>
        private bool verifyMessageFormat(string message)
        {
            if (message.IsNullOrEmpty())
                return false;

            //check if message contains an even number of semicolons
            //if (!message.Count(x => x == ';').IsEven() && message.Count(x => x == ';') > 0)
            if (message.Count(x => x == ';') == 0)
            {
                logger.Error("Invalid message, 0 semicolons found");
                return false;
            }

            //check if every item separated by semicolons consists of key and value
            var fragments = message.Split(';').Where(x => !x.IsNullOrEmpty()) as IEnumerable<string>;
            if (!message.EndsWith(";"))
                fragments = fragments.Take(fragments.Count() - 1);

            if (!fragments.Any(x => x.Contains(':')))
            {
                logger.Error("Invalid message, Key-Value delimiter not found");
                return false;
            }


            //check if both key and value of every metadate are not null
            if (fragments.Any(x =>
                                x.Split(':').Any(y => y.IsNullOrEmpty()
                                || x.Split(':').Count() != 2)
                            ))
            {
                logger.Error("Invalid Message, metadata key or value null");
                return false;
            }

            //check for duplicate keys
            var keys = fragments.Select(x => x.Split(':').First()).Select(x => x.ToLower());
            if (keys.Any(x => keys.Count(y => y == x) > 1))
            {
                logger.Error("Invalid Message, duplicate keys detected");
                return false;
            }


            //check for unknown keys
            Key parsedKey;
            if (keys.Any(x => !Enum.TryParse<Key>(x, true, out parsedKey)))
            {
                logger.Error("Invalid Message, unknown key detected");
                return false;
            }

            //all checks were successful, the message seems to be valid
            return true;
        }        


        /// <summary>
        /// Parses an message received through the messaging client
        /// </summary>
        /// <param name="message"></param>
        private void parseMessage(string message)
        {            
            //check if the message's format is valid
            if (!verifyMessageFormat(message))
            {
                logger.Error("Received invalid message, aborting message processing");
                sendresult(Result.InvalidFormat);
                return;
            }

            //get the messages metadata and payload
            IEnumerable<string> parts = message.Split(';').Where(x => !x.IsNullOrEmpty());
            string payload_str =  message.EndsWith(";") ?  null : parts.Last();
            parts = (payload_str == null) ? parts : parts.Take(parts.Count() - 1);
            
            
            //parse metadata keys
            var metadata = parts.Select(x => new Tuple<Key, string>((Key)Enum.Parse(typeof(Key), x.Split(':').First(), true), x.Split(':').Last()));

            
            //check for message id
            var messageIdQuery = metadata.Where(x => x.Item1 == Key.Message_id);
            int messageId = -1;
            if (!messageIdQuery.Any() || !int.TryParse(messageIdQuery.First().Item2, out messageId))
            {
                logger.Warn("Invalid message. MessageId not found or invalid");
                sendresult(Result.InvalidFormat);
                return;
            }

            //check if message is a delivery notification
            var resultQuery = metadata.Where(x => x.Item1 == Key.Result);
            if (metadata.Count() == 2 && messageId > 0 && resultQuery.Any())
            {
                //handle delivery confirmation     

                Result deliveryResult;
                if (!Enum.TryParse(resultQuery.First().Item2, true, out deliveryResult))
                    deliveryResult = Result.UnknownError;

                logger.Info("Received delivery confirmation for message {0} from {1}, Result: {2}", messageId, this.Recipient,  deliveryResult);


                lock (this)
                {
                    //store result in result cache and unblock waiting threads
                    if (waitingThreads.ContainsKey(messageId))
                    {
                        if (!messageResults.ContainsKey(messageId))
                            messageResults.Add(messageId, deliveryResult);
                        else
                            messageResults[messageId] = deliveryResult;

                        //relese threads waiting for the result
                        waitingThreads[messageId].Release();
                    }                    
                }
                
                return;
            }


            //check for splitted messages            
            var fragmentIndexQuery = metadata.Where(x => x.Item1 == Key.Fragment_Index);
            var fragmentCountQuery = metadata.Where(x => x.Item1 == Key.Fragment_Count);

            if (fragmentCountQuery.Any() || fragmentIndexQuery.Any())
            {
                logger.Info("Received splitted message");
                
                if (!(fragmentIndexQuery.Any() && fragmentIndexQuery.Any()))
                {
                    logger.Error("Missing metadata information for splitted message");                    
                    return;
                }

                int fragmentIndex = int.Parse(fragmentIndexQuery.First().Item2);
                int fragmentCount = int.Parse(fragmentCountQuery.First().Item2);

                if (fragmentCount <= 0 || fragmentIndex < 0)
                {
                    logger.Warn("Fragment Index or Count out of range");
                    sendresult(Result.SplittingError, messageId);
                    return;
                }

                if (!messageFragments.ContainsKey(messageId))
                    messageFragments.Add(messageId, new string[fragmentCount]);   

                //check if fragment index is valid
                if (fragmentIndex >= messageFragments[messageId].Length)
                {
                    logger.Warn("Fragment index out of range");
                    sendresult(Result.SplittingError, messageId);
                    return;
                }

                //check if fragment count is consistent with previous messages
                if (fragmentCount != messageFragments[messageId].Length)
                {
                    logger.Error("Inconsistent fragment count");
                    sendresult(Result.SplittingError, messageId);
                }

                messageFragments[messageId][fragmentIndex] = payload_str;
                onMessageFragmentReceived();

                //check if all fragments have been received
                if (messageFragments[messageId].Any(x => x == null))
                {
                    logger.Info("Missing fragments of message {0}, waiting for next piece", messageId);
                    return;
                }
                else
                {
                    logger.Info("Received all fragments of message {0}", messageId);
                    payload_str = messageFragments[messageId].Aggregate((str1, str2) => str1 + str2);
                    
                    messageFragments.Remove(messageId);
                }
            }


            ParsingResult result = new ParsingResult();
            byte[] payload = payload_str.GetBytesBase64();

            //check if message is encrypted
            var encryptedMetadate = metadata.Any(x => x.Item1 == Key.Encryption) ? metadata.First(x => x.Item1 == Key.Encryption) : null;

            if (encryptedMetadate != null)
            {
                //check if encryption algorith is supported
                if (encryptedMetadate.Item2.ToLower() != "aes")
                {
                    logger.Warn("Encryption algorithm not supported");
                    sendresult(Result.EncryptionError);
                    return;
                }

                payload = payload.DecryptAES(this.EncryptionKey, this.EncryptionIV);

                result.WasEncrypted = true;                
            }


            //check if message is compressed
            var compressedMetadate = metadata.Any(x => x.Item1 == Key.Gzip) ? metadata.First(x => x.Item1 == Key.Gzip) : null;
            if (compressedMetadate != null)
            {
                int length;
                if (!int.TryParse(compressedMetadate.Item2, out length))
                {
                    logger.Error("Could not parse length from GZip metadate");
                    sendresult(Result.InvalidFormat, messageId);
                }

                result.WasCompressed = true;
                payload = payload.Take(length).ToArray<byte>().Decompress();
            }


            result.Payload = payload;

            //everything went okay => send delivery notifiaction
            sendresult(Result.Success, messageId);

            onMessageReceived(result);
        }

          
        /// <summary>
        /// Sends the specified result to the target client
        /// </summary>
        /// <param name="result"></param>
        /// <param name="messageId"></param>
        private void sendresult(Result result, int messageId= -1)
        {
            if(messageId < 0)
                return;

            string message = String.Format("result:{0};message_id:{1};", (int)result, messageId);

            MessagingClient.SendMessage(this.Recipient, message);

        }

        /// <summary>
        /// Handles the MessageReceived event from the underlying messaging client
        /// </summary>
        private void MessagingClient_MessageReceived(object sender, ObjectEventArgs<Message> e)
        {
            if (!e.Handled && e.Value.Sender.ToLower().StartsWith(this.Recipient.ToLower()))
            {
                parseMessage(e.Value.MessageBody);
                e.Handled = true;
            }
        }



        /// <summary>
        /// Raises the TransmissionTimedOutEvent
        /// </summary>
        private void onTransmissionTimedOut()
        {
            if (this.TransmissionTimedOut != null)
                this.TransmissionTimedOut(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the MessageReceived event
        /// </summary>
        /// <param name="message">The message that has been received</param>
        private void onMessageReceived(ParsingResult message)
        {
            if (this.MessageReceived != null)
                this.MessageReceived(this, message);
        }

        private void onMessageFragmentReceived()
        {
            if (this.MessageFragmentReceived != null)
            {
                this.MessageFragmentReceived(this, EventArgs.Empty);
            }
        }

        private enum Key
        { 
            Unknown,
            Message_id, 
            Result,
            Encryption, 
            Gzip,
            Fragment_Index,
            Fragment_Count
        }
    }

    /// <summary>
    /// List of possible results that can be sent back to a message's sender
    /// </summary>
    public enum Result
    {
        Success,
        Timeout, 
        UnknownError,
        SplittingError,
        EncryptionError,
        InvalidFormat
    }
}
