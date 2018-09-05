using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Semantics;
using System;
using System.Collections.Generic;
using System.Text;

namespace PseudoEBNF.Parsing.Rules
{
    public interface IRule
    {
        Match<IParseNode> Match(Parser parser, List<Lexeme> lexemes);
    }
}
