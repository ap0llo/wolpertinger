using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nerdcave.Common;

namespace Wolpertinger.Core
{
    /// <summary>
    /// A Client for the Wolpertinger Transmission Layer Protocol
    /// </summary>
    public interface IWtlpClient
    {

        event EventHandler<ObjectEventArgs<ParsingResult>> MessageReceived;

        event EventHandler TransmissionTimedOut;



        IMessagingClient MessagingClient { get; set; }
        
        string Recipient { get; private set; }

        bool CompressMessages { get; set; }

        bool EncryptMessages { get; set; }

        /// <summary>
        /// The key used for encryption
        /// </summary>
        byte[] EncryptionKey { get; set; }

        /// <summary>
        /// The Initialization Vector used for encryption
        /// </summary>
        byte[] EncryptionIV { get; set; }



        void Send(byte[] message);
       
    }


    public struct ParsingResult
    {
        public bool WasEncrypted { get; set; }

        public bool WasCompressed { get; set; }

        public byte[] Payload { get; set; }
    }



}
