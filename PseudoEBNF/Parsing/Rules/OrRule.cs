using System;
using System.Collections.Generic;
using System.Linq;
using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Parsing.Parsers;
using PseudoEBNF.Reporting;

namespace PseudoEBNF.Parsing.Rules
{
    public class OrRule : Rule
    {
        public override StackParser.Action SuccessAction { get; } = StackParser.Action.NextSibling;
        public override StackParser.Action FailureAction { get; } = StackParser.Action.NextChild;

        public override IReadOnlyList<Rule> Children { get; }

        public Supervisor Super { get; }

        public Grammar Grammar { get; }

        public OrRule(Compatible c, IEnumerable<Rule> rules)
            : base(c)
        {
            if (rules.Any(r => !IsCompatibleWith(r)))
            { throw new Exception(); }

            Children = rules
                .SelectMany(r =>
                {
                    if (r is OrRule or)
                    {
                        return or.Children;
                    }
                    else
                    {
                        return new[] { r };
                    }
                })
                .ToList();
        }

        public override Rule Clone() => new OrRule(this, Children.Select(n => n.Clone()));

        public override bool IsFull(Parser p) => p.Nodes.Count == 1;

        public override bool IsComplete(Parser p) => p.Nodes.Count == 1;

        public override bool IsExhausted(Parser p) => Children.Count <= p.RuleIndex;

        public override string ToString() => $"{{or {string.Join(" ", Children)}}}";

        public override Match<IParseNode> Match(List<Lexeme> lexemes)
        {
            foreach (Rule rule in Children)
            {
                Match<IParseNode> match = rule.Match(lexemes);
                if (match.Success)
                {
                    return new Match<IParseNode>(match.Result, true);
                }
            }

            return new Match<IParseNode>(null, false);
        }
    }
}
