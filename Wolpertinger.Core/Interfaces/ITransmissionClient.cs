using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nerdcave.Common;

namespace Wolpertinger.Core
{
    public interface ITransmissionClient
    {

        event EventHandler<ObjectEventArgs<TransmissionResult>> MessageReceived;

        event EventHandler TransmissionTimedOut;



        IChannel Channel { get; private set; }

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

    public struct TransmissionResult
    {
        public bool WasEncrypted { get; set; }

        public bool WasCompressed { get; set; }

        public byte[] Payload { get; set; }
    }



}
