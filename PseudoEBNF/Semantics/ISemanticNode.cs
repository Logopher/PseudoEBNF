using PseudoEBNF.Parsing.Rules;

namespace PseudoEBNF.Semantics
{
    public interface ISemanticNode
    {
        int NodeType { get; }

        int StartIndex { get; }
    }
}
