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
using System.Security.Cryptography;
using Nerdcave.Common.Extensions;
using System.Security;

namespace Wolpertinger.Core
{    
    /// <summary>
    /// Combined client- and server-implementation of the Authentication component.
    /// See Wolpertinger API Documentation for details on the component.
    /// </summary>
    [Component(ComponentNamesExtended.Authentication, ComponentType.ClientServer)]
    public class AuthenticationComponent : ClientComponent
    {

        #region Fields

        private ECDiffieHellmanCng keyProvider = null;
        
        private string clusterAuthToken = null;
        private bool clusterVerified_target = false;
        private bool clusterVerified_me = false;
        private string userAuthToken = null;

        #endregion Fiels



        #region Client Implementation

        /// <summary>
        /// Calls the EstablishConnection RemoteMethod (Blocking)
        /// </summary>
        public bool EstablishConnection()
        {

            bool accepted =  (bool)callRemoteMethod(AuthenticationMethods.EstablishConnection);
            this.ClientConnection.Connected = accepted;
            return accepted;
        }

        /// <summary>
        /// Calls the KeyExchange RemoteMethod (Blocking)
        /// </summary>
        public void KeyExchange()
        {
            keyProvider = getNewKeyProvider();

            this.ClientConnection.WtlpClient.EncryptionIV = new AesCryptoServiceProvider().IV;

            string response = (string)callRemoteMethod( AuthenticationMethods.KeyExchange, 
                                                        keyProvider.PublicKey.ToByteArray().ToStringBase64(),
                                                        this.ClientConnection.WtlpClient.EncryptionIV.ToStringBase64());

            ECDiffieHellmanPublicKey otherKey = ECDiffieHellmanCngPublicKey.FromByteArray(response.GetBytesBase64(), CngKeyBlobFormat.EccPublicBlob);

            this.ClientConnection.WtlpClient.EncryptionKey = keyProvider.DeriveKeyMaterial(otherKey);
            this.ClientConnection.TrustLevel = 2;
            AwardTrustLevel(2);

            keyProvider = null;            
        }

        /// <summary>
        /// Gets a new cluster authentication token from the target client by calling the ClusterAuthGetToken RemoteMethod (Blocking)
        /// </summary>
        /// <returns>Returns the token returned by the target</returns>
        public string ClusterAuthGetToken()
        {
            string response = (string)callRemoteMethod(AuthenticationMethods.ClusterAuthGetToken);
            return response;

        }

        /// <summary>
        /// Generates a cluster authentication key and sends it to the target client to be verified. 
        /// Calls the ClusterAuthVerify RemoteMethod (Blocking)
        /// </summary>
        /// <param name="authToken">The token to use for verification (has to have been request from the target)</param>
        /// <returns>Returns whether or not the target client has accepted the cluster membership</returns>
        public bool ClusterAuthVerify(string authToken)
        {
            clusterVerified_me = (bool)callRemoteMethod(AuthenticationMethods.ClusterAuthVerify, calculateClusterAuthKey(authToken, this.ClientConnection.ConnectionManager.ClusterKey));
                        
            return clusterVerified_me;
        }


        /// <summary>
        /// Sends a cluster authentication token to the target client and makes the target verify it's cluster membership
        /// Calls the ClusterAuthRequestVerification RemoteMethod (Blocking)
        /// </summary>
        /// <returns>Returns whether the target's cluster membership could be verified</returns>
        public bool ClusterAuthRequestVerification()
        {
            string token = getNewAuthToken();
            clusterAuthToken = token;

            bool verified = (bool)callRemoteMethod(AuthenticationMethods.ClusterAuthRequestVerification, token);

            return verified;
        }

        /// <summary>
        /// Gets a user-authentication token from the target.
        /// Calls the UserAuthGetToken RemoteMethod (Blocking)
        /// </summary>
        /// <returns>Returns the token recieved from the target</returns>
        public string UserAuthGetToken()
        {
            return (string)callRemoteMethod(AuthenticationMethods.UserAuthGetToken);
        }

