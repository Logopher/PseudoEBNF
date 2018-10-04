using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PseudoEBNF.Parsing.Rules
{
    public class AndRule : IRule
    {
        public Guid CompatibilityGuid { get; }

        public IList<IRule> Children { get; }

        public AndRule(Guid compatibilityGuid, IEnumerable<IRule> rules)
        {
            CompatibilityGuid = compatibilityGuid;

            if (rules.Any(r => r.CompatibilityGuid != compatibilityGuid))
            { throw new Exception(); }

            Children = rules
                .SelectMany(r =>
                {
                    if (r is AndRule and)
                    {
                        return and.Children;
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
            return new AndRule(CompatibilityGuid, Children.Select(n => n.Clone()));
        }

        public Match<IParseNode> Match(List<Lexeme> lexemes)
        {
            var index = 0;
            var results = new List<IParseNode>();

            foreach (var rule in Children)
            {
                var match = rule.Match(lexemes.GetRange(index, lexemes.Count - index));
                if (match.Success)
                {
                    if (match.Result != null)
                    {
                        results.Add(match.Result);
                        index += match.Result.LexemeCount;
                    }
                }
                else
                {
                    return new Match<IParseNode>(null, false);
                }
            }

            return new Match<IParseNode>(new BranchParseNode(this, results), true);
        }
    }
}
