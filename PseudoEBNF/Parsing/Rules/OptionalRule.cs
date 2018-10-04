using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Reporting;
using System;
using System.Collections.Generic;

namespace PseudoEBNF.Parsing.Rules
{
    public class OptionalRule : Rule
    {
        public Rule Rule { get; }

        public Supervisor Super { get; }

        public Grammar Grammar { get; }

        public OptionalRule(Compatible c, Rule rule)
            : base(c)
        {
            if (!IsCompatibleWith(rule))
            { throw new Exception(); }

            Rule = rule;
        }

        public override Rule Clone()
        {
            return new OptionalRule(this, Rule.Clone());
        }

        public override Match<IParseNode> Match(List<Lexeme> lexemes)
        {
            var match = Rule.Match(lexemes);
            var results = match.Success ? new[] { match.Result } : new IParseNode[0];

            return new Match<IParseNode>(new BranchParseNode(this, results), true);
        }
    }
}
