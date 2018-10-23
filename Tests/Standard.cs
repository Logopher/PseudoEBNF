using PseudoEBNF;
using PseudoEBNF.JavaScript;
using PseudoEBNF.Parsing.Nodes;
using System;

namespace Tests
{
    internal static class Standard
    {
        public static string Text { get; } = @"
document.getElementById('demo').innerHTML = Date()
";

        public static Parser GetParser ()
        {
            return JavaScriptDefinition.GetParser();
        }

        public static LexemeList GetLexemes(Parser parser = null)
        {
            parser = parser ?? GetParser();

            var ident = parser.GetToken("ident");
            var singleString = parser.GetToken("singleString");
            var ws = parser.GetToken("ws");
            var dot = parser.GetToken("dot");
            var leftParen = parser.GetToken("leftParen");
            var rightParen = parser.GetToken("rightParen");
            var equals = parser.GetToken("equals");

            var results = new LexemeList(parser)
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
            var parser = GetParser();

            var dot = parser.GetRule("dot");
            var leftParen = parser.GetRule("leftParen");
            var rightParen = parser.GetRule("rightParen");
            var equals = parser.GetRule("equals");
            var ident = parser.GetRule("ident");
            var singleString = parser.GetRule("singleString");

            var root = parser.GetRule("root");
            var statement = parser.GetRule("statement");
            var assignment = parser.GetRule("assignment");
            var property = parser.GetRule("property");
            var functionCall = parser.GetRule("functionCall");

            throw new NotImplementedException();
        }
    }
}