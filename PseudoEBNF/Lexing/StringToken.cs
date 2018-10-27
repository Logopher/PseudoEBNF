using System;
using PseudoEBNF.Common;

namespace PseudoEBNF.Lexing
{
    public class StringToken : Token, IEquatable<StringToken>
    {
        public override Guid Guid { get; }

        public override string Name { get; }

        public string Text { get; }

        private StringToken(Compatible c, Guid guid, string name, string text)
            : base(c)
        {
            Guid = guid;
            Name = name;
            Text = text;
        }

        public StringToken(Compatible c, string name, string text)
            : this(c, Guid.NewGuid(), name, text)
        {
        }

        public override Token Clone() => new StringToken(this, Guid, Name, Text);

        public override Match<Lexeme> Match(string input, int index)
        {
            if (input.Substring(index).StartsWith(Text))
            {
                return new Match<Lexeme>(new Lexeme(this, this, Text, index), true);
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

        public override bool Equals(Token other)
        {
            if (!(other is StringToken str))
            { return false; }

            return Equals(str);
        }

        public bool Equals(StringToken other) => Guid == other.Guid;

        public override int GetHashCode() => -737073652 + Guid.GetHashCode();

        public static bool operator ==(StringToken a, StringToken b)
        {
            if (ReferenceEquals(a, null))
            { return false; }

            return a.Equals(b);
        }

        public static bool operator !=(StringToken token1, StringToken token2)
        {
            return !(token1 == token2);
        }
    }
}
