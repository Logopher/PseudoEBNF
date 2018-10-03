using PseudoEBNF;
using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing;

namespace Tests
{
    internal static class Standard
    {
        public static string Text { get; } = @"
document.getElementById('demo').innerHTML = Date()
";

        public static LexemeList Lexemes { get; }

        public static Parser ParserManager { get; }

        static Standard()
        {
            ParserManager = new Parser();

            ParserManager.DefineRegex("ident", @"(\w|\$)(\w|\$|\d)*");
            ParserManager.DefineRegex("singleString", @"'(?:[^\']|\\'|\\[^'])*'");
            ParserManager.DefineRegex("ws", @"\s+");
            ParserManager.DefineString("dot", @".");
            ParserManager.DefineString("leftParen", @"(");
            ParserManager.DefineString("rightParen", @")");
            ParserManager.DefineString("assign", @"=");

            var ident = ParserManager.GetToken("ident");
            var singleString = ParserManager.GetToken("singleString");
            var ws = ParserManager.GetToken("ws");
            var dot = ParserManager.GetToken("dot");
            var leftParen = ParserManager.GetToken("leftParen");
            var rightParen = ParserManager.GetToken("rightParen");
            var assign = ParserManager.GetToken("assign");

            Lexemes = new LexemeList
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
                { assign, "=", 44 },
                { ws, " ", 45 },
                { ident, "Date", 46 },
                { leftParen, "(", 50 },
                { rightParen, ")", 51 },
                { ws, @"
", 52 },
            };
        }
    }
}