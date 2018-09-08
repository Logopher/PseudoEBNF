using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Reporting;
using PseudoEBNF.Semantics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PseudoEBNF.Parsing.Rules
{
    public class AndRule : IRule
    {
        public IList<IRule> Children { get; }

        public AndRule(IEnumerable<IRule> rules)
        {
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
            return new AndRule(Children.Select(n => n.Clone()));
        }

        public Match<IParseNode> Match(Supervisor super, Grammar grammar, List<Lexeme> lexemes)
        {
            var index = 0;
            var results = new List<IParseNode>();

            foreach (var rule in Children)
            {
                var match = rule.Match(super, grammar, lexemes.GetRange(index, lexemes.Count - index));
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
