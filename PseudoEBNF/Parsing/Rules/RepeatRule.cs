using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Reporting;
using PseudoEBNF.Semantics;

namespace PseudoEBNF.Parsing.Rules
{
    public class RepeatRule : IRule
    {
        IRule rule;

        public RepeatRule(IRule rule)
        {
            this.rule = rule;
        }

        public IRule Clone()
        {
            return new RepeatRule(rule.Clone());
        }

        public Match<IParseNode> Match(Supervisor super, Grammar grammar, List<Lexeme> lexemes)
        {
            var index = 0;
            var list = lexemes.ToList();
            var results = new List<IParseNode>();

            while (index < lexemes.Count)
            {
                var match = rule.Match(super, grammar, list.GetRange(index, list.Count - index));
                if (match.Success)
                {
                    results.Add(match.Result);
                    index += match.Result.LexemeCount;
                }
                else
                {
                    break;
                }
            }

            return new Match<IParseNode>(new BranchParseNode(this, results), true);
        }
    }
}
