using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using System.Collections.Generic;

namespace PseudoEBNF.Parsing.Rules
{
    public interface IRule : ICompatible
    {
        Match<IParseNode> Match(List<Lexeme> lexemes);
        IRule Clone();
    }
}
