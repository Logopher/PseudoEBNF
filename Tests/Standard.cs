using PseudoEBNF;

namespace Tests
{
    internal static class Standard
    {
        public static string Text { get; } = @"
document.getElementById('demo').innerHTML = Date()
";

        public static LexemeList Lexemes { get; }

        public static IParser Parser { get; }

        static Standard()
        {
            Parser = new PseudoEBNF.JavaScript.Parser();

            {
                var ident = Parser.GetToken("ident");
                var singleString = Parser.GetToken("singleString");
                var ws = Parser.GetToken("ws");
                var dot = Parser.GetToken("dot");
                var leftParen = Parser.GetToken("leftParen");
                var rightParen = Parser.GetToken("rightParen");
                var equals = Parser.GetToken("equals");

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
                    { equals, "=", 44 },
                    { ws, " ", 45 },
                    { ident, "Date", 46 },
                    { leftParen, "(", 50 },
                    { rightParen, ")", 51 },
                    { ws, @"
", 52 },
                };
            }

            {
                var dot = Parser.GetRule("dot");
                var leftParen = Parser.GetRule("leftParen");
                var rightParen = Parser.GetRule("rightParen");
                var equals = Parser.GetRule("equals");
                var ident = Parser.GetRule("ident");
                var singleString = Parser.GetRule("singleString");

                var root = Parser.GetRule("root");
                var statement = Parser.GetRule("statement");
                var assignment = Parser.GetRule("assignment");
                var property = Parser.GetRule("property");
                var functionCall = Parser.GetRule("functionCall");
            }
        }
    }
}