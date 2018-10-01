using PseudoEBNF.Common;
using System.Collections.Generic;

namespace PseudoEBNF.Semantics
{
    public class BranchSemanticNode : ISemanticNode
    {
        readonly List<ISemanticNode> children = new List<ISemanticNode>();

        public int NodeType { get; }

        public IReadOnlyList<ISemanticNode> Children => children;

        public BranchSemanticNode(int nodeType, IEnumerable<ISemanticNode> children)
        {
            NodeType = nodeType;
            this.children.AddRange(children);
        }

        public BranchSemanticNode(int nodeType, ISemanticNode first, params ISemanticNode[] rest)
            : this(nodeType, first, (IEnumerable<ISemanticNode>)rest)
        {
        }

        public BranchSemanticNode(int nodeType, ISemanticNode first, IEnumerable<ISemanticNode> rest)
            : this(nodeType, Utilities.List(first, rest))
        {
        }
    }
}
