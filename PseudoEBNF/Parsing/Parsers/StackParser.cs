using System;
using System.Collections.Generic;
using System.Linq;
using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Parsing.Rules;
using PseudoEBNF.Reporting;
using PseudoEBNF.Semantics;

namespace PseudoEBNF.Parsing.Parsers
{
    using StackFrame = Rule.Parser;

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

        private List<StackFrame> Stack { get; } = new List<StackFrame>();

        private bool HasMoreFrames => 0 < Stack.Count;

        private int Index => Stack.Sum(f => f.Nodes.Sum(n => n.Length));

        private StackFrame head;

        private StackFrame Head => head ?? PeekFrame();

        private Operation LastOperation { get; set; }

        public StackParser(Grammar grammar)
            : base(grammar)
        {
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
            PushFrame(Grammar.RootRule, input, Index);

            while (true)
            {
                bool success;

                if (Head.Rule is TokenRule token)
                {
                    Match<IParseNode> match = token.Match(input, Index);

                    success = match.Success;

                    PopFrame();

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
                            PushFrame(Head.PopRule(), input, Index);
                            success = true;
                            break;
                        case Operation.Cancel:
                            success = false;
                            break;
                        case Operation.Push:
                            PushFrame(Head.PopRule(), input, Index);
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

                            PushFrame(Head.PopRule(), input, Index);
                        }
                        break;
                    case Action.NextChild:
                        {
                            AddNodeToHead(child);

                            PushFrame(Head.PopRule(), input, Index);
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

            BranchParseNode result;
            if (success)
            {
                LastOperation = Operation.Build;
                result = new BranchParseNode(removedFrame.Rule, removedFrame.InputIndex, removedFrame.Nodes);

                if (removedFrame.Rule is NamedRule named)
                { Super.ReportSuccess(named, result.MatchedText); }
            }
            else
            {
                LastOperation = Operation.Cancel;
                result = null;

                if (removedFrame.Rule is NamedRule named)
                { Super.ReportFailure(named); }
            }

            //Debug.WriteLine($"{Stack.Count}{new string('\t', Stack.Count % 20)}{symbol} {removedFrame.Rule}");

            return result;
        }

        private void AddNodeToHead(IParseNode node)
        {
            if (node != null)
            { Head.AddNode(node); }
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

        private void PushFrame(Rule rule, string input, int startIndex)
        {
            if (rule == null)
            { return; }

            PushFrame(rule.GetParser(input, startIndex));
        }

        private void PushFrame(StackFrame frame)
        {
            if (Stack.Any(f => f.Rule == frame.Rule && f.InputIndex == frame.InputIndex))
            { throw new Exception(); }

            if (frame.Rule is NamedRule named)
            {
                Super.ReportHypothesis(named, Index);
            }
            //Debug.WriteLine($"{Stack.Count}{new string('\t', Stack.Count % 20)}? {frame.Rule}");
            Stack.Add(frame);
            head = frame;
            LastOperation = Operation.Push;
        }
    }
}
