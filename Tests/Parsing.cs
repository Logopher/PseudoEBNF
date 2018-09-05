using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PseudoEBNF.Lexing;
using System.Linq;
using PseudoEBNF;
using PseudoEBNF.Common;
using PseudoEBNF.Parsing.Rules;
using PseudoEBNF.Parsing.Nodes;

namespace Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class Parsing
    {
        [TestMethod]
        public void ImplicitWhitespace()
        {
            var parser = GetEbnfParser();

            parser.Define(RuleName.Root,
                new NameRule(RuleName.Identifier)
                    .And(new OptionalRule(
                        new NameRule(RuleName.Identifier)
                            .And(new NameRule(RuleName.Identifier)))));

            IParseNode node;

            node = parser.ParseSyntax("a");
            Assert.AreEqual(node.Length, 1);

            node = parser.ParseSyntax("a b c");
            Assert.AreEqual(node.Length, 5);
        }

        [TestMethod]
        public void Ebnf()
        {
            var parser = GetEbnfParser();
            
            parser.Define(RuleName.Root,
                new RepeatRule(new NameRule(RuleName.Token)
                    .Or(new NameRule(RuleName.Rule))));

            var node = parser.Parse($@"
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
{RuleName.Root} = {RuleName.Assignment} *{RuleName.Assignment};
");

            node.ToString();
        }

        Parser GetEbnfParser()
        {
            var lexer = new Lexer(true);

            lexer.DefineString(RuleName.Equals, @"=");
            lexer.DefineString(RuleName.Pipe, @"|");
            lexer.DefineString(RuleName.Asterisk, @"*");
            lexer.DefineString(RuleName.QuestionMark, @"?");
            lexer.DefineString(RuleName.ExclamationPoint, @"!");
            lexer.DefineString(RuleName.Semicolon, @";");
            lexer.DefineString(RuleName.LeftParenthesis, @"(");
            lexer.DefineString(RuleName.RightParenthesis, @")");

            lexer.DefineRegex(RuleName.Whitespace, @"\s+");
            lexer.DefineRegex(RuleName.Identifier, @"\w(?:\w|\d)*");
            lexer.DefineRegex(RuleName.String, @"""(\\[^""]|\\""|[^""])*""");
            lexer.DefineRegex(RuleName.Regex, @"/(\\[^/]|\\/|[^/])*/");

            var parser = new Parser(lexer);

            parser.Define(RuleName.And,
                new NameRule(RuleName.SimpleExpression)
                    .And(new NameRule(RuleName.Expression)));

            parser.Define(RuleName.Or,
                new NameRule(RuleName.SimpleExpression)
                    .And(new NameRule(RuleName.Pipe),
                        new NameRule(RuleName.Expression)));

            parser.Define(RuleName.Not,
                new NameRule(RuleName.ExclamationPoint)
                    .And(new NameRule(RuleName.Expression)));

            parser.Define(RuleName.Optional,
                new NameRule(RuleName.QuestionMark)
                    .And(new NameRule(RuleName.Expression)));

            parser.Define(RuleName.Repeat,
                new NameRule(RuleName.Asterisk)
                    .And(new NameRule(RuleName.Expression)));

            parser.Define(RuleName.Group,
                new NameRule(RuleName.LeftParenthesis)
                    .And(new NameRule(RuleName.Expression),
                        new NameRule(RuleName.RightParenthesis)));

            parser.Define(RuleName.Literal,
                new NameRule(RuleName.String)
                    .Or(new NameRule(RuleName.Regex)));

            parser.Define(RuleName.SimpleExpression,
                new NameRule(RuleName.Optional)
                    .Or(new NameRule(RuleName.Repeat),
                        new NameRule(RuleName.Not),
                        new NameRule(RuleName.Group),
                        new NameRule(RuleName.Identifier)));

            parser.Define(RuleName.Expression,
                new NameRule(RuleName.Or)
                    .Or(new NameRule(RuleName.And),
                        new NameRule(RuleName.SimpleExpression)));

            parser.Define(RuleName.Token,
                new NameRule(RuleName.Identifier)
                    .And(new NameRule(RuleName.Equals),
                        new NameRule(RuleName.Literal),
                        new NameRule(RuleName.Semicolon)));

            parser.Define(RuleName.Rule,
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
