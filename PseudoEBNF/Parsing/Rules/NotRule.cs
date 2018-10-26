using System;
using System.Collections.Generic;
using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Parsing.Parsers;
using PseudoEBNF.Reporting;

namespace PseudoEBNF.Parsing.Rules
{
    public class NotRule : Rule
    {
        public Rule Rule { get; }

        public Supervisor Super { get; }

        public Grammar Grammar { get; }

        public override StackParser.Action SuccessAction { get; } = StackParser.Action.Cancel;
        public override StackParser.Action FailureAction { get; } = StackParser.Action.NextChild;
        public override IReadOnlyList<Rule> Children { get; }

        public NotRule(Compatible c, Rule rule)
            : base(c)
        {
            if (!IsCompatibleWith(rule))
            { throw new Exception(); }

            Rule = rule;

            Children = new[] { rule };
        }

        public override Rule Clone() => new NotRule(this, Rule.Clone());

        public override bool IsFull(Parser p) => 2 == p.RuleIndex && p.Nodes.Count == 0;

        public override bool IsComplete(Parser p) => 2 == p.RuleIndex && p.Nodes.Count == 0;

        public override bool IsExhausted(Parser p) => 1 <= p.RuleIndex;

        public override string ToString() => $"{{not {Rule}}}";

        public override Match<IParseNode> Match(List<Lexeme> lexemes)
        {
            Match<IParseNode> match = Rule.Match(lexemes);
            if (match.Success)
            {
                return new Match<IParseNode>(match.Result, false);
            }
            else
            {
                return new Match<IParseNode>(null, true);
            }
        }
    }
}
