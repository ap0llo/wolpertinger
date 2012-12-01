using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nerdcave.Common;
using Nerdcave.Common.Extensions;
using Slf;
using System.Threading;

namespace Wolpertinger.Core
{
    public class DefaultTransmissionClient : ITransmissionClient
    {


        private ILogger logger = LoggerService.GetLogger("DefaultTransmissionClient");

        private Dictionary<int, EventWaitHandle> waitingThreads = new Dictionary<int, EventWaitHandle>();
        private Dictionary<int, Result> messageResults = new Dictionary<int, Result>(); 

        public event EventHandler<ObjectEventArgs<TransmissionResult>> MessageReceived;

        public event EventHandler TransmissionTimedOut;


        public DefaultTransmissionClient(IConnectionManager connectionManager, string recipient)
        {
            this.Recipient = recipient;
            this.ConnectionManager = connectionManager;
            
        }

        //void Channel_MessageRecieved(object sender, ObjectEventArgs<ReceivedMessage> e)
        //{
        //    if (!e.Handled && e.Value.Sender == this.Recipient)
        //    {
        //        parseMessage(e.Value.Message);
        //    }
        //}


        public IConnectionManager ConnectionManager { get; private set; }

        public string Recipient { get; private set; }

        public bool CompressMessages { get; set; }

        public bool EncryptMessages { get; set; }

        public byte[] EncryptionKey { get; set; }

        public byte[] EncryptionIV { get; set; }


        public void Send(byte[] message)
        {
            throw new NotImplementedException();
        }




        private void parseMessage(string message)
        {
            //check if message has body
            if (message.IsNullOrEmpty())
            {
                logger.Info("Received empty message. Message will be ignored");
                return;
            }


            //check if message format is valid
            int count = message.Count(x => x == ';');
            if (!count.IsEven())
            {
                logger.Warn("Received invalid message: Semicolon count uneven");
                sendresult(Result.InvalidFormat);
                return;
            }

            IEnumerable<string> parts = message.Split(';');

            string payload_str =  (parts.Count() == count) ?  null : parts.Last();
            parts = (parts.Count() == count) ? parts : parts.Take(parts.Count() - 1);
            
            //check if all metadates have a valid format
            if(parts.Any(x=> !x.Contains(':')))
            {
                logger.Warn("Received invlaid message: Invalid metadate format");
                sendresult(Result.InvalidFormat);
                return;
            }


            //parse metadata keys
            Key parsedKey;
            var metadata = parts.Select(x => new Tuple<string, string>(x.Split(':').First(), x.Split(':').Last()))
                .Select(x => Enum.TryParse(x.Item1.ToLower(),true, out parsedKey) 
                            ? new Tuple<Key, string>(parsedKey, x.Item2) 
                            : new Tuple<Key, string>(Key.Unknown, x.Item2));

            //check for unknown keys
            if (metadata.Any(x => x.Item1 == Key.Unknown))
            {
                logger.Warn("Cannot parse message. Unknwon key encountered");
                sendresult(Result.UnknownKey);
                return;
            }

            //check for key duplicates
            if (metadata.Any(x => metadata.Count(y => y.Item1 == x.Item1) > 1))
            {
                logger.Warn("Cannot parse message. Duplicate Key");
                sendresult(Result.DuplicateKey);
                return;
            }            

            //check for message id
            var messageIdQuery = metadata.Where(x => x.Item1 == Key.Message_id);
            int messageId = -1;
            if (!messageIdQuery.Any() || !int.TryParse(messageIdQuery.First().Item2, out messageId))
            {
                logger.Warn("Invalid message. MessageId not found or invalid");
                sendresult(Result.InvalidMessageId);
                return;
            }

            //check if message is a delivery notification
            var resultQuery = metadata.Where(x => x.Item1 == Key.Result);
            if (metadata.Count() == 2 && messageId > 0 && resultQuery.Any())
            {
                //handle delivery confirmation
                

            }


            //check for splitted messages

                //  TODO

            TransmissionResult result = new TransmissionResult();
            
            //check if message is encrypted
            var encryptedMetaDate = metadata.Any(x => x.Item1 == Key.Encryption) ? metadata.First(x => x.Item1 == Key.Encryption) : null;

            if (encryptedMetaDate != null)
            {
                //check if encryption algorith is supported
                if (encryptedMetaDate.Item2.ToLower() != "aes")
                {
                    logger.Warn("Encryption algorithm not supported");
                    sendresult(Result.EncryptionMethodNotSupported);
                    return;
                }


                result.WasEncrypted = true;
            }



        }


        private void sendresult(Result result, int messageId= -1)
        {
            throw new NotImplementedException();
        }

        


        private void onTransmissionTimedOut()
        {
            if (this.TransmissionTimedOut != null)
                this.TransmissionTimedOut(this, EventArgs.Empty);
        }

        private void onMessageReceived(TransmissionResult message)
        {
            if (this.MessageReceived != null)
                this.MessageReceived(this, message);
        }


        private enum Key
        { 
            Unknown,
            Message_id, 
            Result,
            Encryption, 
            Fragment_Index,
            Fragment_Count
        }
    }


    public enum Result
    {
        Unknown,
        Successful,
        InvalidFormat,
        UnknownKey,
        DuplicateKey,
        InvalidMessageId,
        EncryptionMethodNotSupported,
        
    }
}
