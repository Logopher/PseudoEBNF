using Microsoft.VisualStudio.TestTools.UnitTesting;
using PseudoEBNF;
using PseudoEBNF.Common;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Semantics;

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
        public void SingleToken()
        {
            var grammar = $@"
abc = ""abc"";

root = abc;
";
            var parser = parserGen.SpawnParser(grammar);

            parser.AttachAction("abc", (branch, recurse) =>
            {
                var value = branch.Leaf.MatchedText;

                return new LeafSemanticNode(0, value);
            });

            parser.AttachAction("root", (branch, recurse) =>
            {
                return recurse(branch.GetDescendant(0));
            });

            parser.Lock();

            var result = parser.Parse("abc");
        }

        [TestMethod]
        public void ImplicitWhitespace()
        {
            var grammar = $@"
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
";
            var parser = parserGen.SpawnParser(grammar, RuleName.Whitespace, RuleName.LineComment);

            parser.AttachAction(RuleName.Identifier, (branch, recurse) =>
            {
                return new LeafSemanticNode((int)EbnfNodeType.Identifier, branch.Leaf.MatchedText);
            });

            parser.AttachAction(RuleName.Literal, (branch, recurse) =>
            {
                return new BranchSemanticNode(0, recurse(branch));
            });

            parser.Lock();

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

            var grammar = $@"
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
";

            var parser = parserGen.SpawnParser(grammar, RuleName.Whitespace, RuleName.LineComment);

            parser.ToString();
        }

        /*
        [TestMethod]
        public void ParseSyntax()
        {
            var parser = Standard.GetParser();
            var lexemes = Standard.GetLexemes(parser);

            parser.Lock();

            var tree = parser.ParseSyntax(lexemes);

            Assert.AreEqual(54, tree.Length);
            Assert.AreEqual(52, tree.GetDescendant(0, 0, 0, 0).Length);
            Assert.AreEqual("'demo'", tree.GetDescendant(0, 0, 0, 0, 1, 1, 0, 0, 1).MatchedText);
            Assert.AreEqual(0, tree.GetDescendant(1).Branches.Count);
        }
        */

        [TestMethod]
        public void Parse()
        {
            var text = Standard.Text;

            var parser = Standard.GetParser();

            parser.Lock();

            var tree = parser.Parse(text);
        }

        [TestMethod]
        public void RealCode()
        {
            var source = Resources.LoadString("Tests.Resources.angular-mocks.js");

            var parser = Standard.GetParser();

            parser.Lock();

            var tree = parser.Parse(source);
        }
    }
}
