using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PseudoEBNF.Common;
using PseudoEBNF.Lexing;

namespace Tests
{
    [TestClass]
    public class Lexing
    {
        //*
        [TestMethod]
        public void ImplicitWhitespace()
        {
            List<Lexeme> lexemes;
            Lexer lexer;

            lexer = new Lexer();

            lexer.DefineRegex(RuleName.Identifier, @"\w+");

            lexemes = lexer.Lex("a").ToList();
            Assert.AreEqual(lexemes.Count, 1);

            Assert.ThrowsException<Exception>(() =>
            {
                lexer.Lex("a b c").ToList();
            });

            lexer = new Lexer();
            lexer.SetImplicit(RuleName.Whitespace);

            lexer.DefineRegex(RuleName.Identifier, @"\w+");
            lexer.DefineRegex(RuleName.Whitespace, @"\s+");

            lexemes = lexer.Lex("a b c").ToList();
            Assert.AreEqual(lexemes.Count, 5);
        }

        [TestMethod]
        public void ExplicitWhitespace()
        {
            Lexer lexer;

            List<Lexeme> lexemes;

            lexer = new Lexer();

            lexer.DefineRegex(RuleName.Identifier, @"\w(?:\w|\d)*");

            lexemes = lexer.Lex("a").ToList();
            Assert.AreEqual(lexemes.Count, 1);

            Assert.ThrowsException<Exception>(() =>
            {
                lexer.Lex("a b c").ToList();
            });

            lexer = new Lexer();

            lexer.DefineRegex(RuleName.Identifier, @"\w(?:\w|\d)*");
            lexer.DefineRegex(RuleName.Whitespace, @"\s+");

            lexemes = lexer.Lex("a b c").ToList();
            Assert.AreEqual(lexemes.Count, 5);
        }

        [TestMethod]
        public void LexemeListComparison()
        {
            LexemeList a = Standard.GetLexemes();

            var b = a
                .Select(l => new Lexeme(l, l.Token, l.MatchedText, l.StartIndex))
                .ToList();

            Assert.IsTrue(a.Equals(b));
        }

        [TestMethod]
        public void LexemeList()
        {
            Lexer lexer = Standard.GetLexer();
            LexemeList expected = Standard.GetLexemes(lexer);

            var lexemes = lexer.Lex(Standard.Text)
                .ToList();

            Assert.IsTrue(expected.Equals(lexemes));
        }
        //*/
    }
}
