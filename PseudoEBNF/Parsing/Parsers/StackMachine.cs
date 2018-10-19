using PseudoEBNF.Common;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Parsing.Rules;
using System.Collections.Generic;
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
            PushFrame(Index, Grammar.RootRule);

            while (true)
            {
                if (PeekFrame().IsFull || PeekFrame().IsExhausted)
                {
                    BranchParseNode node = null;
                    var frame = PopFrame();
                    var success = frame.IsComplete;

                    if (success)
                    { node = new BranchParseNode(frame.Rule, Index, frame.Nodes); }

                    ConsumeFrame(node, success);

                    if (PeekFrame() == null)
                    { return node; }
                }

                var rule = PeekFrame().DequeueRule();

                if (rule == null)
                { continue; }
                else if (rule is TokenRule token)
                {
                    var match = token.Match(input, Index);

                    ConsumeFrame(match.Result, match.Success);
                }
                else
                { PushFrame(Index, rule); }
            }
        }

        void ConsumeFrame(IParseNode node, bool success)
        {
            var frame = PopFrame();

            if (frame == null)
            { return; }

            Action action;

            {
                var parentFrame = PeekFrame();
                if (parentFrame == null)
                { return; }

                if (success)
                {
                    action = parentFrame.Rule.SuccessAction;
                    frame.Nodes.Add(node);
                }
                else
                {
                    action = parentFrame.Rule.FailureAction;
                }
            }

            BranchParseNode result;

            result = new BranchParseNode(frame.Rule, Index, frame.Nodes);
            PeekFrame().Nodes.Add(result);

            switch (action)
            {
                case Action.NextSibling:
                    frame = PopFrame();

                    result = new BranchParseNode(frame.Rule, Index, frame.Nodes);
                    PeekFrame().Nodes.Add(result);
                    goto case Action.NextChild;
                case Action.NextChild:
                    break;
                case Action.Cancel:
                    frame = PopFrame();
                    break;
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
            var below = PeekFrame();
            Stack.Add(frame);
            return frame;
        }

        class StackFrame
        {
            public Rule Rule { get; }
            public List<IParseNode> Nodes { get; } = new List<IParseNode>();
            public bool IsFull => Rule.IsFull(Nodes);
            public bool IsComplete => Rule.IsComplete(Nodes);
            public bool IsExhausted => Rule.IsExhausted(RuleIndex + 1);

            public int InputIndex { get; }
            public int RuleIndex { get; private set; } = -1;

            public bool IsLeaf => Rule is TokenRule;

            public StackFrame(int startIndex, Rule rule)
            {
                InputIndex = startIndex;
                Rule = rule;
            }

            internal Rule PeekRule()
            {
                return Rule.GetChild(RuleIndex + 1);
            }

            internal Rule DequeueRule()
            {
                return Rule.GetChild(++RuleIndex);
            }
        }

        public enum Action
        {
            NextSibling,
            NextChild,
            Repeat,
            Cancel,
        }
    }
}
