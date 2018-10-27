﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PseudoEBNF;
using PseudoEBNF.Common;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.PseudoEBNF;
using PseudoEBNF.Semantics;

namespace Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class Parsing
    {
        private ParserGenerator parserGen;

        [TestInitialize]
        public void Init() => parserGen = new ParserGenerator();

        [TestMethod]
        public void SingleToken()
        {
            var grammar = $@"
abc = ""abc"";

root = abc;
";
            var settings = new ParserSettings
            {
                Algorithm = Parser.Algorithm.LL,
                NestingType = Parser.NestingType.Stack,
                Unit = Parser.Unit.Character,
            };
            Parser parser = parserGen.SpawnParser(settings, grammar);

            parser.AttachAction("abc", (branch, recurse) =>
            {
                var value = branch.Leaf.MatchedText;
                var startIndex = branch.Leaf.StartIndex;

                return new LeafSemanticNode(0, startIndex, value);
            });

            parser.AttachAction("root", (branch, recurse) =>
            {
                return recurse(branch.GetDescendant(0));
            });

            parser.Lock();

            ISemanticNode result = parser.Parse("abc");
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
{RuleName.String} = /""(\\\\[^""]|\\\\""|[^""])*""/;
{RuleName.Regex} = /\/(\\\\[^\/]|\\\\\/|[^\/])+\//;

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
            var settings = new ParserSettings
            {
                Algorithm = Parser.Algorithm.LL,
                NestingType = Parser.NestingType.Stack,
                Unit = Parser.Unit.Character,
            };
            Parser parser = parserGen.SpawnParser(settings, grammar, RuleName.Whitespace, RuleName.LineComment);

            parser.AttachAction(RuleName.Identifier, (branch, recurse) =>
            {
                var startIndex = branch.Leaf.StartIndex;

                return new LeafSemanticNode((int)EbnfNodeType.Identifier, startIndex, branch.Leaf.MatchedText);
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
{RuleName.LeftParenthesis} = ""("";
{RuleName.RightParenthesis} = "")"";

{RuleName.Whitespace} = /\s+/;
{RuleName.Identifier} = /\w(?:\w|\d)*/;
{RuleName.String} = /""(\\\\[^""]|\\\\""|[^""])*""/;
{RuleName.Regex} = /\/(\\\\[^\/]|\\\\\/|[^\/])+\//;
{RuleName.LineComment} = /\/\/[^{"\r\n"}]*(?=[{"\r\n"}])/;

{RuleName.And} = {RuleName.SimpleExpression} {RuleName.Expression};
{RuleName.Or} = {RuleName.SimpleExpression} {RuleName.Pipe} {RuleName.Expression};
{RuleName.Not} = {RuleName.ExclamationPoint} {RuleName.Expression};
{RuleName.Optional} = {RuleName.QuestionMark} {RuleName.Expression};
{RuleName.Repeat} = {RuleName.Asterisk} {RuleName.Expression};
{RuleName.Group} = {RuleName.LeftParenthesis} {RuleName.Expression} {RuleName.RightParenthesis};

{RuleName.Literal} = {RuleName.String} | {RuleName.Regex};
{RuleName.SimpleExpression} = {RuleName.Not} | {RuleName.Optional} | {RuleName.Repeat} | {RuleName.Group} | {RuleName.Identifier};
{RuleName.Expression} = {RuleName.Or} | {RuleName.And} | {RuleName.SimpleExpression};

{RuleName.Token} = {RuleName.Identifier} {RuleName.Equals} {RuleName.Literal} {RuleName.Semicolon};
{RuleName.Rule} = {RuleName.Identifier} {RuleName.Equals} {RuleName.Expression} {RuleName.Semicolon};

// This is a comment.
{RuleName.Root} = *({RuleName.Token} | {RuleName.Rule});
";
            var settings = new ParserSettings
            {
                Algorithm = Parser.Algorithm.LL,
                NestingType = Parser.NestingType.Recursion,
                Unit = Parser.Unit.Lexeme,
            };
            Parser parser = parserGen.SpawnParser(settings, grammar, RuleName.Whitespace, RuleName.LineComment);

            /*
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

            parser.AttachAction(RuleName.Token, (branch, recurse) =>
            {
                ISemanticNode name = recurse(branch.GetDescendant(0));
                ISemanticNode value = recurse(branch.GetDescendant(2));

                return new BranchSemanticNode((int)EbnfNodeType.Token, name, value);
            });

            parser.AttachAction(RuleName.Rule, (branch, recurse) =>
            {
                ISemanticNode name = recurse(branch.GetDescendant(0));
                ISemanticNode value = recurse(branch.GetDescendant(2));

                return new BranchSemanticNode((int)EbnfNodeType.Rule, name, value);
            });

            parser.AttachAction(RuleName.Root, (branch, recurse) =>
            {
                IEnumerable<ISemanticNode> rules = branch
                    .Elements
                    .Select(recurse);

                return new BranchSemanticNode((int)EbnfNodeType.Root, branch.StartIndex, rules);
            });

            parser.AttachAction(RuleName.Literal, RuleActions.Unwrap);
            parser.AttachAction(RuleName.Expression, RuleActions.Unwrap);
            parser.AttachAction(RuleName.SimpleExpression, RuleActions.Unwrap);
            */

            parser.Lock();

            var result = parser.ParseSyntax(grammar);

            Assert.AreEqual(grammar, result.MatchedText);
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

            Parser parser = Standard.GetParser();

            parser.Lock();

            ISemanticNode tree = parser.Parse(text);
        }

        [TestMethod]
        public void RealCode()
        {
            var source = Resources.LoadString("Tests.Resources.angular-mocks.js");

            Parser parser = Standard.GetParser();

            parser.Lock();

            ISemanticNode tree = parser.Parse(source);
        }
    }
}
