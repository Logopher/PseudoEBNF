using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EBNF.Parsing.Rules;

namespace EBNF.Parsing.Nodes
{
    public class BranchNode : INode
    {
        public IRule Rule { get; }
        public List<INode> Children { get; }
        public string MatchedText => string.Join("", Children.Select(n => n.MatchedText));
        public int Length => Children.Select(n => n.Length).Sum();
        public int LexemeCount => Children.Select(n => n.LexemeCount).Sum();

        public BranchNode(IRule rule, IEnumerable<INode> nodes)
        {
            Rule = rule;
            Children = nodes.ToList();
        }
    }
}
