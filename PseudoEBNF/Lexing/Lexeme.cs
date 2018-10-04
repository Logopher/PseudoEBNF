﻿using PseudoEBNF.Common;
using System;
using System.Collections.Generic;

namespace PseudoEBNF.Lexing
{
    public class Lexeme : IEquatable<Lexeme>, ICompatible
    {
        public Guid CompatibilityGuid { get; }

        public IToken Token { get; }

        public string MatchedText { get; }

        public int StartIndex { get; }

        public int Length => MatchedText.Length;

        public Lexeme(Guid compatibilityGuid, IToken token, string matchedText, int index)
        {
            CompatibilityGuid = compatibilityGuid;

            if(token.CompatibilityGuid != compatibilityGuid)
            { throw new Exception(); }

            Token = token ?? throw new Exception();
            MatchedText = matchedText ?? throw new Exception();
            StartIndex = index;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Lexeme other))
            { return false; }

            return Equals(other);
        }

        public bool Equals(Lexeme other)
        {
            if (ReferenceEquals(this, other))
            { return true; }

            if (ReferenceEquals(other, null))
            { return false; }

            if (!Token.Equals(other.Token))
            { return false; }

            if (MatchedText != other.MatchedText)
            { return false; }

            if (StartIndex != other.StartIndex)
            { return false; }

            return true;
        }

        public override int GetHashCode()
        {
            var hashCode = -359988537;
            hashCode = hashCode * -1521134295 + Token.GetHashCode();
            hashCode = hashCode * -1521134295 + MatchedText.GetHashCode();
            hashCode = hashCode * -1521134295 + StartIndex.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Lexeme a, Lexeme b)
        {
            if (ReferenceEquals(a, null))
            { return false; }

            return a.Equals(b);
        }

        public static bool operator !=(Lexeme a, Lexeme b)
        {
            return !(a == b);
        }
    }
}