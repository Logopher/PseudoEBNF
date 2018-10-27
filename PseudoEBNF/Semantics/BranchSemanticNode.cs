using System.Collections.Generic;
using System.Linq;

namespace PseudoEBNF.Semantics
{
    public class BranchSemanticNode : ISemanticNode
    {
        private readonly List<ISemanticNode> children = new List<ISemanticNode>();

        public int NodeType { get; }

        public int StartIndex { get; }

        public IReadOnlyList<ISemanticNode> Children => children;

        public BranchSemanticNode(int nodeType, int index, IEnumerable<ISemanticNode> children)
        {
            NodeType = nodeType;
            StartIndex = index;
            this.children.AddRange(children);
        }

        public BranchSemanticNode(int nodeType, int index, ISemanticNode first, params ISemanticNode[] rest)
            : this(nodeType, index, first, (IEnumerable<ISemanticNode>)rest)
        {
        }

        public BranchSemanticNode(int nodeType, int index, ISemanticNode first, IEnumerable<ISemanticNode> rest)
            : this(nodeType, index, new[] { first }.Concat(rest))
        {
        }

        public BranchSemanticNode(int nodeType, ISemanticNode first, params ISemanticNode[] rest)
            : this(nodeType, first, (IEnumerable<ISemanticNode>)rest)
        {
        }

        public BranchSemanticNode(int nodeType, ISemanticNode first, IEnumerable<ISemanticNode> rest)
            : this(nodeType, first.StartIndex, new[] { first }.Concat(rest))
        {
        }
    }
}
