using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            Lexer lexer = new Lexer();
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
            Lexer lexer = new Lexer();

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
    }
}
