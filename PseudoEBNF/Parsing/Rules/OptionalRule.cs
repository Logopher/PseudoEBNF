using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Reporting;
using System;
using System.Collections.Generic;

namespace PseudoEBNF.Parsing.Rules
{
    public class OptionalRule : IRule
    {
        public Guid CompatibilityGuid { get; }

        public IRule Rule { get; }

        public Supervisor Super { get; }

        public Grammar Grammar { get; }

        public OptionalRule(Guid compatibilityGuid, IRule rule)
        {
            CompatibilityGuid = compatibilityGuid;

            if (rule.CompatibilityGuid != compatibilityGuid)
            { throw new Exception(); }

            Rule = rule;
        }

        public IRule Clone()
        {
            return new OptionalRule(CompatibilityGuid, Rule.Clone());
        }

        public Match<IParseNode> Match(List<Lexeme> lexemes)
        {

            var match = Rule.Match(lexemes);
            var results = match.Success ? new[] { match.Result } : new IParseNode[0];

            return new Match<IParseNode>(new BranchParseNode(this, results), true);
        }
    }
}
