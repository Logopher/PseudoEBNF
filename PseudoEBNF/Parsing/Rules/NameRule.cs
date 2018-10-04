using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using System;
using System.Collections.Generic;

namespace PseudoEBNF.Parsing.Rules
{
    public class NameRule : Rule
    {
        public string Name { get; }

        public Grammar Grammar { get; }

        public Rule Rule => Grammar.GetRule(Name);

        public NameRule(Compatible c, Grammar grammar, string name)
            : base(c)
        {
            if (!IsCompatibleWith(grammar))
            { throw new Exception(); }

            Grammar = grammar;

            if (name == RuleName.Root)
            {
                throw new Exception();
            }

            Name = name;
        }

        public override Rule Clone()
        {
            return new NameRule(this, Grammar, Name);
        }

        public override Match<IParseNode> Match(List<Lexeme> lexemes)
        {
            var rule = Grammar.GetRule(Name);

            var match = rule.Match(lexemes);
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