        /// <summary>
        /// Verifies this client's cluster membership by calculating a authentication key based on the specified token and sending it to the target.
        /// Calls the UserAuthVerify RemoteMethod (Blocking)
        /// </summary>
        /// <param name="authToken">The authentication token received by the target</param>
        /// <returns>Returns whether this client was verified or not</returns>
        public bool UserAuthVerify(string username, string authToken, SecureString password)
        {
            string authKey = calculateUserAuthKey(authToken, password);

            return (bool)callRemoteMethod(AuthenticationMethods.UserAuthVerify, username, authKey);
        }

        /// <summary>
        /// Sets a new TrustLevel for the target client and notifies the target about the change.
        /// Asynchronously calls the AwardTrustLevel RemoteMethod.
        /// </summary>
        /// <param name="trustLevel">The level of trust to award</param>
        public void AwardTrustLevel(int trustLevel)
        {
            this.ClientConnection.TrustLevel = trustLevel;
            callRemoteMethodAsync(AuthenticationMethods.AwardTrustLevel, false, trustLevel);
        }


        #endregion Client Implementation



        #region Server Implementation


        /// <summary>
        /// Server implementation of the EstablishConnection RemoteMethod
        /// </summary>        
        [MethodCallHandler(AuthenticationMethods.EstablishConnection), TrustLevel(0)]
        public CallResult EstablishConnection_server()
        {
            //Check if connection is to be accepted
            bool accepted = (this.ClientConnection.AcceptConnections && ClientConnection.ConnectionManager.AcceptIncomingConnections);

            if (accepted)
            {
                AwardTrustLevel(1);
                this.ClientConnection.Connected = true;
            }

            return new ResponseResult(accepted);
        }

        /// <summary>
        /// Server implementation of the KeyExchange RemoteMethod
        /// </summary>
        [MethodCallHandler(AuthenticationMethods.KeyExchange), TrustLevel(1)]
        public CallResult KeyExchange_server(string publicKey, string iv)
        {
            //Initialize a new key provides
            this.keyProvider = AuthenticationComponent.getNewKeyProvider();
            ECDiffieHellmanPublicKey otherKey = ECDiffieHellmanCngPublicKey.FromByteArray(publicKey.GetBytesBase64(), CngKeyBlobFormat.EccPublicBlob);

            //derive connection key from target's public key
            ClientConnection.WtlpClient.EncryptionKey = keyProvider.DeriveKeyMaterial(otherKey);
            ClientConnection.WtlpClient.EncryptionIV = iv.GetBytesBase64();

            //Increase Trust level (connection is now encrypted)
            AwardTrustLevel(2);

//            ClientConnection.MessageProcessor.EncryptMessages = true;

            //Send back our own public key so target can derive connection key, too
            CallResult result = new ResponseResult(keyProvider.PublicKey.ToByteArray().ToStringBase64());
            
            //Reset key provider
            keyProvider = null;

            return result;

        }

        /// <summary>
        /// Server implementation of the ClusterAuthGetToken RemoteMethod
        /// </summary>
        [MethodCallHandler(AuthenticationMethods.ClusterAuthGetToken), TrustLevel(2)]
        public CallResult ClusterAuthGetToken_server()
        {
            //generate new cluster authentication token and save it (necessary in ClusterAuthVerify())
            clusterAuthToken = getNewAuthToken();

            return new ResponseResult(clusterAuthToken);
        }

        /// <summary>
        /// Server implementation of the ClusterAuthVerify RemoteMethod
        /// </summary>
        [MethodCallHandler(AuthenticationMethods.ClusterAuthVerify), TrustLevel(2)]
        public CallResult CluserAuthVerify_server(string verificationKey)
        {
            //check if cluserAuthToken has been requested before calling this method
            if (String.IsNullOrEmpty(clusterAuthToken))
            {                
                this.ClientConnection.ResetConnection();
                return new ErrorResult(RemoteErrorCode.InvalidParametersError);
            }
            //check if verification key even has a value
            else if (verificationKey.IsNullOrEmpty())
            {
                return new ErrorResult(RemoteErrorCode.InvalidParametersError);
            }
            else
            {
                //check if verification key is valied
                clusterVerified_target = checkClusterAuthKey(verificationKey, clusterAuthToken, this.ClientConnection.ConnectionManager.ClusterKey);                

                //Increase trust level if target has been verified
                if (clusterVerified_target)
                    AwardTrustLevel(3);
                else
                    this.ClientConnection.ResetConnection();
                
                return new ResponseResult(clusterVerified_target);
            }
        }

