using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Semantics;

namespace PseudoEBNF.Parsing.Rules
{
    public class NameRule : IRule
    {
        public string Name { get; }

        public NameRule(string name)
        {
            Name = name;
        }

        public Match<IParseNode> Match(Parser parser, List<Lexeme> lexemes)
        {
            Debug.WriteLine($"? {Name} {string.Join(" ", lexemes.Select(n => n.MatchedText))}");

            var rule = parser.GetRule(Name);

            var match = rule.Match(parser, lexemes);
            if (match.Success)
            {
                Debug.WriteLine($"+ {Name}");
                return new Match<IParseNode>(new BranchParseNode(this, new[] { match.Result }), true);
            }
            else
            {
                Debug.WriteLine($"- {Name}");
                return new Match<IParseNode>(null, false);
            }
        }
    }
}
