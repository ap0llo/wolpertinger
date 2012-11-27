using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolpertinger.Core
{
    [Component(ComponentNames.Authentication)]
    public class AuthenticationServerComponent : ServerComponent
    {

        [RemoteMethodCallHandler(AuthenticationMethods.EstablishConnection)]
        public CallResult EstablishConnection()
        { 
        }

        [RemoteMethodCallHandler(AuthenticationMethods.KeyExchange)]
        public CallResult KeyExchange(string publicKey, string iv)
        {
        }

        [RemoteMethodCallHandler(AuthenticationMethods.ClusterAuthGetToken)]
        public CallResult ClusterAuthGetToken()
        { 
        }

        [RemoteMethodCallHandler(AuthenticationMethods.ClusterAuthVerify)]
        public CallResult CluserAuthVerify(string verificationKey)
        {
        }

        [RemoteMethodCallHandler(AuthenticationMethods.ClusterAuthRequestVerification)]
        public CallResult ClusterAuthRequestVerification(string token)
        {
            AuthenticationClientComponent localClient = (AuthenticationClientComponent)ClientConnection.GetClientComponent(ComponentNames.Authentication);

            string key = AuthenticationStatic.GetClusterAuthKey(token);

            return new ResponseResult() { ResonseValue = key };
        }

        [RemoteMethodCallHandler(AuthenticationMethods.UserAuthGetToken)]
        public CallResult UserAuthGetToken()
        { 
        }

        [RemoteMethodCallHandler(AuthenticationMethods.UserAuthVerify)]
        public CallResult UserAuthVerify(string username, string userAuthKey)
        {
        }


    }
}
