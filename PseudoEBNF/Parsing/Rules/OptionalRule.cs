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
    public class OptionalRule : Rule
    {
        public Rule Rule { get; }

        public Supervisor Super { get; }

        public Grammar Grammar { get; }

        public override StackParser.Action SuccessAction { get; } = StackParser.Action.NextSibling;
        public override StackParser.Action FailureAction { get; } = StackParser.Action.NextSibling;

        public override IReadOnlyList<Rule> Children { get; }

        public OptionalRule(Compatible c, Rule rule)
            : base(c)
        {
            if (!IsCompatibleWith(rule))
            { throw new Exception(); }

            Rule = rule;

            Children = new[] { rule };
        }

        public override Rule Clone() => new OptionalRule(this, Rule.Clone());

        public override bool IsFull(Parser p) => p.Nodes.Count == 1;

        public override bool IsComplete(Parser p) => true;

        public override bool IsExhausted(Parser p) => 0 < p.RuleIndex;

        public override string ToString() => $"{{optional {Rule}}}";

        public override Match<IParseNode> Match(List<Lexeme> lexemes)
        {
            Match<IParseNode> match = Rule.Match(lexemes);
            IParseNode[] results = match.Success ? new[] { match.Result } : new IParseNode[0];
            if (match.Success)
            {
                return new Match<IParseNode>(new BranchParseNode(this, match.Result.StartIndex, new[] { match.Result }), true);
            }
            else
            {
                return new Match<IParseNode>(new BranchParseNode(this, lexemes.FirstOrDefault()?.StartIndex ?? -1, new IParseNode[0]), false);
            }
        }
    }
}
