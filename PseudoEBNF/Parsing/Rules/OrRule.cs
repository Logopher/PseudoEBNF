using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Parsing.Parsers;
using PseudoEBNF.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PseudoEBNF.Parsing.Rules
{
    public class OrRule : Rule
    {
        public override StackMachine.Action SuccessAction { get; } = StackMachine.Action.NextSibling;
        public override StackMachine.Action FailureAction { get; } = StackMachine.Action.NextChild;

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

        public override Rule Clone()
        {
            return new OrRule(this, Children.Select(n => n.Clone()));
        }

        public override bool IsFull(IReadOnlyList<IParseNode> nodes)
        {
            return nodes.Count == 1;
        }

        public override bool IsComplete(IReadOnlyList<IParseNode> nodes)
        {
            return nodes.Count == 1;
        }

        public override bool IsExhausted(int ruleIndex)
        {
            return Children.Count <= ruleIndex;
        }

        public override string ToString()
        {
            return $"{{or {string.Join(" ", Children)}}}";
        }

        public override Match<IParseNode> Match(List<Lexeme> lexemes)
        {
            foreach (var rule in Children)
            {
                var match = rule.Match(lexemes);
                if (match.Success)
                {
                    return new Match<IParseNode>(match.Result, true);
                }
            }

            return new Match<IParseNode>(null, false);
        }
    }
}
