using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PseudoEBNF.Parsing.Rules
{
    public class TokenRule : IRule
    {
        public Guid CompatibilityGuid { get; }

        public IToken Token { get; }

        public Supervisor Super { get; }

        public Grammar Grammar { get; }

        public TokenRule(Guid compatibilityGuid, IToken token)
        {
            CompatibilityGuid = compatibilityGuid;
            
            if (token.CompatibilityGuid != compatibilityGuid)
            { throw new Exception(); }

            Token = token;
        }

        public IRule Clone()
        {
            return new TokenRule(CompatibilityGuid, Token.Clone());
        }

        public Match<IParseNode> Match(List<Lexeme> lexemes)
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
