using PseudoEBNF.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace PseudoEBNF.Lexing
{
    public class StringToken : IToken
    {
        public string Name { get; }

        public string Text { get; }

        public StringToken(string name, string text)
        {
            Name = name;
            Text = text;
        }

        public Match<Lexeme> Match(string input, int index)
        {
            if(input.Substring(index).StartsWith(Text))
            {
                return new Match<Lexeme>(new Lexeme(this, Text, index), true);
            }
            else
            {
                return new Match<Lexeme>(null, false);
            }
        }
    }
}
