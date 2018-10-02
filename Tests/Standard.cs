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

        public static Grammar Grammar { get; }

        public static Lexer Lexer { get; }

        public static Parser Parser { get; }

        static Standard()
        {
            Grammar = new Grammar();

            Lexer = new Lexer(Grammar);

            var ident = Grammar.DefineRegex("ident", @"(\w|\$)(\w|\$|\d)*");
            var singleString = Grammar.DefineRegex("singleString", @"'(?:[^\']|\\'|\\[^'])*'");
            var ws = Grammar.DefineRegex("ws", @"\s+");
            var dot = Grammar.DefineString("dot", @".");
            var leftParen = Grammar.DefineString("leftParen", @"(");
            var rightParen = Grammar.DefineString("rightParen", @")");
            var assign = Grammar.DefineString("assign", @"=");

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