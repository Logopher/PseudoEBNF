using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Reporting;
using System.Collections.Generic;

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
            var results = match.Success ? new[] { match.Result } : new IParseNode[0];

            return new Match<IParseNode>(new BranchParseNode(this, results), true);
        }
    }
}
