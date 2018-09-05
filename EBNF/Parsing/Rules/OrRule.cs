using System;
using System.Collections.Generic;
using System.Linq;
using EBNF.Common;
using EBNF.Lexing;
using EBNF.Parsing.Nodes;

namespace EBNF.Parsing.Rules
{
    public class OrRule : IRule
    {
        public IEnumerable<IRule> Children { get; }

        public OrRule(IEnumerable<IRule> rules)
        {
            Children = rules.SelectMany(r =>
            {
                if (r is OrRule or)
                {
                    return or.Children;
                }
                else
                {
                    return new[] { r };
                }
            });
        }

        public Match<INode> Match(Parser parser, List<Lexeme> lexemes)
        {
            foreach(var rule in Children)
            {
                var match = rule.Match(parser, lexemes);
                if(match.Success)
                {
                    return new Match<INode>(match.Result, true);
                }
            }

            return new Match<INode>(null, false);
        }
    }
}
