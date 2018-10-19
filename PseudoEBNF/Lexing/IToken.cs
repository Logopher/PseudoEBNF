using PseudoEBNF.Common;
using System;

namespace PseudoEBNF.Lexing
{
    public abstract class Token : Compatible, IEquatable<Token>
    {
        public Token(Compatible c)
            : base(c)
        {}

        public abstract Guid Guid { get; }

        public abstract string Name { get; }

        public abstract Match<Lexeme> Match(string input, int index);

        public abstract Token Clone();

        public abstract bool Equals(Token other);

        public override string ToString()
        {
            return $"{{token {Name}}}";
        }
    }
}