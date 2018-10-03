using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var parser = new Parser();
            parser.SetImplicit(RuleName.Whitespace);

            List<Lexeme> lexemes;

            parser.DefineRegex(RuleName.Identifier, @"\w+");

            lexemes = parser.Lex("a").ToList();
            Assert.AreEqual(lexemes.Count, 1);

            Assert.ThrowsException<Exception>(() =>
            {
                parser.Lex("a b c").ToList();
            });

            parser.DefineRegex(RuleName.Whitespace, @"\s+");

            lexemes = parser.Lex("a b c").ToList();
            Assert.AreEqual(lexemes.Count, 5);
        }

        [TestMethod]
        public void ExplicitWhitespace()
        {
            var parser = new Parser();

            List<Lexeme> lexemes;

            parser.DefineRegex(RuleName.Identifier, @"\w(?:\w|\d)*");

            lexemes = parser.Lex("a").ToList();
            Assert.AreEqual(lexemes.Count, 1);

            Assert.ThrowsException<Exception>(() =>
            {
                parser.Lex("a b c").ToList();
            });

            parser.DefineRegex(RuleName.Whitespace, @"\s+");

            lexemes = parser.Lex("a b c").ToList();
            Assert.AreEqual(lexemes.Count, 5);
        }

        [TestMethod]
        public void LexemeListComparison()
        {
            var a = Standard.Lexemes;

            var b = a
                .Select(l => new Lexeme(l.Token, l.MatchedText, l.StartIndex))
                .ToList();

            Assert.IsTrue(a.Equals(b));
        }

        [TestMethod]
        public void LexemeList()
        {
            var expected = Standard.Lexemes;

            var parser = Standard.ParserManager;

            var lexemes = parser.Lex(Standard.Text)
                .ToList();

            Assert.IsTrue(expected.Equals(lexemes));
        }
    }
}
