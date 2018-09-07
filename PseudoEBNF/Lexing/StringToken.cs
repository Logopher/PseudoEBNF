using PseudoEBNF.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace PseudoEBNF.Lexing
{
    public class StringToken : IToken
    {
        public Guid Guid { get; }

        public string Name { get; }

        public string Text { get; }

        StringToken(Guid guid, string name, string text)
        {
            Guid = guid;
            Name = name;
            Text = text;
        }

        public StringToken(string name, string text)
            : this(Guid.NewGuid(), name, text)
        {
        }

        public IToken Clone()
        {
            return new StringToken(Guid, Name, Text);
        }

        public Match<Lexeme> Match(string input, int index)
        {
            if (input.Substring(index).StartsWith(Text))
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
