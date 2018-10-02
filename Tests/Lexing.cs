using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var lexer = new Lexer();
            lexer.SetImplicit(RuleName.Whitespace);

            List<Lexeme> lexemes;

            lexer.DefineRegex(RuleName.Identifier, @"\w+");

            lexemes = lexer.Lex("a").ToList();
            Assert.AreEqual(lexemes.Count, 1);

            Assert.ThrowsException<Exception>(() =>
            {
                lexer.Lex("a b c").ToList();
            });

            lexer.DefineRegex(RuleName.Whitespace, @"\s+");

            lexemes = lexer.Lex("a b c").ToList();
            Assert.AreEqual(lexemes.Count, 5);
        }

        [TestMethod]
        public void ExplicitWhitespace()
        {
            var lexer = new Lexer();

            List<Lexeme> lexemes;

            lexer.DefineRegex(RuleName.Identifier, @"\w(?:\w|\d)*");

            lexemes = lexer.Lex("a").ToList();
            Assert.AreEqual(lexemes.Count, 1);

            Assert.ThrowsException<Exception>(() =>
            {
                lexer.Lex("a b c").ToList();
            });

            lexer.DefineRegex(RuleName.Whitespace, @"\s+");

            lexemes = lexer.Lex("a b c").ToList();
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
            var standard = Standard.Lexemes;

            var lexer = new Lexer(Standard.Grammar);

            var lexemes = lexer.Lex(Standard.Text)
                .ToList();

            Assert.IsTrue(standard.Equals(lexemes));
        }
    }
}
