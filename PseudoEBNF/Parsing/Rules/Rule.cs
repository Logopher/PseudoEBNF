using System;
using System.Collections.Generic;
using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Parsing.Parsers;

namespace PseudoEBNF.Parsing.Rules
{
    public abstract class Rule : Compatible
    {
        public Rule(Compatible c)
            : base(c)
        {
        }

        public virtual StackParser.Action SuccessAction { get; } = StackParser.Action.NextChild;
        public virtual StackParser.Action FailureAction { get; } = StackParser.Action.Cancel;

        public virtual bool IsFull(Parser p) => Children.Count == p.Nodes.Count;

        public virtual bool IsComplete(Parser p) => Children.Count == p.Nodes.Count;

        public virtual bool IsExhausted(Parser p) => Children.Count <= p.RuleIndex;

        public abstract IReadOnlyList<Rule> Children { get; }
        public int ChildCount => Children.Count;

        public virtual Rule GetChild(Parser p)
        {
            if (IsExhausted(p))
            { return null; }

            return Children[p.RuleIndex];
        }

        public abstract Rule Clone();

        public override string ToString() => $"{{rule {string.Join(" ", Children)}}}";

        public abstract Match<IParseNode> Match(List<Lexeme> lexemes);

        public Parser GetParser(string input, int inputIndex) => new Parser(this, input, inputIndex);

        public class Parser
        {
            public string Input { get; }
            public Rule Rule { get; }
            public List<IParseNode> Nodes { get; } = new List<IParseNode>();

            public bool IsFull => Rule.IsFull(this);
            public bool IsComplete => Rule.IsComplete(this);
            public bool IsExhausted => Rule.IsExhausted(this);

            public int InputIndex { get; }
            public int RuleIndex { get; private set; } = 0;

            public Rule PreviousRule { get; private set; }

            internal Parser(Rule rule, string input, int inputIndex)
            {
                Rule = rule;
                Input = input;
                InputIndex = inputIndex;
            }

            internal void AddNode(IParseNode node)
            {
                if (node.Rule != PreviousRule)
                { throw new Exception(); }

                Nodes.Add(node);
            }

            internal Rule PeekRule() => Rule.GetChild(this);

            internal Rule PopRule()
            {
                Rule result = PeekRule();

                RuleIndex++;

                PreviousRule = result;

                return result;
            }
        }
    }
}
