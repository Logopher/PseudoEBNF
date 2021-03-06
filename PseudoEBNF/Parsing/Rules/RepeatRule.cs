﻿using System;
using System.Collections.Generic;
using System.Linq;
using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Parsing.Parsers;
using PseudoEBNF.Reporting;

namespace PseudoEBNF.Parsing.Rules
{
    public class RepeatRule : Rule
    {
        public Rule Rule { get; }

        public Supervisor Super { get; }

        public Grammar Grammar { get; }

        public override StackParser.Action SuccessAction { get; } = StackParser.Action.NextChild;
        public override StackParser.Action FailureAction { get; } = StackParser.Action.NextSibling;

        public override IReadOnlyList<Rule> Children { get; }

        public RepeatRule(Compatible c, Rule rule)
            : base(c)
        {
            if (!IsCompatibleWith(rule))
            { throw new Exception(); }

            Rule = rule;

            Children = new[] { rule };
        }

        public override Rule Clone() => new RepeatRule(this, Rule.Clone());

        public override bool IsFull(Parser p) => false;

        public override bool IsComplete(Parser p) => true;

        public override bool IsExhausted(Parser p) => false;

        public override Rule GetChild(Parser p) => Rule;

        public override string ToString() => $"{{repeat {Rule}}}";

        public override Match<IParseNode> Match(List<Lexeme> lexemes)
        {
            var index = 0;
            var results = new List<IParseNode>();

            while (index < lexemes.Count)
            {
                Match<IParseNode> match = Rule.Match(lexemes.GetRange(index, lexemes.Count - index));
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

            return new Match<IParseNode>(new BranchParseNode(this, lexemes.FirstOrDefault()?.StartIndex ?? -1, results), true);
        }
    }
}
