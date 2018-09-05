using EBNF.Common;
using EBNF.Lexing;
using EBNF.Parsing.Nodes;
using System;
using System.Collections.Generic;
using System.Text;

namespace EBNF.Parsing.Rules
{
    public interface IRule
    {
        Match<INode> Match(Parser parser, List<Lexeme> lexemes);
    }
}
