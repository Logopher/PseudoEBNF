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

        public override Rule Clone()
        {
            return new OptionalRule(this, Rule.Clone());
        }

        public override bool IsFull(IReadOnlyList<IParseNode> nodes)
        {
            return nodes.Count == 1;
        }

        public override bool IsComplete(IReadOnlyList<IParseNode> nodes)
        {
            return true;
        }

        public override bool IsExhausted(int ruleIndex)
        {
            return 0 < ruleIndex;
        }

        public override string ToString()
        {
            return $"{{optional {Rule}}}";
        }

        public override Match<IParseNode> Match(List<Lexeme> lexemes)
        {
            var match = Rule.Match(lexemes);
            var results = match.Success ? new[] { match.Result } : new IParseNode[0];
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
