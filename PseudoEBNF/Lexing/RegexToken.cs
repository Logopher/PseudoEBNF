using PseudoEBNF.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace PseudoEBNF.Lexing
{
    public class RegexToken : IToken
    {
        public Guid Guid { get; }

        public string Name { get; }

        public Regex Regex { get; }

        RegexToken(Guid guid, string name, Regex regex)
        {
            Guid = guid;
            Name = name;
            Regex = regex;
        }

        public RegexToken(string name, string pattern)
            : this(Guid.NewGuid(), name, new Regex($@"\G{pattern}", RegexOptions.Compiled))
        {
        }

        public IToken Clone()
        {
            return new RegexToken(Guid, Name, Regex);
        }

        public Match<Lexeme> Match(string input, int index)
        {
            var match = Regex.Match(input, index);
            if (match.Success)
            {
                return new Match<Lexeme>(new Lexeme(this, match.Groups[0].Value, index), true);
            }
            else
            {
                return new Match<Lexeme>(null, false);
            }
        }
    }
}
