using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EBNF.Common;
using EBNF.Lexing;
using EBNF.Parsing.Nodes;

namespace EBNF.Parsing.Rules
{
    public class RepeatRule : IRule
    {
        IRule rule;

        public RepeatRule(IRule rule)
        {
            this.rule = rule;
        }

        public Match<INode> Match(Parser parser, List<Lexeme> lexemes)
        {
            var index = 0;
            var list = lexemes.ToList();
            var results = new List<INode>();

            while (index < lexemes.Count)
            {
                var match = rule.Match(parser, list.GetRange(index, list.Count - index));
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

            return new Match<INode>(new BranchNode(this, results), true);
        }
    }
}
