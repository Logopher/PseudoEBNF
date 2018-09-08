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

        public Supervisor()
        {
            stack.Push(root);
        }

        public void ReportHypothesis(NamedRule rule)
        {
            Debug.WriteLine($"? {rule.Name}");

            var node = new Node();
            Head.Add(node);
            stack.Push(node);
        }

        public void ReportFailure(NamedRule rule)
        {
            Debug.WriteLine($"- {rule.Name}");

            Head.Success = false;
            stack.Pop();
        }

        public void ReportSuccess(NamedRule rule)
        {
            Debug.WriteLine($"+ {rule.Name}");

            Head.Success = true;
            stack.Pop();
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
