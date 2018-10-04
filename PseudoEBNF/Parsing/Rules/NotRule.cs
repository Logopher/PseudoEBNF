using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Reporting;
using System;
using System.Collections.Generic;

namespace PseudoEBNF.Parsing.Rules
{
    public class NotRule : Rule
    {
        public Rule Rule { get; }

        public Supervisor Super { get; }

        public Grammar Grammar { get; }

        public NotRule(Compatible c, Rule rule)
            : base(c)
        {
            if (!IsCompatibleWith(rule))
            { throw new Exception(); }

            Rule = rule;
        }

        public override Rule Clone()
        {
            return new NotRule(this, Rule.Clone());
        }

        public override Match<IParseNode> Match(List<Lexeme> lexemes)
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
