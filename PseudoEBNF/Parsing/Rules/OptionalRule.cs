using System;
using System.Collections.Generic;
using System.Text;
using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Semantics;

namespace PseudoEBNF.Parsing.Rules
{
    public class OptionalRule : IRule
    {
        IRule rule;

        public OptionalRule(IRule rule)
        {
            this.rule = rule;
        }

        public IEnumerable<IRule> GetChildren(Parser parser)
        {
            return new[] { rule };
        }

        public Match<IParseNode> Match(Parser parser, List<Lexeme> lexemes)
        {
            var match = rule.Match(parser, lexemes);
            if(match.Success)
            {
                return new Match<IParseNode>(match.Result, true);
            }
            else
            {
                return new Match<IParseNode>(null, true);
            }
        }
    }
}
