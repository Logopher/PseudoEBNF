using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Parsing.Parsers;
using System;
using System.Collections.Generic;

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

        public virtual bool IsFull(IReadOnlyList<IParseNode> nodes)
        {
            return Children.Count == nodes.Count;
        }

        public virtual bool IsComplete(IReadOnlyList<IParseNode> nodes)
        {
            return Children.Count == nodes.Count;
        }

        public virtual bool IsExhausted(int ruleIndex)
        {
            return Children.Count <= ruleIndex;
        }

        public abstract IReadOnlyList<Rule> Children { get; }
        public int ChildCount => Children.Count;

        public virtual Rule GetChild(int index)
        {
            if (IsExhausted(index))
            { return null; }

            return Children[index];
        }

        public abstract Rule Clone();

        public override string ToString()
        {
            return $"{{rule {string.Join(" ", Children)}}}";
        }

        public abstract Match<IParseNode> Match(List<Lexeme> lexemes);

        public Parser GetParser(string input, int inputIndex)
        {
            return new Parser(this, input, inputIndex);
        }

        public class Parser
        {
            public string Input { get; }
            public Rule Rule { get; }
            public List<IParseNode> Nodes { get; } = new List<IParseNode>();

            public bool IsFull => Rule.IsFull(Nodes);
            public bool IsComplete => Rule.IsComplete(Nodes);
            public bool IsExhausted => Rule.IsExhausted(RuleIndex);

            public int InputIndex { get; }
            public int RuleIndex { get; private set; } = 0;

            internal Parser(Rule rule, string input, int inputIndex)
            {
                Rule = rule;
                Input = input;
                InputIndex = inputIndex;
            }

            internal void AddNode(IParseNode node)
            {
                if (node.Rule != PeekRule())
                { throw new Exception(); }

                Nodes.Add(node);
            }

            internal Rule PeekRule() => Rule.GetChild(RuleIndex);

            internal Rule PopRule() => Rule.GetChild(RuleIndex++);
        }
    }
}
