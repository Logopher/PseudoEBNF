using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using System.Collections.Generic;

namespace PseudoEBNF.Parsing.Rules
{
    public abstract class Rule : Compatible
    {
        public Rule(Compatible c)
            : base(c)
        {
        }

        public abstract Match<IParseNode> Match(List<Lexeme> lexemes);
        public abstract Rule Clone();
    }
}
