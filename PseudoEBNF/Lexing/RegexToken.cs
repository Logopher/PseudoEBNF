using System;
using System.Text.RegularExpressions;
using PseudoEBNF.Common;

namespace PseudoEBNF.Lexing
{
    public class RegexToken : Token, IEquatable<RegexToken>
    {
        public override Guid Guid { get; }

        public override string Name { get; }

        public Regex Regex { get; }

        private RegexToken(Compatible c, Guid guid, string name, Regex regex)
            : base(c)
        {
            Guid = guid;
            Name = name;
            Regex = regex;
        }

        public RegexToken(Compatible c, string name, string pattern)
            : this(c, Guid.NewGuid(), name, new Regex($@"\G{pattern}", RegexOptions.Compiled | RegexOptions.ExplicitCapture))
        {
        }

        public override Token Clone() => new RegexToken(this, Guid, Name, Regex);

        public override Match<Lexeme> Match(string input, int index)
        {
            Match match = Regex.Match(input, index);
            if (match.Success)
            {
                return new Match<Lexeme>(new Lexeme(this, this, match.Groups[0].Value, index), true);
            }
            else
            {
                return new Match<Lexeme>(null, false);
            }
        }

        public bool Equals(RegexToken other) => Guid == other.Guid;

        public override bool Equals(Token other)
        {
            if (!(other is RegexToken regTok))
            { return false; }

            return Equals(regTok);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is RegexToken regTok))
            { return false; }

            return Equals(regTok);
        }

        public override int GetHashCode() => -737073652 + Guid.GetHashCode();

        public static bool operator ==(RegexToken a, RegexToken b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(RegexToken a, RegexToken b)
        {
            return !(a == b);
        }
    }
}
