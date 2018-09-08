using System;
using System.Collections.Generic;
using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;

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
            var rule = grammar.GetRule(Name);

            var match = rule.Match(grammar, lexemes);
            if (match.Success)
            {
                return new Match<IParseNode>(new BranchParseNode(this, new[] { match.Result }), true);
            }
            else
            {
                return new Match<IParseNode>(null, false);
            }
        }
    }
}
