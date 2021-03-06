﻿using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Rules;
using System;
using System.Collections.Generic;
using System.Text;

namespace PseudoEBNF.Parsing.Nodes
{
    public class LeafParseNode : IParseNode
    {
        public int StartIndex { get; }
        public Rule Rule { get; }
        public Lexeme Lexeme { get; }
        public string MatchedText => Lexeme.MatchedText;
        public int Length => Lexeme.Length;
        public int LexemeCount { get; } = 1;

        public LeafParseNode(Rule rule, int index, Lexeme lexeme)
        {
            StartIndex = index;
            Rule = rule;
            Lexeme = lexeme;
        }
    }
}
