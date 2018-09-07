using System;
using System.Collections.Generic;
using System.Linq;
using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Semantics;

namespace PseudoEBNF.Parsing.Rules
{
    public class OrRule : IRule
    {
        public IList<IRule> Children { get; }

        public OrRule(IEnumerable<IRule> rules)
        {
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
            return new OrRule(Children.Select(n => n.Clone()));
        }

        public Match<IParseNode> Match(Grammar grammar, List<Lexeme> lexemes)
        {
            foreach (var rule in Children)
            {
                var match = rule.Match(grammar, lexemes);
                if (match.Success)
                {
                    return new Match<IParseNode>(match.Result, true);
                }
            }

            return new Match<IParseNode>(null, false);
        }
    }
}
