using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nerdcave.Common.Extensions;
using System.Security.Cryptography;

namespace Wolpertinger.Core
{
    /// <summary>
    /// Helper methods used by both AuthenticationClientComponent and AuthenticationServerComponent
    /// </summary>
    public class AuthenticationStatic
    {

        //protected ClientConnection clientConnection { get; set; }

        //public void EstablishConnectionAsync()
        //{
        //    callRemoteMethodAsync(AuthenticationMethods.EstablishConnection, "", true);
        //}

        //[TrustLevel(0)]
        //[RemoteMethodCallHandler(AuthenticationMethods.EstablishConnection)]
        //protected void incomingEstablishConnection(Guid callId)
        //{

        //    bool acceptConnection = (ClientConnection.AcceptIncomingConnectionsGlobal && AcceptIncomingConnectionsConnection);

        //    sendMethodResponse(callId, acceptConnection, "");

        //    clientConnection.Connected = true;
        //    TrustLevel = 1;
        //    raiseConnectionEstablished();
        //}

        //[RemoteMethodResponseHandler(AuthenticationMethods.EstablishConnection)]
        //protected void responseHandlerEstablishConnection(Guid callId, bool response)
        //{
        //    if (response)
        //    {
        //        Connected = true;
        //        raiseConnectionEstablished();
        //        //this.connectionResetRemote += new EventHandler<ObjectEventArgs>(ClientConnection_connectionResetRemote);
        //    }
        //}




        /// <summary>
        /// Generates a ClusterAuthKey based on the specified authToken and the CluserKey it gets from ConnectionManager
        /// </summary>
        /// <param name="authToken">The authToken to use to derive a AuthKey</param>
        /// <returns></returns>
        public static string GetClusterAuthKey(string authToken, byte[] clusterKey)
        {
            string key = clusterKey.ToHexString();

            if (authToken.IsNullOrEmpty() || key.IsNullOrEmpty())
            {
                return "";
            }
            else
            {
                return (key + authToken).GetHashSHA1();
            }

        }

        public static string GetUserAuthKey(string authToken, string password)
        {
            if (authToken.IsNullOrEmpty() || password.IsNullOrEmpty())
            {
                return "";
            }
            else
            {
                return (authToken + password).GetHashSHA1();
            }
        }

        public static bool CheckClusterAuthKey(string authKey, string issuedAuthToken, byte[] clusterKey)
        {
            string myAuthKey = GetClusterAuthKey(issuedAuthToken, clusterKey);

            if (myAuthKey.IsNullOrEmpty() || authKey.IsNullOrEmpty())
            {
                return false;
            }
            else
            {
                return (myAuthKey == authKey);
            }
        }

        public static bool CheckUserAuthKey(string authKey, string issuedUserAuthToken)
        {

            string myAuthKey = GetUserAuthKey(issuedUserAuthToken, ConnectionManager.GetConnectionManager().AdminPassword);

            if (myAuthKey.IsNullOrEmpty() || authKey.IsNullOrEmpty())
            {
                return false;
            }
            else
            {
                return (myAuthKey == authKey);
            }


        }


        public static string GetNewClusterAuthToken()
        {
            //TODO
            throw new NotImplementedException();
        }


        public static string GetNewUserAuthToken()
        {
            //TODO
            throw new NotImplementedException();
        }


        public static ECDiffieHellmanCng GetNewKeyProvider()
        {
            ECDiffieHellmanCng keyProvider = new ECDiffieHellmanCng();
            keyProvider.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
            keyProvider.HashAlgorithm = CngAlgorithm.Sha256;

            return keyProvider;
        }


    }
}
