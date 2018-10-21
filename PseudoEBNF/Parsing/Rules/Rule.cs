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

        public virtual StackMachine.Action SuccessAction { get; } = StackMachine.Action.NextChild;
        public virtual StackMachine.Action FailureAction { get; } = StackMachine.Action.CancelChild;

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
    }
}
