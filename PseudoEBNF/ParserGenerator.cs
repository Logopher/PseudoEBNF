using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing;
using PseudoEBNF.Parsing.Rules;
using PseudoEBNF.PseudoEBNF;
using PseudoEBNF.Semantics;
using System;
using System.Linq;

namespace PseudoEBNF
{
    public class ParserGenerator
    {
        Parser parser;

        public ParserGenerator()
        {
            parser = new Parser();

            parser.SetImplicit(RuleName.Whitespace);
            parser.SetImplicit(RuleName.LineComment);

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
            parser.DefineRegex(RuleName.Regex, @"/(\\[^/]|\\/|[^/])+/");
            parser.DefineRegex(RuleName.LineComment, @"//[^\r\n]*(?=[\r\n])");

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

            parser.DefineRule(RuleName.Root,
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
            var result = new Parser();

            if (implicitWhitespace)
            {
                result.SetImplicit(RuleName.Whitespace);
            }

            var semantics = parser.Parse(grammar);

            var root = (BranchSemanticNode)semantics;

            foreach (var decl in root.Children.Cast<BranchSemanticNode>())
            {
                var left = (LeafSemanticNode)decl.Children[0];
                var name = left.Value;

                var right = decl.Children[1];

                if (right is BranchSemanticNode branch)
                {
                    var rule = Interpret(result, right);
                    result.DefineRule(name, rule);
                }
                else if (right is LeafSemanticNode leaf)
                {
                    switch ((EbnfNodeType)leaf.NodeType)
                    {
                        case EbnfNodeType.String:
                            result.DefineString(name, leaf.Value);
                            break;
                        case EbnfNodeType.Regex:
                            result.DefineRegex(name, leaf.Value);
                            break;

                        default:
                            throw new Exception();
                    }
                }
                else
                {
                    throw new Exception();
                }
            }

            return result;
        }

        private IRule Interpret(Parser result, ISemanticNode node)
        {
            IRule rule = null;

            if (node is BranchSemanticNode branch)
            {
                switch ((EbnfNodeType)branch.NodeType)
                {
                    case EbnfNodeType.Group:
                        rule = Interpret(result, branch.Children[0]);
                        break;
                    case EbnfNodeType.Repeat:
                        rule = new RepeatRule(Interpret(result, branch.Children[0]));
                        break;
                    case EbnfNodeType.Optional:
                        rule = new OptionalRule(Interpret(result, branch.Children[0]));
                        break;
                    case EbnfNodeType.Not:
                        rule = new NotRule(Interpret(result, branch.Children[0]));
                        break;

                    case EbnfNodeType.And:
                        rule = new AndRule(branch.Children.Select(child => Interpret(result, child)));
                        break;
                    case EbnfNodeType.Or:
                        rule = new OrRule(branch.Children.Select(child => Interpret(result, child)));
                        break;

                    case EbnfNodeType.Root:
                    case EbnfNodeType.Rule:
                    case EbnfNodeType.Token:
                    default:
                        throw new Exception();
                }
            }
            else if (node is LeafSemanticNode leaf)
            {
                switch ((EbnfNodeType)leaf.NodeType)
                {
                    case EbnfNodeType.Identifier:
                        rule = new NameRule(leaf.Value);
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
