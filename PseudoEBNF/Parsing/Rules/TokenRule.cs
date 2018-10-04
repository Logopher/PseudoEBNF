using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PseudoEBNF.Parsing.Rules
{
    public class TokenRule : Rule
    {
        public Token Token { get; }

        public Supervisor Super { get; }

        public Grammar Grammar { get; }

        public TokenRule(Compatible c, Token token)
            : base(c)
        {
            if (!IsCompatibleWith(token))
            { throw new Exception(); }

            Token = token;
        }

        public override Rule Clone()
        {
            return new TokenRule(this, Token.Clone());
        }

        public override Match<IParseNode> Match(List<Lexeme> lexemes)
        {
            var first = lexemes.FirstOrDefault();
            if (first?.Token.Guid == Token.Guid)
            {
                return new Match<IParseNode>(new LeafParseNode(this, first), true);
            }
            else
            {
                return new Match<IParseNode>(null, false);
            }
        }
    }
}
