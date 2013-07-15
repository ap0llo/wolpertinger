using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wolpertinger.Core;
using Wolpertinger.Powershell.Init;

namespace Wolpertinger.Powershell
{
    static class Program
    {
        private static IConnectionManager _connectionManager;
        private static bool _initialized = false;

        public static readonly string APPDATAFOLDER = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Wolpertinger", "Manager");

        
        public static IConnectionManager ConnectionManager
        {
            get
            {
                if (!_initialized)
                {
                    _initialized = true;
                    init();
                }
                return _connectionManager;
            }
            set
            {
                _connectionManager = value;
            }
        }




        private static void init()
        {
            AppDataInit.Initialize();
            ComponentFactoryInit.Initialize();
            XmlSerializerInit.Initialize();

            _connectionManager = new DefaultConnectionManager();            
            _connectionManager.AcceptIncomingConnections = false;
            _connectionManager.LoadSettings(APPDATAFOLDER);

            XmppLoggerInit.Initialze();
        }




    }
}
