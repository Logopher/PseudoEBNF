using EBNF.Lexing;
using EBNF.Parsing.Rules;
using System;
using System.Collections.Generic;
using System.Text;

namespace EBNF.Parsing.Nodes
{
    public class LeafNode : INode
    {
        public IRule Rule { get; }
        public Lexeme Lexeme { get; }
        public string MatchedText => Lexeme.MatchedText;
        public int Length => Lexeme.Length;
        public int LexemeCount { get; } = 1;

        public LeafNode(IRule rule, Lexeme lexeme)
        {
            Rule = rule;
            Lexeme = lexeme;
        }
    }
}
