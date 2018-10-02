using PseudoEBNF.Common;
using System;
using System.Collections.Generic;

namespace PseudoEBNF.Lexing
{
    public class StringToken : IToken, IEquatable<StringToken>
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

        public override bool Equals(object obj)
        {
            if (!(obj is StringToken strTok))
            { return false; }

            return Equals(strTok);
        }

        public bool Equals(IToken other)
        {
            if (!(other is StringToken str))
            { return false; }

            return Equals(str);
        }

        public bool Equals(StringToken other)
        {
            return Guid == other.Guid;
        }

        public override int GetHashCode()
        {
            return -737073652 + Guid.GetHashCode();
        }

        public static bool operator ==(StringToken a, StringToken b)
        {
            if(ReferenceEquals(a, null))
            { return false; }

            return a.Equals(b);
        }

        public static bool operator !=(StringToken token1, StringToken token2)
        {
            return !(token1 == token2);
        }
    }
}
