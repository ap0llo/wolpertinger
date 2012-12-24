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
        /// <summary>
        /// Event that is raised each time a message has been received and parsed.
        /// </summary>
        event EventHandler<ObjectEventArgs<ParsingResult>> MessageReceived;

        /// <summary>
        /// Event that is raised when the connection timed out
        /// </summary>
        event EventHandler TransmissionTimedOut;


        /// <summary>
        /// The underlying messaging client used for sending and receiving messages
        /// </summary>
        IMessagingClient MessagingClient { get;  }
        
        /// <summary>
        /// The address of the WltpClient's target 
        /// </summary>
        string Recipient { get; }

        /// <summary>
        /// Specifies whether messages will be compressed before being sent
        /// </summary>
        bool CompressMessages { get; set; }

        /// <summary>
        /// Specifies whether messages will be encrypted before being sent
        /// </summary>
        bool EncryptMessages { get; set; }

        /// <summary>
        /// The key used for encryption
        /// </summary>
        byte[] EncryptionKey { get; set; }

        /// <summary>
        /// The Initialization Vector used for encryption
        /// </summary>
        byte[] EncryptionIV { get; set; }


        /// <summary>
        /// Sends the specified data to the target client specified in the 'Recipient' property
        /// </summary>
        /// <param name="message">The message to be sent</param>
        void Send(byte[] message);


        /// <summary>
        /// Processes the specified message
        /// </summary>
        /// <param name="message">The message to be processed</param>
        void HandleMessage(string message);

    }

    /// <summary>
    /// Encapsulates a received message along with additional information    
    /// </summary>
    public struct ParsingResult
    {
        /// <summary>
        /// Indicates whether the message was sent encrypted
        /// </summary>
        public bool WasEncrypted { get; set; }

        /// <summary>
        /// Indicates wheter the message was sent compressed
        /// </summary>
        public bool WasCompressed { get; set; }

        /// <summary>
        /// The message's payload
        /// </summary>
        public byte[] Payload { get; set; }
    }



}
