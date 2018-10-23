using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Parsing.Rules;
using PseudoEBNF.Reporting;
using PseudoEBNF.Semantics;

namespace PseudoEBNF.Parsing.Parsers
{
    public class StackParser : Parser
    {
        private enum Operation
        {
            Push,
            Build,
            Cancel,
        }

        public enum Action
        {
            NextSibling,
            NextChild,
            Cancel,
        }

        public Supervisor Super { get; }
        public Grammar Grammar { get; }
        public bool IsLocked => Grammar.IsLocked;

        private List<StackFrame> Stack { get; } = new List<StackFrame>();

        private bool HasMoreFrames => 0 < Stack.Count;

        private int Index => Stack.Sum(f => f.Nodes.Sum(n => n.Length));

        private StackFrame head;

        private StackFrame Head => head ?? PeekFrame();

        private Operation LastOperation { get; set; }

        public StackParser(Grammar grammar)
            : base(grammar)
        {
            Super = grammar.Super;
            Grammar = grammar;
        }

        public override void Lock()
        {
            if (!IsLocked)
            {
                Grammar.Lock();
            }
        }

        public override NamedRule GetRule(string name)
        {
            if (!IsLocked)
            { throw new Exception(); }

            return Grammar.GetRule(name);
        }

        public override Token GetToken(string name)
        {
            if (!IsLocked)
            { throw new Exception(); }

            return Grammar.GetToken(name);
        }

        public override NameRule ReferenceRule(string name)
        {
            if (IsLocked)
            { throw new Exception(); }

            return new NameRule(this, Grammar, name);
        }

        public override ISemanticNode Parse(string input)
        {
            if (!IsLocked)
            { throw new Exception(); }

            BranchParseNode parseTree = ParseSyntax(input);

            ISemanticNode semanticTree = ParseSemantics(parseTree);

            return semanticTree;
        }

        public override ISemanticNode ParseSemantics(BranchParseNode node)
        {
            if (!IsLocked)
            { throw new Exception(); }

            if (node.Rule is NamedRule named)
            { return named.Action(node, ParseSemantics); }
            else
            { throw new Exception(); }
        }

        public override BranchParseNode ParseSyntax(string input)
        {
            PushFrame(Index, Grammar.RootRule);

            while (true)
            {
                bool success;

                if (Head.Rule is TokenRule token)
                {
                    Match<IParseNode> match = token.Match(input, Index);

                    success = match.Success;
                    AddNodeToHead(match.Result);
                }
                else if (Head.IsFull || Head.IsExhausted)
                {
                    success = Head.IsComplete;
                }
                else
                {
                    switch (LastOperation)
                    {
                        case Operation.Build:
                            PushFrame(Index, Head.PopRule());
                            success = true;
                            break;
                        case Operation.Cancel:
                            success = false;
                            break;
                        case Operation.Push:
                            PushFrame(Index, Head.PopRule());
                            continue;
                        default:
                            throw new Exception();
                    }
                }

                BranchParseNode child = BuildNode(success);

                if (Head == null)
                { return child; }

                Action action = success ? Head.Rule.SuccessAction : Head.Rule.FailureAction;
                switch (action)
                {
                    case Action.NextSibling:
                        {
                            AddNodeToHead(child);

                            AddNodeToHead(BuildNode(true));

                            PushFrame(Index, Head.PopRule());
                        }
                        break;
                    case Action.NextChild:
                        {
                            AddNodeToHead(child);

                            PushFrame(Index, Head.PopRule());
                        }
                        break;
                    case Action.Cancel:
                        LastOperation = Operation.Cancel;
                        break;
                }
            }
        }

        private BranchParseNode BuildNode(bool success)
        {
            StackFrame removedFrame = PopFrame();

            char symbol;
            BranchParseNode result;
            if (success)
            {
                LastOperation = Operation.Build;
                symbol = '+';
                result = new BranchParseNode(removedFrame.Rule, removedFrame.InputIndex, removedFrame.Nodes);
            }
            else
            {
                LastOperation = Operation.Cancel;
                symbol = '-';
                result = null;
            }

            Debug.WriteLine($"{Stack.Count}{new string('\t', Stack.Count % 20)}{symbol} {removedFrame.Rule}");

            return result;
        }

        private void AddNodeToHead(IParseNode node)
        {
            if (node != null)
            { Head.Nodes.Add(node); }
        }

        private StackFrame PopFrame()
        {
            var index = Stack.Count - 1;

            if (index < 0)
            { return null; }

            StackFrame result = Stack[index];
            Stack.RemoveAt(index);

            head = null;

            return result;
        }

        private StackFrame PeekFrame(int depth = 0)
        {
            var index = Stack.Count - (depth + 1);

            if (index < 0)
            { return null; }

            StackFrame result = Stack[index];

            if (index == 0)
            { head = result; }

            return result;
        }

        private void PushFrame(int startIndex, Rule rule)
        {
            if (rule == null)
            { return; }

            PushFrame(new StackFrame(startIndex, rule));
        }

        private void PushFrame(StackFrame frame)
        {
            Debug.WriteLine($"{Stack.Count}{new string('\t', Stack.Count % 20)}? {frame.Rule}");
            Stack.Add(frame);
            head = frame;
            LastOperation = Operation.Push;
        }

        public override void AttachAction(string name, Func<BranchParseNode, Func<BranchParseNode, ISemanticNode>, ISemanticNode> action)
        {
            if (IsLocked)
            { throw new Exception(); }

            Grammar.AttachAction(name, action);
        }

        private class StackFrame
        {
            public Rule Rule { get; }
            public List<IParseNode> Nodes { get; } = new List<IParseNode>();
            public bool IsFull => Rule.IsFull(Nodes);
            public bool IsComplete => Rule.IsComplete(Nodes);
            public bool IsExhausted => Rule.IsExhausted(RuleIndex);

            public int InputIndex { get; }
            public int RuleIndex { get; private set; } = 0;

            public StackFrame(int startIndex, Rule rule)
            {
                InputIndex = startIndex;
                Rule = rule ?? throw new Exception();
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
