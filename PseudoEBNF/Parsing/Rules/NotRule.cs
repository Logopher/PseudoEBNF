using System;
using System.Collections.Generic;
using System.Text;
using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Reporting;
using PseudoEBNF.Semantics;

namespace PseudoEBNF.Parsing.Rules
{
    public class NotRule : IRule
    {
        public Guid CompatibilityGuid { get; }

        public IRule Rule { get; }

        public Supervisor Super { get; }

        public Grammar Grammar { get; }

        public NotRule(Guid compatibilityGuid, IRule rule)
        {
            CompatibilityGuid = compatibilityGuid;

            if (rule.CompatibilityGuid != compatibilityGuid)
            { throw new Exception(); }

            Rule = rule;
        }

        public IRule Clone()
        {
            return new NotRule(CompatibilityGuid, Rule.Clone());
        }

        public Match<IParseNode> Match(List<Lexeme> lexemes)
        {
            var match = Rule.Match(lexemes);
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
