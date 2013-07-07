﻿/*

Licensed under the new BSD-License
 
Copyright (c) 2011-2013, Andreas Grünwald 
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

    Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
    Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer 
	in the documentation and/or other materials provided with the distribution.
    Neither the name of the Wolpertinger project nor the names of its contributors may be used to endorse or promote products 
	derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS 
BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, 
STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommandLineParser.ParameterParsers;
using CommandLineParser.Interfaces;

namespace Wolpertinger.Testing.CommandLineParsers
{
    [TestClass]
    public class BoolParserTests
    {

        IParameterParser boolParser = new BoolParser();




        [TestMethod]
        public void TestCanParse()
        {
            Assert.IsTrue(boolParser.CanParse("true"));
            Assert.IsTrue(boolParser.CanParse("TRUE"));
            Assert.IsTrue(boolParser.CanParse("True"));
            Assert.IsTrue(boolParser.CanParse("tRue"));
            Assert.IsTrue(boolParser.CanParse("trUe"));
            Assert.IsTrue(boolParser.CanParse("truE"));

            Assert.IsTrue(boolParser.CanParse("TRue"));
            Assert.IsTrue(boolParser.CanParse("TrUe"));
            Assert.IsTrue(boolParser.CanParse("TruE"));
            Assert.IsTrue(boolParser.CanParse("tRuE"));
            Assert.IsTrue(boolParser.CanParse("tRuE"));


            Assert.IsTrue(boolParser.CanParse("false"));
            Assert.IsTrue(boolParser.CanParse("False"));
            Assert.IsTrue(boolParser.CanParse("fAlse"));
            Assert.IsTrue(boolParser.CanParse("faLse"));
            Assert.IsTrue(boolParser.CanParse("falSe"));
            Assert.IsTrue(boolParser.CanParse("falsE"));
            Assert.IsTrue(boolParser.CanParse("FAlse"));
            Assert.IsTrue(boolParser.CanParse("FALSE"));

            Assert.IsTrue(boolParser.CanParse("0"));
            Assert.IsTrue(boolParser.CanParse("1"));


            Assert.IsFalse(boolParser.CanParse(""));
            Assert.IsFalse(boolParser.CanParse(null));
            Assert.IsFalse(boolParser.CanParse("5"));
            Assert.IsFalse(boolParser.CanParse("trueFalse"));
            Assert.IsFalse(boolParser.CanParse("00"));
        }
    }
}
