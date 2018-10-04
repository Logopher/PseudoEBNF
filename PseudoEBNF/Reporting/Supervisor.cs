using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Rules;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace PseudoEBNF.Reporting
{
    public class Supervisor
    {
        readonly Node root = new Node(string.Empty);

        Stack<Node> stack = new Stack<Node>();

        bool @implicit = false;

        Node Head => stack.Peek();

        int Depth => stack.Count - 1;

        internal Supervisor()
        {
            stack.Push(root);
        }

        public void ReportHypothesis(NamedRule rule, int? index)
        {
            if (rule.Name == RuleName.Implicit)
            {
                @implicit = true;
            }

            WriteLine($"? {rule.Name}", index ?? -1);

            var node = new Node(rule.Name);
            Head.Add(node);
            stack.Push(node);
        }

        public void ReportFailure(NamedRule rule)
        {
            //WriteLine($"- {rule.Name}");

            Head.Success = false;
            var node = stack.Pop();

            if (node.Name == RuleName.Implicit)
            {
                @implicit = false;
            }
        }

        public void ReportSuccess(NamedRule rule, string text)
        {
            WriteLine($@"+ {rule.Name}
{text}");

            Head.Success = true;
            var node = stack.Pop();

            if (node.Name == RuleName.Implicit)
            {
                @implicit = false;
            }
        }

        internal void ReportHypothesis(Token token, int? index)
        {
            WriteLine($"? {token.Name}", index ?? -1);
        }

        public void ReportFailure(Token token)
        {

        }

        public void ReportSuccess(Token token, string text)
        {
            WriteLine($@"+ {token.Name}
{text}");
        }

        void WriteLine(string text, int? index = null)
        {
            if (@implicit)
            {
                return;
            }

            var indexStr = "\t" + (index?.ToString() ?? "");
            Debug.WriteLine($"{Depth}{indexStr}{new string('\t', Depth % 40)}{text}");
        }

        class Node : IEnumerable<Node>
        {
            readonly List<Node> children = new List<Node>();

            public string Name { get; }

            public Node(string name)
            {
                Name = name;
            }

            public IReadOnlyList<Node> Children => children;

            public bool? Success { get; set; }

            public void Add(Node child)
            {
                children.Add(child);
            }

            public IEnumerator<Node> GetEnumerator()
            {
                return children.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
