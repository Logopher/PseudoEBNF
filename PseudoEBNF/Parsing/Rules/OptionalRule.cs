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
    public class OptionalRule : IRule
    {
        IRule rule;

        public OptionalRule(IRule rule)
        {
            this.rule = rule;
        }

        public IRule Clone()
        {
            return new OptionalRule(rule.Clone());
        }

        public Match<IParseNode> Match(Supervisor super, Grammar grammar, List<Lexeme> lexemes)
        {
            var match = rule.Match(super, grammar, lexemes);
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
