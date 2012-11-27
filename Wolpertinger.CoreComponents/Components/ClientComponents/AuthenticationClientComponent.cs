using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.Core
{    
    [Component(ComponentNames.Authentication)]
    public class AuthenticationClientComponent : ClientComponent
    {

        public bool EstablishConnection()
        {
            return (bool)callRemoteMethod(AuthenticationMethods.EstablishConnection);
        }


        public string KeyExchange(string publicKey)
        {
            return (string)callRemoteMethod(AuthenticationMethods.KeyExchange, publicKey);
        }

        public string ClusterAuthGetToken()
        {
            return (string)callRemoteMethod(AuthenticationMethods.ClusterAuthGetToken);
        }


        public bool ClusterAuthVerify(string verificationKey)
        {
            return (bool)callRemoteMethod(AuthenticationMethods.ClusterAuthVerify, verificationKey);
        }


        public bool ClusterAuthRequestVerification()
        {
            string token = AuthenticationStatic.GetNewClusterAuthToken();

            string returnedKey = (string)callRemoteMethod(AuthenticationMethods.ClusterAuthRequestVerification, token);

            return AuthenticationStatic.CheckClusterAuthKey(returnedKey, token, ClientConnection.ConnectionManager.ClusterKey);
        }

        public string UserAuthGetToken()
        {
            return (string)callRemoteMethod(AuthenticationMethods.UserAuthGetToken);
        }

        public bool UserAuthVerify(string userAuthKey)
        {
            return (bool)callRemoteMethod(AuthenticationMethods.UserAuthVerify, userAuthKey);
        }


    }
}
