﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using PseudoEBNF;
using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class Lexing
    {
        [TestMethod]
        public void ImplicitWhitespace()
        {
            Parser parser;
            List<Lexeme> lexemes;
            
            parser = new Parser();
            parser.SetImplicit(RuleName.Whitespace);

            parser.DefineRegex(RuleName.Identifier, @"\w+");

            parser.Lock();

            lexemes = parser.Lex("a").ToList();
            Assert.AreEqual(lexemes.Count, 1);

            Assert.ThrowsException<Exception>(() =>
            {
                parser.Lex("a b c").ToList();
            });

            parser = new Parser();
            parser.SetImplicit(RuleName.Whitespace);

            parser.DefineRegex(RuleName.Identifier, @"\w+");
            parser.DefineRegex(RuleName.Whitespace, @"\s+");

            parser.Lock();

            lexemes = parser.Lex("a b c").ToList();
            Assert.AreEqual(lexemes.Count, 5);
        }

        [TestMethod]
        public void ExplicitWhitespace()
        {
            Parser parser;

            List<Lexeme> lexemes;

            parser = new Parser();

            parser.DefineRegex(RuleName.Identifier, @"\w(?:\w|\d)*");

            parser.Lock();

            lexemes = parser.Lex("a").ToList();
            Assert.AreEqual(lexemes.Count, 1);

            Assert.ThrowsException<Exception>(() =>
            {
                parser.Lex("a b c").ToList();
            });

            parser = new Parser();
            
            parser.DefineRegex(RuleName.Identifier, @"\w(?:\w|\d)*");
            parser.DefineRegex(RuleName.Whitespace, @"\s+");

            parser.Lock();

            lexemes = parser.Lex("a b c").ToList();
            Assert.AreEqual(lexemes.Count, 5);
        }

        [TestMethod]
        public void LexemeListComparison()
        {
            var a = Standard.GetLexemes();

            var b = a
                .Select(l => new Lexeme(l.CompatibilityGuid, l.Token, l.MatchedText, l.StartIndex))
                .ToList();

            Assert.IsTrue(a.Equals(b));
        }

        [TestMethod]
        public void LexemeList()
        {
            var parser = Standard.GetParser();
            var expected = Standard.GetLexemes(parser);
            
            parser.Lock();

            var lexemes = parser.Lex(Standard.Text)
                .ToList();

            Assert.IsTrue(expected.Equals(lexemes));
        }
    }
}
