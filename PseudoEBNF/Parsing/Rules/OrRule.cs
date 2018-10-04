using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PseudoEBNF.Parsing.Rules
{
    public class OrRule : Rule
    {
        public IList<Rule> Children { get; }

        public Supervisor Super { get; }

        public Grammar Grammar { get; }

        public OrRule(Compatible c, IEnumerable<Rule> rules)
            : base(c)
        {
            if (rules.Any(r => !IsCompatibleWith(r)))
            { throw new Exception(); }

            Children = rules
                .SelectMany(r =>
                {
                    if (r is OrRule or)
                    {
                        return or.Children;
                    }
                    else
                    {
                        return new[] { r };
                    }
                })
                .ToList();
        }

        public override Rule Clone()
        {
            return new OrRule(this, Children.Select(n => n.Clone()));
        }

        public override Match<IParseNode> Match(List<Lexeme> lexemes)
        {
            foreach (var rule in Children)
            {
                var match = rule.Match(lexemes);
                if (match.Success)
                {
                    return new Match<IParseNode>(match.Result, true);
                }
            }

            return new Match<IParseNode>(null, false);
        }
    }
}
