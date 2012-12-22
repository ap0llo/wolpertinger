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

namespace Wolpertinger.Core
{
    /// <summary>
    /// Names of buildt-in Components 
    /// </summary>
    public static partial class ComponentNamesExtended
    {
        public const string Core = "Core";
        public const string Authentication = "Authentication";
        public const string ClientInfoProvider = "ClientInfoProvider";
        public const string XmppLoggingConfigurator = "XmppLoggingConfigurator";
        public const string FileShare = "FileShare";
    }

    /// <summary>
    /// Names of RemoteMethods in the 'FileShare' component
    /// </summary>
    public static class FileShareMethods
    {
       public const string  AddSharedDirectory = "AddSharedDirectory";
       public const string  RemoveSharedDirectory = "RemoveSharedDirectory";
       public const string  GetDirectoryInfo = "GetDirectoryInfo";
       public const string  GetFileInfo = "GetFileInfo";
       public const string  GetMounts = "GetMounts";
       public const string  AddPermission = "AddPermission";
       public const string  GetPermission = "GetPermission";
       public const string  GetAddedPermissions = "GetAddedPermissions";
       public const string  RemovePermission = "RemovePermission";
       public const string  SetRootDirectoryPath = "SetRootDirectoryPath";
       public const string GetRootDirectoryPath = "GetRootDirectoryPath";
    }


    /// <summary>
    /// Names of RemoteMethods in the 'Authentication' component
    /// </summary>
    public static class AuthenticationMethods
    {
        public const string EstablishConnection = "EstablishConnection";
        public const string KeyExchange = "KeyExchange";
        public const string ClusterAuthGetToken = "ClusterAuthGetToken";
        public const string ClusterAuthVerify = "ClusterAuthVerify";
        public const string ClusterAuthRequestVerification = "ClusterAuthRequestVerification";
        public const string UserAuthGetToken = "UserAuthGetToken";
        public const string UserAuthVerify = "UserAuthVerify";
        public const string AwardTrustLevel = "AwardTrustLevel";
    }


    /// <summary>
    /// Names of RemoteMethods in the 'ClientInfoProvider' component
    /// </summary>
    public static class ClientInfoProviderMethods 
    {
        public const string GetClientInfo = "GetClientInfo";
        public const string GetKnownClients = "GetKnownClients";
    }

    /// <summary>
    /// Names of RemoteMethods in the 'XmppLoggingConfigurator' component
    /// </summary>
    public static class XmppLoggingConfiguratorMethods
    {
        public const string SetEnable = "SetEnable";
        public const string GetEnable = "GetEnable";
        public const string SetRecipient = "SetRecipient";
        public const string GetRecipient = "GetRecipient";
        public const string SetLogLevel = "SetLogLevel";
        public const string GetLogLevel = "GetLogLevel";
        public const string SetEnableDebugLogging = "SetEnableDebugLogging";
        public const string GetEnableDebugLogging = "GetEnableDebugLogging";
        public const string TestLogging = "TestLogging";
    }
}