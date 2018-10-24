using PseudoEBNF.Parsing.Rules;
using System;
using System.Collections.Generic;
using System.Text;

namespace PseudoEBNF.Semantics
{
    public class LeafSemanticNode : ISemanticNode
    {
        public int NodeType { get; }

        public int StartIndex { get; }

        public string Value { get; }

        public LeafSemanticNode(int nodeType, int index, string value)
        {
            NodeType = nodeType;
            StartIndex = index;
            Value = value;
        }
    }
}
