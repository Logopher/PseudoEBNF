using System;
using System.Linq;
using PseudoEBNF.Common;
using PseudoEBNF.Parsing.Parsers;
using PseudoEBNF.Parsing.Rules;
using PseudoEBNF.PseudoEBNF;
using PseudoEBNF.Semantics;

namespace PseudoEBNF
{
    public class ParserGenerator
    {
        private readonly BootstrapParser parser = new BootstrapParser();

        public ParserGenerator()
        {
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
            parser.DefineRegex(RuleName.LineComment, $@"//[^{"\r\n"}]*(?=[{"\r\n"}])");

            parser.DefineRule(RuleName.And,
                parser.ReferenceRule(RuleName.SimpleExpression)
                    .And(parser.ReferenceRule(RuleName.Expression)));

            parser.DefineRule(RuleName.Or,
                parser.ReferenceRule(RuleName.SimpleExpression)
                    .And(parser.ReferenceRule(RuleName.Pipe),
                        parser.ReferenceRule(RuleName.Expression)));

            parser.DefineRule(RuleName.Not,
                parser.ReferenceRule(RuleName.ExclamationPoint)
                    .And(parser.ReferenceRule(RuleName.SimpleExpression)));

            parser.DefineRule(RuleName.Optional,
                parser.ReferenceRule(RuleName.QuestionMark)
                    .And(parser.ReferenceRule(RuleName.SimpleExpression)));

            parser.DefineRule(RuleName.Repeat,
                parser.ReferenceRule(RuleName.Asterisk)
                    .And(parser.ReferenceRule(RuleName.SimpleExpression)));

            parser.DefineRule(RuleName.Group,
                parser.ReferenceRule(RuleName.LeftParenthesis)
                    .And(parser.ReferenceRule(RuleName.Expression),
                        parser.ReferenceRule(RuleName.RightParenthesis)));

            parser.DefineRule(RuleName.Literal,
                parser.ReferenceRule(RuleName.String)
                    .Or(parser.ReferenceRule(RuleName.Regex)));

            parser.DefineRule(RuleName.SimpleExpression,
                parser.ReferenceRule(RuleName.Optional)
                    .Or(parser.ReferenceRule(RuleName.Repeat),
                        parser.ReferenceRule(RuleName.Not),
                        parser.ReferenceRule(RuleName.Group),
                        parser.ReferenceRule(RuleName.Identifier)));

            parser.DefineRule(RuleName.Expression,
                parser.ReferenceRule(RuleName.Or)
                    .Or(parser.ReferenceRule(RuleName.And),
                        parser.ReferenceRule(RuleName.SimpleExpression)));

            parser.DefineRule(RuleName.Token,
                parser.ReferenceRule(RuleName.Identifier)
                    .And(parser.ReferenceRule(RuleName.Equals),
                        parser.ReferenceRule(RuleName.Literal),
                        parser.ReferenceRule(RuleName.Semicolon)));

            parser.DefineRule(RuleName.Rule,
                parser.ReferenceRule(RuleName.Identifier)
                    .And(parser.ReferenceRule(RuleName.Equals),
                        parser.ReferenceRule(RuleName.Expression),
                        parser.ReferenceRule(RuleName.Semicolon)));

            parser.DefineRule(RuleName.Root,
                new RepeatRule(parser, parser.ReferenceRule(RuleName.Token)
                    .Or(parser.ReferenceRule(RuleName.Rule))));

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

            parser.AttachAction(RuleName.Literal, RuleActions.Unwrap);
            parser.AttachAction(RuleName.Expression, RuleActions.Unwrap);
            parser.AttachAction(RuleName.SimpleExpression, RuleActions.Unwrap);
        }

        public Parser SpawnParser(Parser parser, ParserSettings settings, string grammar, params string[] implicitNames)
        {
            var resultGrammar = new Grammar();

            foreach (var name in implicitNames)
            {
                resultGrammar.SetImplicit(name);
            }

            parser.Lock();

            ISemanticNode semantics = parser.Parse(grammar);

            var root = (BranchSemanticNode)semantics;

            foreach (BranchSemanticNode decl in root.Children.Cast<BranchSemanticNode>())
            {
                var left = (LeafSemanticNode)decl.Children[0];
                var name = left.Value;

                ISemanticNode right = decl.Children[1];

                if (right is BranchSemanticNode branch)
                {
                    Rule rule = Interpret(resultGrammar, right);
                    resultGrammar.DefineRule(name, rule);
                }
                else if (right is LeafSemanticNode leaf)
                {
                    switch ((EbnfNodeType)leaf.NodeType)
                    {
                        case EbnfNodeType.Identifier:
                            resultGrammar.DefineRule(name, resultGrammar.ReferenceRule(leaf.Value));
                            break;
                        case EbnfNodeType.String:
                            resultGrammar.DefineString(name, leaf.Value);
                            break;
                        case EbnfNodeType.Regex:
                            resultGrammar.DefineRegex(name, leaf.Value);
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

            return new ParserManager(resultGrammar, settings);
        }

        public Parser SpawnParser(ParserSettings settings, string grammar, params string[] implicitNames) => SpawnParser(parser, settings, grammar, implicitNames);

        private Rule Interpret(Grammar grammar, ISemanticNode node)
        {
            Rule rule = null;

            if (node is BranchSemanticNode branch)
            {
                switch ((EbnfNodeType)branch.NodeType)
                {
                    case EbnfNodeType.Group:
                        rule = Interpret(grammar, branch.Children[0]);
                        break;
                    case EbnfNodeType.Repeat:
                        rule = new RepeatRule(grammar, Interpret(grammar, branch.Children[0]));
                        break;
                    case EbnfNodeType.Optional:
                        rule = new OptionalRule(grammar, Interpret(grammar, branch.Children[0]));
                        break;
                    case EbnfNodeType.Not:
                        rule = new NotRule(grammar, Interpret(grammar, branch.Children[0]));
                        break;

                    case EbnfNodeType.And:
                        rule = new AndRule(grammar, branch.Children.Select(child => Interpret(grammar, child)));
                        break;
                    case EbnfNodeType.Or:
                        rule = new OrRule(grammar, branch.Children.Select(child => Interpret(grammar, child)));
                        break;

                    case EbnfNodeType.None:
                        rule = Interpret(grammar, branch.Children.Single());
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
                        rule = grammar.ReferenceRule(leaf.Value);
                        break;

                    case EbnfNodeType.String:
                    case EbnfNodeType.Regex:
                        break;

                    default:
                        throw new Exception();
                }
            }

            if (rule == null)
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
