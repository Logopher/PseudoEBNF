using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using EBNF.Common;
using EBNF.Lexing;
using EBNF.Parsing.Nodes;

namespace EBNF.Parsing.Rules
{
    public class NameRule : IRule
    {
        public string Name { get; }

        public NameRule(string name)
        {
            Name = name;
        }

        public Match<INode> Match(Parser parser, List<Lexeme> lexemes)
        {
            Debug.WriteLine($"? {Name} {string.Join(" ", lexemes.Select(n => n.MatchedText))}");

            var rule = parser.GetRule(Name);

            var match = rule.Match(parser, lexemes);
            if (match.Success)
            {
                Debug.WriteLine($"+ {Name}");
                return new Match<INode>(new BranchNode(this, new[] { match.Result }), true);
            }
            else
            {
                Debug.WriteLine($"- {Name}");
                return new Match<INode>(null, false);
            }
        }
    }
}
