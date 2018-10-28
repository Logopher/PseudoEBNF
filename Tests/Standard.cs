using System;
using PseudoEBNF;
using PseudoEBNF.JavaScript;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;

namespace Tests
{
    internal static class Standard
    {
        public static string Text { get; } = @"
document.getElementById('demo').innerHTML = Date()
";

        public static Parser GetParser() => JavaScriptDefinition.GetParser();

        public static Lexer GetLexer() => JavaScriptDefinition.GetLexer();

        public static LexemeList GetLexemes(Lexer lexer = null)
        {
            lexer = lexer ?? GetLexer();

            Token ident = lexer.GetToken("ident");
            Token singleString = lexer.GetToken("singleString");
            Token ws = lexer.GetToken("ws");
            Token dot = lexer.GetToken("dot");
            Token leftParen = lexer.GetToken("leftParen");
            Token rightParen = lexer.GetToken("rightParen");
            Token equals = lexer.GetToken("equals");

            var results = new LexemeList(lexer)
                {
                    { ws, @"
", 0 },
                    { ident, "document", 2 },
                    { dot, ".", 10 },
                    { ident, "getElementById", 11 },
                    { leftParen, "(", 25 },
                    { singleString, "'demo'", 26 },
                    { rightParen, ")", 32 },
                    { dot, ".", 33 },
                    { ident, "innerHTML", 34 },
                    { ws, " ", 43 },
                    { equals, "=", 44 },
                    { ws, " ", 45 },
                    { ident, "Date", 46 },
                    { leftParen, "(", 50 },
                    { rightParen, ")", 51 },
                    { ws, @"
", 52 },
                };

            return results;
        }

        public static BranchParseNode GetParseTree()
        {
            Parser parser = GetParser();

            PseudoEBNF.Parsing.Rules.NamedRule dot = parser.GetRule("dot");
            PseudoEBNF.Parsing.Rules.NamedRule leftParen = parser.GetRule("leftParen");
            PseudoEBNF.Parsing.Rules.NamedRule rightParen = parser.GetRule("rightParen");
            PseudoEBNF.Parsing.Rules.NamedRule equals = parser.GetRule("equals");
            PseudoEBNF.Parsing.Rules.NamedRule ident = parser.GetRule("ident");
            PseudoEBNF.Parsing.Rules.NamedRule singleString = parser.GetRule("singleString");

            PseudoEBNF.Parsing.Rules.NamedRule root = parser.GetRule("root");
            PseudoEBNF.Parsing.Rules.NamedRule statement = parser.GetRule("statement");
            PseudoEBNF.Parsing.Rules.NamedRule assignment = parser.GetRule("assignment");
            PseudoEBNF.Parsing.Rules.NamedRule property = parser.GetRule("property");
            PseudoEBNF.Parsing.Rules.NamedRule functionCall = parser.GetRule("functionCall");

            throw new NotImplementedException();
        }
    }
}