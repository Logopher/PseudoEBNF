using Microsoft.VisualStudio.TestTools.UnitTesting;
using PseudoEBNF.Lexing;
using PseudoEBNF;
using PseudoEBNF.Common;
using PseudoEBNF.Parsing.Rules;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Parsing;

namespace Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class Parsing
    {
        ParserGenerator parserGen;

        [TestInitialize]
        public void Init()
        {
            parserGen = new ParserGenerator();
        }

        [TestMethod]
        public void ImplicitWhitespace()
        {
            var parser = parserGen.SpawnParser($@"
{RuleName.Equals} = ""="";
{RuleName.Pipe} = ""|"";
{RuleName.Asterisk} = ""*"";
{RuleName.QuestionMark} = ""?"";
{RuleName.ExclamationPoint} = ""!"";
{RuleName.Semicolon} = "";"";

{RuleName.Whitespace} = /\s+/;
{RuleName.Identifier} = /\w(?:\w|\d)*/;
{RuleName.String} = /""(\\[^""]|\\""|[^""])*""/;
{RuleName.Regex} = /\/(\\[^\/]|\\\/|[^\/])+\//;

{RuleName.And} = {RuleName.SimpleExpression} {RuleName.Expression};
{RuleName.Or} = {RuleName.SimpleExpression} {RuleName.Pipe} {RuleName.Expression};
{RuleName.Not} = {RuleName.ExclamationPoint} {RuleName.Expression};
{RuleName.Optional} = {RuleName.QuestionMark} {RuleName.Expression};
{RuleName.Repeat} = {RuleName.Asterisk} {RuleName.Expression};
{RuleName.Group} = {RuleName.LeftParenthesis} {RuleName.Expression} {RuleName.RightParenthesis};

{RuleName.Literal} = {RuleName.String} {RuleName.Pipe} {RuleName.Regex};
{RuleName.SimpleExpression} = {RuleName.Not} {RuleName.Pipe} {RuleName.Optional} {RuleName.Pipe} {RuleName.Repeat} {RuleName.Pipe} {RuleName.Group};
{RuleName.Expression} = {RuleName.Or} {RuleName.Pipe} {RuleName.And} {RuleName.Pipe} {RuleName.SimpleExpression};

{RuleName.Token} = {RuleName.Identifier} {RuleName.Equals} {RuleName.Literal} {RuleName.Semicolon};
{RuleName.Rule} = {RuleName.Identifier} {RuleName.Equals} {RuleName.Expression} {RuleName.Semicolon};

{RuleName.Assignment} = {RuleName.Token} {RuleName.Pipe} {RuleName.Rule};
//{RuleName.Root} = {RuleName.Assignment} *{RuleName.Assignment};
{RuleName.Root} = {RuleName.Identifier} ?({RuleName.Identifier} {RuleName.Identifier});
");

            IParseNode node;

            node = parser.ParseSyntax("a");
            Assert.AreEqual(node.Length, 1);

            node = parser.ParseSyntax("a b c");
            Assert.AreEqual(node.Length, 5);
        }

        [TestMethod]
        public void Ebnf()
        {
            var parserGen = new ParserGenerator();
            
            var parser = parserGen.SpawnParser($@"
{RuleName.Equals} = ""="";
{RuleName.Pipe} = ""|"";
{RuleName.Asterisk} = ""*"";
{RuleName.QuestionMark} = ""?"";
{RuleName.ExclamationPoint} = ""!"";
{RuleName.Semicolon} = "";"";

{RuleName.Whitespace} = /\s+/;
{RuleName.Identifier} = /\w(?:\w|\d)*/;
{RuleName.String} = /""(\\[^""]|\\""|[^""])*""/;
{RuleName.Regex} = /\/(\\[^\/]|\\\/|[^\/])*\//;

{RuleName.And} = {RuleName.SimpleExpression} {RuleName.Expression};
{RuleName.Or} = {RuleName.SimpleExpression} {RuleName.Pipe} {RuleName.Expression};
{RuleName.Not} = {RuleName.ExclamationPoint} {RuleName.Expression};
{RuleName.Optional} = {RuleName.QuestionMark} {RuleName.Expression};
{RuleName.Repeat} = {RuleName.Asterisk} {RuleName.Expression};
{RuleName.Group} = {RuleName.LeftParenthesis} {RuleName.Expression} {RuleName.RightParenthesis};

{RuleName.Literal} = {RuleName.String} {RuleName.Pipe} {RuleName.Regex};
{RuleName.SimpleExpression} = {RuleName.Not} {RuleName.Pipe} {RuleName.Optional} {RuleName.Pipe} {RuleName.Repeat} {RuleName.Pipe} {RuleName.Group};
{RuleName.Expression} = {RuleName.Or} {RuleName.Pipe} {RuleName.And} {RuleName.Pipe} {RuleName.SimpleExpression};

{RuleName.Token} = {RuleName.Identifier} {RuleName.Equals} {RuleName.Literal} {RuleName.Semicolon};
{RuleName.Rule} = {RuleName.Identifier} {RuleName.Equals} {RuleName.Expression} {RuleName.Semicolon};

{RuleName.Assignment} = {RuleName.Token} {RuleName.Pipe} {RuleName.Rule};
//{RuleName.Root} = {RuleName.Assignment} *{RuleName.Assignment};
{RuleName.Root} = {RuleName.Assignment} *{RuleName.Assignment};
");

            parser.ToString();
        }

        Parser GetJsParser()
        {
            var parser = GetEbnfParser();
            return null;
        }

        Parser GetEbnfParser()
        {
            var parser = new Parser();

            parser.SetImplicit(RuleName.Whitespace);

            parser.DefineString(RuleName.Equals, @"=");
            parser.DefineString(RuleName.Pipe, @"|");
            parser.DefineString(RuleName.Asterisk, @"*");
            parser.DefineString(RuleName.QuestionMark, @"?");
            parser.DefineString(RuleName.ExclamationPoint, @"!");
            parser.DefineString(RuleName.Semicolon, @";");
            parser.DefineString(RuleName.LeftParenthesis, @"(");
            parser.DefineString(RuleName.RightParenthesis, @")");

            parser.DefineRegex(RuleName.Whitespace, @"\s+");
            parser.DefineRegex(RuleName.Identifier, @"\w(?:\w|\d)*");
            parser.DefineRegex(RuleName.String, @"""(\\[^""]|\\""|[^""])*""");
            parser.DefineRegex(RuleName.Regex, @"/(\\[^/]|\\/|[^/])*/");

            parser.DefineRule(RuleName.And,
                new NameRule(RuleName.SimpleExpression)
                    .And(new NameRule(RuleName.Expression)));

            parser.DefineRule(RuleName.Or,
                new NameRule(RuleName.SimpleExpression)
                    .And(new NameRule(RuleName.Pipe),
                        new NameRule(RuleName.Expression)));

            parser.DefineRule(RuleName.Not,
                new NameRule(RuleName.ExclamationPoint)
                    .And(new NameRule(RuleName.Expression)));

            parser.DefineRule(RuleName.Optional,
                new NameRule(RuleName.QuestionMark)
                    .And(new NameRule(RuleName.Expression)));

            parser.DefineRule(RuleName.Repeat,
                new NameRule(RuleName.Asterisk)
                    .And(new NameRule(RuleName.Expression)));

            parser.DefineRule(RuleName.Group,
                new NameRule(RuleName.LeftParenthesis)
                    .And(new NameRule(RuleName.Expression),
                        new NameRule(RuleName.RightParenthesis)));

            parser.DefineRule(RuleName.Literal,
                new NameRule(RuleName.String)
                    .Or(new NameRule(RuleName.Regex)));

            parser.DefineRule(RuleName.SimpleExpression,
                new NameRule(RuleName.Optional)
                    .Or(new NameRule(RuleName.Repeat),
                        new NameRule(RuleName.Not),
                        new NameRule(RuleName.Group),
                        new NameRule(RuleName.Identifier)));

            parser.DefineRule(RuleName.Expression,
                new NameRule(RuleName.Or)
                    .Or(new NameRule(RuleName.And),
                        new NameRule(RuleName.SimpleExpression)));

            parser.DefineRule(RuleName.Token,
                new NameRule(RuleName.Identifier)
                    .And(new NameRule(RuleName.Equals),
                        new NameRule(RuleName.Literal),
                        new NameRule(RuleName.Semicolon)));

            parser.DefineRule(RuleName.Rule,
                new NameRule(RuleName.Identifier)
                    .And(new NameRule(RuleName.Equals),
                        new NameRule(RuleName.Expression),
                        new NameRule(RuleName.Semicolon)));

            parser.AttachAction(RuleName.Whitespace, RuleActions.Whitespace);

            parser.AttachAction(RuleName.String, RuleActions.String);
            parser.AttachAction(RuleName.Regex, RuleActions.Regex);
            parser.AttachAction(RuleName.Identifier, RuleActions.Identifier);

            parser.AttachAction(RuleName.Repeat, RuleActions.Repeat);
            parser.AttachAction(RuleName.Optional, RuleActions.Optional);
            parser.AttachAction(RuleName.Not, RuleActions.Not);
            parser.AttachAction(RuleName.Group, RuleActions.Group);

            parser.AttachAction(RuleName.And, RuleActions.And);
            parser.AttachAction(RuleName.Or, RuleActions.Or);

            parser.AttachAction(RuleName.Token, RuleActions.Token);
            parser.AttachAction(RuleName.Rule, RuleActions.Rule);

            return parser;
        }
    }
}