        /// <summary>
        /// Server implementation of the ClusterAuthRequestVerification RemoteMethod
        /// </summary>
        [MethodCallHandler(AuthenticationMethods.ClusterAuthRequestVerification), TrustLevel(2)]
        public CallResult ClusterAuthRequestVerification_server(string token)
        {
            //check even has a value
            if (token.IsNullOrEmpty())
                return new ErrorResult(RemoteErrorCode.InvalidParametersError);

            //Generate a ClusterAuthKey based on the token and send it back
            bool verified = ClusterAuthVerify(token);

            return new ResponseResult(verified);
        }

        /// <summary>
        /// Server implementation of the UserAuthGetToken RemoteMethod
        /// </summary>
        [MethodCallHandler(AuthenticationMethods.UserAuthGetToken), TrustLevel(3)]
        public CallResult UserAuthGetToken_server()
        {
            //Generate new authToken and save it so it can be checked in UserAuthVerify()
            userAuthToken = getNewAuthToken();

            return new ResponseResult(userAuthToken);
        }

        /// <summary>
        /// Server implementation of the UserAuthVerify RemoteMethod
        /// </summary>
        [MethodCallHandler(AuthenticationMethods.UserAuthVerify), TrustLevel(3)]
        public CallResult UserAuthVerify_server(string username, string userAuthKey)
        {
            //check if username and authkey (derived from password) are correct
            bool verified = ((username.ToLower() == this.ClientConnection.ConnectionManager.WolpertingerUsername.ToLower()) 
                                && checkUserAuthKey(userAuthKey, userAuthToken, this.ClientConnection.ConnectionManager.WolpertingerPassword));
            
            //delete issued userAuthToken (token may only be used once)
            userAuthToken = null;


            ResponseResult result = new ResponseResult();

            //increase Trust level if credentilas were correct
            if (verified)
            {
                this.ClientConnection.TrustLevel = 4;
                result.PostProcessingAction = new Action(delegate { AwardTrustLevel(4); });
            }
            else
            {
                result.PostProcessingAction = new Action(delegate { this.ClientConnection.ResetConnection(true); });                
            }

            result.ResponseValue = verified;

            //Return if client was verified or not
            return result;
        }

        /// <summary>
        /// Server implementation of the AwardTrustLevel RemoteMethod
        /// </summary>
        [MethodCallHandler(AuthenticationMethods.AwardTrustLevel), TrustLevel(0)]
        public CallResult AwardTrustLevel_server(int trustLevel)
        {
            this.ClientConnection.MyTrustLevel = trustLevel;
            if (trustLevel == 2)
                this.ClientConnection.WtlpClient.EncryptMessages = true;
            
            return new VoidResult();
        }

        #endregion Server Implementation


        #region Private/Protected Methods


        /// <summary>
        /// Initializes a new Instance of ECDiffieHellmanCng and sets the default values for use for the key-exchange
        /// </summary>
        /// <returns>Retunrs the initialized instance of ECDiffieHellmanCng</returns>
        protected static ECDiffieHellmanCng getNewKeyProvider()
        {
            ECDiffieHellmanCng keyProvider = new ECDiffieHellmanCng();
            keyProvider.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
            keyProvider.HashAlgorithm = CngAlgorithm.Sha256;

            return keyProvider;
        }


