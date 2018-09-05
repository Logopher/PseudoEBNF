using EBNF.Common;
using EBNF.Lexing;
using EBNF.Parsing.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EBNF.Parsing.Rules
{
    public class TokenRule : IRule
    {
        public IToken Token { get; }

        public TokenRule(IToken token)
        {
            Token = token;
        }

        public Match<INode> Match(Parser parser, List<Lexeme> lexemes)
        {
            var first = lexemes.FirstOrDefault();
            if (first?.Token == Token)
            {
                return new Match<INode>(new LeafNode(this, first), true);
            }
            else
            {
                return new Match<INode>(null, false);
            }
        }
    }
}
