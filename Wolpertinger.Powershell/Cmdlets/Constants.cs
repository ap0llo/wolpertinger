using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wolpertinger.Powershell.Cmdlets
{
    /// <summary>
    /// Definitions of nouns used for Cmdlets
    /// </summary>
    static class Nouns
    {
        public const string ClientInfo = "ClientInfo";
        public const string User = "User";
        public const string Connection = "Connection";
        public const string Mount = "Mount";
        public const string Permission = "Permission";
        public const string Snapshot = "Snapshot";
        public const string Directory = "Directory";
        public const string File = "File";
        public const string RootDirectory = "RootDirectory";
        public const string WolpertingerVersion = "WolpertingerVersion";

        public const string XmppLogger = "XmppLogger";
        public const string XmppLoggerDebugLogging = "XmppLoggerDebugLogging";
        public const string XmppLoggerIsEnabled = "XmppLoggerIsEnabled";
        public const string XmppLoggerDebugLoggingIsEnabled = "XmppLoggerDebugLoggingIsEnabled";
        public const string XmppLoggerLogLevel = "XmppLoggerLogLevel";
        public const string XmppLoggerRecipient = "XmppLoggerRecipient";
    }



    static class ParameterSets
    {
        public const string FromConnection = "FromConnection";
        public const string FromDirectoryObject = "FromDirectoryObject";
        public const string FromOutFile = "FromOutFile";        
    }
}