        /// <summary>
        /// Calculates a cluster-authentication key using the specified token and password
        /// </summary>
        /// <param name="authToken">The authentication token for calculating the authentication key</param>
        /// <param name="clusterKey">The cluster-key for calculating the authentication key</param>
        /// <returns>Retunrs a cluster authentication key based on the specified token and cluster-key</returns>
        protected static string calculateClusterAuthKey(string authToken, byte[] clusterKey)
        {
            byte[] salt = authToken.GetBytesBase64();
            string key = clusterKey.ToHexString();

            if (salt == null || key.IsNullOrEmpty())
            {
                return "";
            }
            else
            {
                return getSaltedHashString(key, salt);
            }

        }

        /// <summary>
        /// Checks if the given authentication key is valid for the specified token and cluser-key
        /// </summary>
        /// <param name="authKey">The authentication key to be checked</param>
        /// <param name="issuedAuthToken">The authentication token used to calculate the key</param>
        /// <param name="clusterKey">The cluser-key to calculate the key</param>
        /// <returns>Returns whether the specified authentication key was correct for the given token and cluster-key</returns>
        protected static bool checkClusterAuthKey(string authKey, string issuedAuthToken, byte[] clusterKey)
        {
            string myAuthKey = calculateClusterAuthKey(issuedAuthToken, clusterKey);

            if (myAuthKey.IsNullOrEmpty() || authKey.IsNullOrEmpty())
            {
                return false;
            }
            else
            {
                return (myAuthKey == authKey);
            }
        }

        /// <summary>
        /// Generates a new authentication token to be used with cluser- or user-authentication.
        /// </summary>
        /// <returns>Returns the generated token. The token actually is a 32-byte value base64-encoded as string</returns>
        protected static string getNewAuthToken()
        {
            byte[] saltArray = new byte[32];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(saltArray);

            return saltArray.ToStringBase64();
        }

        /// <summary>
        /// Calculates a user-authentication key using the specified token and password
        /// </summary>
        /// <param name="authToken">The token to calculate the authentication key</param>
        /// <param name="password">The password for calculating the authentication key</param>
        /// <returns>Returns a user authentication key based on the specified token and password</returns>
        protected static string calculateUserAuthKey(string authToken, SecureString password)
        {
            //get the salt as bytes from the token
            byte[] salt = authToken.GetBytesBase64();

            //check if salt ans password have valid values
            if (salt == null || password.IsNullOrEmpty())
            {
                return "";
            }
            //calculate key
            else
            {
                return getSaltedHashString(password.Unwrap(), salt);
            }
        }

        /// <summary>
        /// Checks if the given authentication key is valid for the specified auth-token and password
        /// </summary>
        /// <param name="authKey">The authentication-key to be checked</param>
        /// <param name="issuedUserAuthToken">The authentication token used to calculate the key</param>
        /// <param name="password">The password used to calculate the key</param>
        /// <returns>Returns whether the specified authentication key was correct for the given token and password</returns>
        protected static bool checkUserAuthKey(string authKey, string issuedUserAuthToken, SecureString password)
        {
            //calculate key based on the auth-token and the password
            string myAuthKey = calculateUserAuthKey(issuedUserAuthToken, password);

            if (myAuthKey.IsNullOrEmpty() || authKey.IsNullOrEmpty())
            {
                return false;
            }
            else
            {
                //compare the just calculated key with the one passed in as parameter
                return (myAuthKey == authKey);
            }
        }

        /// <summary>
        /// Calculates a salted 64-byte hash value encoded as base64-string using PBKDF2
        /// </summary>
        /// <param name="value">The value to be hashed</param>
        /// <param name="salt">The salt for calculating the hash</param>
        /// <returns>Returns a hash of the specified value as base64-encoded string</returns>
        protected static string getSaltedHashString(string value, byte[] salt)
        {
            //initialize new instace of Rfc2898DeriveBytes (implements salted hashing using PBKDF2) 
            Rfc2898DeriveBytes hashProvider = new Rfc2898DeriveBytes(value, salt);
            //get the first 64 bytes of the salted hash
            return hashProvider.GetBytes(64).ToStringBase64();       
        }


        #endregion Private/Protected Methods

    }

}
