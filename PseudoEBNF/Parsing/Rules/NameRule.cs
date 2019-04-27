using System;
using System.Collections.Generic;
using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;

namespace PseudoEBNF.Parsing.Rules
{
    public class NameRule : Rule
    {
        public string Name { get; }

        public Grammar Grammar { get; }

        private Rule rule;
        public Rule Rule => rule ?? (rule = Grammar.GetRule(Name));

        public override IReadOnlyList<Rule> Children => new[] { Rule };

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

        public override Rule Clone() => new NameRule(this, Grammar, Name);

        public override Match<IParseNode> Match(List<Lexeme> lexemes) => Rule.Match(lexemes);

        public override string ToString() => $"{{rule {Name}}}";
    }
}
