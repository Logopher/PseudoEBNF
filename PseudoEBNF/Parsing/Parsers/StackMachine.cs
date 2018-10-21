using PseudoEBNF.Common;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Parsing.Rules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PseudoEBNF.Parsing.Parsers
{
    public class StackMachine
    {
        enum Operation
        {
            Push,
            Build,
            Cancel,
        }

        public Grammar Grammar { get; }

        List<StackFrame> Stack { get; } = new List<StackFrame>();

        bool HasMoreFrames => 0 < Stack.Count;

        int Index => Stack.Sum(f => f.Nodes.Sum(n => n.Length));

        public StackMachine(Grammar grammar)
        {
            Grammar = grammar;
        }

        public BranchParseNode Parse(string input)
        {
            StackFrame removedFrame = null;
            var frame = PushFrame(Index, Grammar.RootRule);
            var lastOperation = Operation.Push;

            while (true)
            {
                bool success;
                frame = PeekFrame();

                BranchParseNode popnpeek(bool forceSuccess)
                {
                    var countSuccess = success || forceSuccess;

                    removedFrame = PopFrame();
                    frame = PeekFrame();

                    char symbol;
                    BranchParseNode result;
                    if (countSuccess)
                    {
                        lastOperation = Operation.Build;
                        symbol = '+';
                        result = new BranchParseNode(removedFrame.Rule, removedFrame.InputIndex, removedFrame.Nodes);
                    }
                    else
                    {
                        lastOperation = Operation.Cancel;
                        symbol = '-';
                        result = null;
                    }

                    Debug.WriteLine($"{Stack.Count}{new string('\t', Stack.Count % 20)}{symbol} {removedFrame.Rule}");

                    return result;
                }

                void addnode(IParseNode n)
                {
                    if (n != null)
                    { frame.Nodes.Add(n); }
                }

                if (frame.Rule is TokenRule token)
                {
                    var match = token.Match(input, Index);

                    success = match.Success;
                    addnode(match.Result);
                }
                else if (frame.IsFull || frame.IsExhausted)
                {
                    success = frame.IsComplete;
                }
                else
                {
                    switch (lastOperation)
                    {
                        case Operation.Build:
                            frame = PushFrame(Index, frame.PopRule());
                            lastOperation = Operation.Push;
                            success = true;
                            break;
                        case Operation.Cancel:
                            success = false;
                            break;
                        case Operation.Push:
                            frame = PushFrame(Index, frame.PopRule());
                            continue;
                        default:
                            throw new Exception();
                    }
                }

                var child = popnpeek(false);

                if(frame == null)
                {
                    return child;
                }

                var action = success ? frame.Rule.SuccessAction : frame.Rule.FailureAction;
                switch (action)
                {
                    case Action.NextSibling:
                        {
                            addnode(child);

                            addnode(popnpeek(true));

                            var rule = frame.PopRule();
                            if (rule != null)
                            {
                                frame = PushFrame(Index, rule);
                                lastOperation = Operation.Push;
                            }
                        }
                        break;
                    case Action.NextChild:
                        {
                            addnode(child);

                            var rule = frame.PopRule();
                            if (rule != null)
                            {
                                frame = PushFrame(Index, rule);
                                lastOperation = Operation.Push;
                            }
                        }
                        break;
                    case Action.Cancel:
                        lastOperation = Operation.Cancel;
                        break;
                }

                frame = null;
            }
        }

        StackFrame PopFrame()
        {
            var index = Stack.Count - 1;

            if (index < 0)
            { return null; }

            var result = Stack[index];
            Stack.RemoveAt(index);

            return result;
        }

        StackFrame PeekFrame(int depth = 0)
        {
            var index = Stack.Count - (depth + 1);

            if (index < 0)
            { return null; }

            return Stack[index];
        }

        StackFrame PushFrame(int startIndex, Rule rule)
        {
            return PushFrame(new StackFrame(startIndex, rule));
        }

        StackFrame PushFrame(StackFrame frame)
        {
            Debug.WriteLine($"{Stack.Count}{new string('\t', Stack.Count % 20)}? {frame.Rule}");
            Stack.Add(frame);
            return frame;
        }

        class StackFrame
        {
            public Rule Rule { get; }
            public List<IParseNode> Nodes { get; } = new List<IParseNode>();
            public bool IsFull => Rule.IsFull(Nodes);
            public bool IsComplete => Rule.IsComplete(Nodes);
            public bool IsExhausted => Rule.IsExhausted(RuleIndex);

            public int InputIndex { get; }
            public int RuleIndex { get; private set; } = 0;

            public bool IsLeaf => Rule is TokenRule;

            public StackFrame(int startIndex, Rule rule)
            {
                InputIndex = startIndex;
                Rule = rule;
            }

            internal Rule PeekRule()
            {
                return Rule.GetChild(RuleIndex);
            }

            internal Rule PopRule()
            {
                return Rule.GetChild(RuleIndex++);
            }
        }

        public enum Action
        {
            NextSibling,
            NextChild,
            Cancel,
        }
    }
}
