using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nerdcave.Common.Xml;
using Wolpertinger.FileShareCommon;
using System.IO;
using System.Xml.Linq;

namespace Wolpertinger.Testing
{
    /// <summary>
    /// Tests for testing interop with alternate wolpertinger implementation
    /// </summary>
    [TestClass]    
    public class InteropTests
    {
        /*
         * Temporarily deactivated tests
         */


        //[TestInitialize]
        //public void Initialize()
        //{
        //    XmlSerializer.RegisterType(typeof(DirectoryObject), "directoryObject");
        //}

        //[TestMethod]
        //public void DeserializationTest1()
        //{
        //    string input = File.ReadAllText("../f.xml");
        //    var xml = XElement.Parse(input);

        //    var dir = XmlSerializer.Deserialize(xml) as DirectoryObject;
        //}
    }
}
