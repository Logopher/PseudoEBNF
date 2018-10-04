using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Reporting;
using System;
using System.Collections.Generic;

namespace PseudoEBNF.Parsing.Rules
{
    public class NameRule : IRule
    {
        public Guid CompatibilityGuid { get; }

        public string Name { get; }

        public Grammar Grammar { get; }

        public IRule Rule => Grammar.GetRule(Name);

        public NameRule(Guid compatibilityGuid, Grammar grammar, string name)
        {
            CompatibilityGuid = compatibilityGuid;

            if (grammar.CompatibilityGuid != compatibilityGuid)
            { throw new Exception(); }
            
            Grammar = grammar;

            if (name == RuleName.Root)
            {
                throw new Exception();
            }

            Name = name;
        }

        public IRule Clone()
        {
            return new NameRule(CompatibilityGuid, Grammar, Name);
        }

        public Match<IParseNode> Match(List<Lexeme> lexemes)
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
