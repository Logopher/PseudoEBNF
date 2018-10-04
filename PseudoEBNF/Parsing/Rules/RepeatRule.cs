using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PseudoEBNF.Parsing.Rules
{
    public class RepeatRule : IRule
    {
        public Guid CompatibilityGuid { get; }

        public IRule Rule { get; }

        public Supervisor Super { get; }

        public Grammar Grammar { get; }

        public RepeatRule(Guid compatibilityGuid, IRule rule)
        {
            CompatibilityGuid = compatibilityGuid;

            if (rule.CompatibilityGuid != compatibilityGuid)
            { throw new Exception(); }

            Rule = rule;
        }

        public IRule Clone()
        {
            return new RepeatRule(CompatibilityGuid, Rule.Clone());
        }

        public Match<IParseNode> Match(List<Lexeme> lexemes)
        {
            var index = 0;
            var list = lexemes.ToList();
            var results = new List<IParseNode>();

            while (index < lexemes.Count)
            {
                var match = Rule.Match(list.GetRange(index, list.Count - index));
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
