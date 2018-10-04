using System;
using System.Collections.Generic;
using System.Linq;
using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Reporting;
using PseudoEBNF.Semantics;

namespace PseudoEBNF.Parsing.Rules
{
    public class OrRule : IRule
    {
        public Guid CompatibilityGuid { get; }

        public IList<IRule> Children { get; }

        public Supervisor Super { get; }

        public Grammar Grammar { get; }

        public OrRule(Guid compatibilityGuid, IEnumerable<IRule> rules)
        {
            CompatibilityGuid = compatibilityGuid;
            
            if (rules.Any(r => r.CompatibilityGuid != compatibilityGuid))
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

        public IRule Clone()
        {
            return new OrRule(CompatibilityGuid, Children.Select(n => n.Clone()));
        }

        public Match<IParseNode> Match(List<Lexeme> lexemes)
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
