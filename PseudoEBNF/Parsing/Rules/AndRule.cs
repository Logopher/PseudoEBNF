﻿using System;
using System.Collections.Generic;
using System.Linq;
using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Parsing.Parsers;

namespace PseudoEBNF.Parsing.Rules
{
    public class AndRule : Rule
    {
        public override IReadOnlyList<Rule> Children { get; }

        public override StackParser.Action SuccessAction { get; } = StackParser.Action.NextChild;
        public override StackParser.Action FailureAction { get; } = StackParser.Action.Cancel;

        public AndRule(Compatible c, IEnumerable<Rule> rules)
            : base(c)
        {
            if (rules.Any(r => !IsCompatibleWith(r)))
            { throw new Exception(); }

            Children = rules
                .SelectMany(r =>
                {
                    if (r is AndRule and)
                    {
                        return and.Children;
                    }
                    else
                    {
                        return new[] { r };
                    }
                })
                .ToList();
        }

        public override Rule Clone() => new AndRule(this, Children.Select(n => n.Clone()));

        public override string ToString() => $"{{and {string.Join(" ", Children)}}}";

        public override Match<IParseNode> Match(List<Lexeme> lexemes)
        {
            var index = 0;
            var results = new List<IParseNode>();

            foreach (Rule rule in Children)
            {
                Match<IParseNode> match = rule.Match(lexemes.GetRange(index, lexemes.Count - index));
                if (match.Success)
                {
                    if (match.Result != null)
                    {
                        results.Add(match.Result);
                        index += match.Result.LexemeCount;
                    }
                }
                else
                {
                    return new Match<IParseNode>(null, false);
                }
            }

            return new Match<IParseNode>(new BranchParseNode(this, results.First().StartIndex, results), true);
        }
    }
}
