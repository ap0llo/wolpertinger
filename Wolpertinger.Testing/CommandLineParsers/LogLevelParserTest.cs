using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommandLineParser.ParameterParsers;
using CommandLineParser.Interfaces;


namespace Wolpertinger.Testing.CommandLineParsers
{
    [TestClass]
    public class LogLevelParserTest
    {
        IParameterParser parser = new LogLevelParser();


        [TestMethod]
        public void Test_CanParse()
        {
            Assert.IsTrue(parser.CanParse(Core.LogLevel.None.ToString()));
            Assert.IsTrue(parser.CanParse(Core.LogLevel.Info.ToString()));
            Assert.IsTrue(parser.CanParse(Core.LogLevel.Warn.ToString()));
            Assert.IsTrue(parser.CanParse(Core.LogLevel.Error.ToString()));
            Assert.IsTrue(parser.CanParse(Core.LogLevel.Fatal.ToString()));


            Assert.IsFalse(parser.CanParse("WarnWarn"));
            Assert.IsFalse(parser.CanParse(""));
            Assert.IsFalse(parser.CanParse(null));

        }
    }
}
