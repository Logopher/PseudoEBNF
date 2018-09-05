using System;
using System.Collections.Generic;
using System.Text;
using EBNF.Common;
using EBNF.Lexing;
using EBNF.Parsing.Nodes;

namespace EBNF.Parsing.Rules
{
    public class NotRule : IRule
    {
        IRule rule;

        public NotRule(IRule rule)
        {
            this.rule = rule;
        }

        public Match<INode> Match(Parser parser, List<Lexeme> lexemes)
        {
            var match = rule.Match(parser, lexemes);
            if(match.Success)
            {
                return new Match<INode>(match.Result, false);
            }
            else
            {
                return new Match<INode>(null, true);
            }
        }
    }
}
