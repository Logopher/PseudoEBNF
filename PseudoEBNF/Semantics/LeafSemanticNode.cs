using PseudoEBNF.Parsing.Rules;
using System;
using System.Collections.Generic;
using System.Text;

namespace PseudoEBNF.Semantics
{
    public class LeafSemanticNode : ISemanticNode
    {
        public int NodeType { get; }

        public string Value { get; }

        public LeafSemanticNode(int nodeType, string value)
        {
            NodeType = nodeType;
            Value = value;
        }
    }
}
