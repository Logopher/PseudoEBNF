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
            var ident = new RegexToken("ident", @"(\w|\$)(\w|\$|\d)*");
            var singleString = new RegexToken("singleString", @"'(?:[^\']|\\'|\\[^'])*'");
            var ws = new RegexToken("ws", @"\s+");
            var dot = new StringToken("dot", @".");
            var leftParen = new StringToken("leftParen", @"(");
            var rightParen = new StringToken("rightParen", @")");
            var assign = new StringToken("assign", @"=");

            var a = new LexemeList
            {
                { ident, "document", 0 },
                { dot, ".", 8 },
                { ident, "getElementById", 9 },
                { leftParen, "(", 23 },
                { singleString, "'demo'", 24 },
                { rightParen, ")", 30 },
                { dot, ".", 31 },
                { ident, "innerHTML", 32 },
                { ws, " ", 41 },
                { assign, "=", 42 },
                { ws, " ", 43 },
                { ident, "Date", 44 },
                { leftParen, "(", 48 },
                { rightParen, ")", 49 },
            };

            var b = new LexemeList
            {
                { ident, "document", 0 },
                { dot, ".", 8 },
                { ident, "getElementById", 9 },
                { leftParen, "(", 23 },
                { singleString, "'demo'", 24 },
                { rightParen, ")", 30 },
                { dot, ".", 31 },
                { ident, "innerHTML", 32 },
                { ws, " ", 41 },
                { assign, "=", 42 },
                { ws, " ", 43 },
                { ident, "Date", 44 },
                { leftParen, "(", 48 },
                { rightParen, ")", 49 },
            };

            Assert.IsTrue(a.Equals(b));
        }

        [TestMethod]
        public void LexemeList()
        {
            var grammar = new Grammar();

            var ident = grammar.DefineRegex("ident", @"(\w|\$)(\w|\$|\d)*");
            var singleString = grammar.DefineRegex("singleString", @"'(?:[^\']|\\'|\\[^'])*'");
            var ws = grammar.DefineRegex("ws", @"\s+");
            var dot = grammar.DefineString("dot", @".");
            var leftParen = grammar.DefineString("leftParen", @"(");
            var rightParen = grammar.DefineString("rightParen", @")");
            var assign = grammar.DefineString("assign", @"=");

            var standard = new LexemeList
            {
                { ws, "\r\n", 0 },
                { ident, "document", 2 },
                { dot, ".", 10 },
                { ident, "getElementById", 11 },
                { leftParen, "(", 25 },
                { singleString, "'demo'", 26 },
                { rightParen, ")", 32 },
                { dot, ".", 33 },
                { ident, "innerHTML", 34 },
                { ws, " ", 43 },
                { assign, "=", 44 },
                { ws, " ", 45 },
                { ident, "Date", 46 },
                { leftParen, "(", 50 },
                { rightParen, ")", 51 },
                { ws, "\r\n", 52 },
            };

            var lexer = new Lexer(grammar);

            var lexemes = lexer.Lex(@"
document.getElementById('demo').innerHTML = Date()
")
                .ToList();

            Assert.IsTrue(standard.Equals(lexemes));
        }
    }
}
