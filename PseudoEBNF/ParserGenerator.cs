using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Rules;
using PseudoEBNF.PseudoEBNF;
using PseudoEBNF.Semantics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PseudoEBNF
{
    public class ParserGenerator
    {
        Lexer lexer;
        Parser parser;

        public ParserGenerator()
        {
            lexer = new Lexer();

            lexer.MarkTokenInsignificant(RuleName.Whitespace);
            lexer.MarkTokenInsignificant(RuleName.LineComment);

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
            lexer.DefineRegex(RuleName.Regex, @"/(\\[^/]|\\/|[^/])+/");
            lexer.DefineRegex(RuleName.LineComment, @"//[^\r\n]*(?=[\r\n])");

            parser = new Parser(lexer);

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

            parser.Define(RuleName.Root,
                new RepeatRule(new NameRule(RuleName.Token)
                    .Or(new NameRule(RuleName.Rule))));

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
            parser.AttachAction(RuleName.LineComment, RuleActions.LineComment);

            parser.AttachAction(RuleName.Root, RuleActions.Root);
        }

        public Parser SpawnParser(string grammar, bool implicitWhitespace = true)
        {
            var lexer = new Lexer();
            var result = new Parser(lexer);

            if (implicitWhitespace)
            {
                result.MarkRuleInsignificant(RuleName.Whitespace);
            }

            var semantics = parser.Parse(grammar);

            var root = (BranchSemanticNode)semantics;

            foreach (var decl in root.Children.Cast<BranchSemanticNode>())
            {
                var left = (LeafSemanticNode)decl.Children[0];
                var name = left.Value;

                var right = decl.Children[1];

                Interpret(result, name, right);
            }

            return result;
        }

        private IRule Interpret(Parser result, string name, ISemanticNode node)
        {
            IRule rule = null;

            if (node is BranchSemanticNode branch)
            {
                switch ((EbnfNodeType)branch.NodeType)
                {
                    case EbnfNodeType.Group:
                        rule = Interpret(result, name, branch.Children[0]);
                        break;
                    case EbnfNodeType.Repeat:
                        rule = new RepeatRule(Interpret(result, name, branch.Children[0]));
                        break;
                    case EbnfNodeType.Optional:
                        rule = new OptionalRule(Interpret(result, name, branch.Children[0]));
                        break;
                    case EbnfNodeType.Not:
                        rule = new NotRule(Interpret(result, name, branch.Children[0]));
                        break;

                    case EbnfNodeType.And:
                        rule = new AndRule(branch.Children.Select(child => Interpret(result, name, child)));
                        break;
                    case EbnfNodeType.Or:
                        rule = new OrRule(branch.Children.Select(child => Interpret(result, name, child)));
                        break;

                    case EbnfNodeType.Root:
                    case EbnfNodeType.Rule:
                    case EbnfNodeType.Token:
                    default:
                        throw new Exception();
                }

                result.Define(name, rule);
            }
            else if (node is LeafSemanticNode leaf)
            {
                IToken token;

                switch ((EbnfNodeType)leaf.NodeType)
                {
                    case EbnfNodeType.Identifier:
                        rule = new NameRule(leaf.Value);
                        break;
                    case EbnfNodeType.String:
                        result.Lexer.DefineString(name, leaf.Value);
                        rule = result.GetRule(name);
                        break;
                    case EbnfNodeType.Regex:
                        result.Lexer.DefineRegex(name, leaf.Value);
                        rule = result.GetRule(name);
                        break;

                    default:
                        throw new Exception();
                }
            }

            if(rule == null)
            {
                throw new Exception();
            }
            else
            {
                return rule;
            }
        }
    }
}
