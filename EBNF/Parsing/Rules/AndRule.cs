using EBNF.Common;
using EBNF.Lexing;
using EBNF.Parsing.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EBNF.Parsing.Rules
{
    public class AndRule : IRule
    {
        public IEnumerable<IRule> Children { get; }

        public AndRule(IEnumerable<IRule> rules)
        {
            Children = rules.SelectMany(r =>
            {
                if (r is AndRule and)
                {
                    return and.Children;
                }
                else
                {
                    return new[] { r };
                }
            });
        }

        public Match<INode> Match(Parser parser, List<Lexeme> lexemes)
        {
            var index = 0;
            var results = new List<INode>();

            foreach (var rule in Children)
            {
                var match = rule.Match(parser, lexemes.GetRange(index, lexemes.Count - index));
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
                    return new Match<INode>(null, false);
                }
            }

            return new Match<INode>(new BranchNode(this, results), true);
        }
    }
}
