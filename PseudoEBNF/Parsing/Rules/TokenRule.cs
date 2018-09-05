﻿using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Semantics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PseudoEBNF.Parsing.Rules
{
    public class TokenRule : IRule
    {
        public IToken Token { get; }

        public TokenRule(IToken token)
        {
            Token = token;
        }

        public Match<IParseNode> Match(Parser parser, List<Lexeme> lexemes)
        {
            var first = lexemes.FirstOrDefault();
            if (first?.Token == Token)
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