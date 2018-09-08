using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Reporting;
using System.Collections.Generic;

namespace PseudoEBNF.Parsing.Rules
{
    public interface IRule
    {
        Match<IParseNode> Match(Supervisor super, Grammar grammar, List<Lexeme> lexemes);
        IRule Clone();
    }
}
