using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PseudoEBNF.Parsing.Rules;

namespace PseudoEBNF.Parsing.Nodes
{
    public class BranchParseNode : IParseNode
    {
        public IRule Rule { get; }
        public List<IParseNode> Children { get; }
        public string MatchedText => string.Join("", Children.Select(n => n.MatchedText));
        public int Length => Children.Select(n => n.Length).Sum();
        public int LexemeCount => Children.Select(n => n.LexemeCount).Sum();

        public BranchParseNode(IRule rule, IEnumerable<IParseNode> nodes)
        {
            Rule = rule;
            Children = nodes.ToList();
        }
    }
}
