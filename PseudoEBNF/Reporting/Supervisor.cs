using PseudoEBNF.Parsing.Rules;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace PseudoEBNF.Reporting
{
    public class Supervisor
    {
        readonly Node root = new Node();

        Stack<Node> stack = new Stack<Node>();

        Node Head => stack.Peek();

        int depth => stack.Count - 1;

        public Supervisor()
        {
            stack.Push(root);
        }

        public void ReportHypothesis(NamedRule rule, int index)
        {
            WriteLine($"? {rule.Name}", index);

            var node = new Node();
            Head.Add(node);
            stack.Push(node);
        }

        public void ReportFailure(NamedRule rule)
        {
            WriteLine($"- {rule.Name}");

            Head.Success = false;
            stack.Pop();
        }

        public void ReportSuccess(NamedRule rule, string text)
        {
            WriteLine($@"+ {rule.Name}
{text}");

            Head.Success = true;
            stack.Pop();
        }

        void WriteLine(string text, int? index = null)
        {
            var indexStr = "\t" + (index?.ToString() ?? "");
            Debug.WriteLine($"{depth}{indexStr}{new string('\t', depth % 40)}{text}");
        }

        class Node : IEnumerable<Node>
        {
            readonly List<Node> children = new List<Node>();

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
