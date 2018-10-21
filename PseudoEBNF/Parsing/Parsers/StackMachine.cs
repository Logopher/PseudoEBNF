﻿using PseudoEBNF.Common;
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

        StackFrame head;
        StackFrame Head => head ?? PeekFrame();

        Operation LastOperation { get; set; }

        public StackMachine(Grammar grammar)
        {
            Grammar = grammar;
        }

        public BranchParseNode Parse(string input)
        {
            PushFrame(Index, Grammar.RootRule);

            while (true)
            {
                bool success;

                if (Head.Rule is TokenRule token)
                {
                    var match = token.Match(input, Index);

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
                            LastOperation = Operation.Push;
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

                var child = BuildNode(success);

                if (Head == null)
                {
                    return child;
                }

                var action = success ? Head.Rule.SuccessAction : Head.Rule.FailureAction;
                switch (action)
                {
                    case Action.NextSibling:
                        {
                            AddNodeToHead(child);

                            AddNodeToHead(BuildNode(true));

                            var rule = Head.PopRule();
                            if (rule != null)
                            {
                                PushFrame(Index, rule);
                                LastOperation = Operation.Push;
                            }
                        }
                        break;
                    case Action.NextChild:
                        {
                            AddNodeToHead(child);

                            var rule = Head.PopRule();
                            if (rule != null)
                            {
                                PushFrame(Index, rule);
                                LastOperation = Operation.Push;
                            }
                        }
                        break;
                    case Action.Cancel:
                        LastOperation = Operation.Cancel;
                        break;
                }
            }
        }

        BranchParseNode BuildNode(bool success)
        {
            var removedFrame = PopFrame();

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

        void AddNodeToHead(IParseNode node)
        {
            if (node != null)
            { Head.Nodes.Add(node); }
        }

        StackFrame PopFrame()
        {
            var index = Stack.Count - 1;

            if (index < 0)
            { return null; }

            var result = Stack[index];
            Stack.RemoveAt(index);

            head = null;

            return result;
        }

        StackFrame PeekFrame(int depth = 0)
        {
            var index = Stack.Count - (depth + 1);

            if (index < 0)
            { return null; }

            var result = Stack[index];

            if (index == 0)
            { head = result; }

            return result;
        }

        StackFrame PushFrame(int startIndex, Rule rule)
        {
            return PushFrame(new StackFrame(startIndex, rule));
        }

        StackFrame PushFrame(StackFrame frame)
        {
            Debug.WriteLine($"{Stack.Count}{new string('\t', Stack.Count % 20)}? {frame.Rule}");
            Stack.Add(frame);
            head = frame;
            LastOperation = Operation.Push;
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
