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
            if(name == RuleName.Root)
            {
                throw new Exception();
            }

            Name = name;
        }

        public IRule Clone()
        {
            return new NameRule(Name);
        }

        public Match<IParseNode> Match(Grammar grammar, List<Lexeme> lexemes)
        {
            Debug.WriteLine($"? {Name} {string.Join(" ", lexemes.Select(n => n.MatchedText))}");

            var rule = grammar.GetRule(Name);

            var match = rule.Match(grammar, lexemes);
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
