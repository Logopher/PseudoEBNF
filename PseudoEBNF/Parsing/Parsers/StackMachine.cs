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
            StackFrame parentFrame = null;

            while (true)
            {
                bool success;
                frame = PeekFrame();

                BranchParseNode popnpeek(bool forceSuccess)
                {
                    var countSuccess = success || forceSuccess;

                    removedFrame = PopFrame();
                    frame = PeekFrame();
                    parentFrame = PeekFrame(1);

                    Debug.WriteLine($"{Stack.Count}{new string('\t', Stack.Count % 20)}{(countSuccess ? '+' : '-')} {removedFrame.Rule}");

                    return countSuccess ? new BranchParseNode(removedFrame.Rule, removedFrame.InputIndex, removedFrame.Nodes) : null;
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
                    if (success)
                    { addnode(match.Result); }
                }
                else if (frame.IsFull || frame.IsExhausted)
                {
                    success = frame.IsComplete;
                }
                else
                {
                    if (frame.RuleIndex != 0)
                    { throw new Exception(); }

                    var rule = frame.PopRule();

                    if (rule == null)
                    { throw new Exception(); }
                    else
                    { frame = PushFrame(Index, rule); }

                    continue;
                }

                if (parentFrame == null)
                {
                    if (success)
                    { return new BranchParseNode(frame.Rule, frame.InputIndex, frame.Nodes); }
                    else
                    { return null; }
                }

                var action = success ? parentFrame.Rule.SuccessAction : parentFrame.Rule.FailureAction;
                switch (action)
                {
                    case Action.NextSibling:
                        {
                            addnode(popnpeek(false));

                            addnode(popnpeek(true));

                            var rule = frame.PopRule();
                            if (rule != null)
                            { frame = PushFrame(Index, rule); }
                        }
                        break;
                    case Action.NextChild:
                        {
                            addnode(popnpeek(false));

                            var rule = frame.PopRule();
                            if (rule != null)
                            { frame = PushFrame(Index, rule); }
                        }
                        break;
                    case Action.CancelChild:
                        {
                            var child = popnpeek(false);
                        }
                        break;
                    case Action.CancelSelf:
                        {
                            var child = popnpeek(false);

                            var self = popnpeek(false);
                        }
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
            CancelChild,
            CancelSelf,
        }
    }
}
