using PseudoEBNF.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace PseudoEBNF.Lexing
{
    public class RegexToken : IToken
    {
        public string Name { get; }

        public Regex Regex { get; }

        public RegexToken(string name, string pattern)
        {
            Name = name;
            Regex = new Regex($"^{pattern}", RegexOptions.Compiled);
        }

        public Match<Lexeme> Match(string input, int index)
        {
            var match = Regex.Match(input.Substring(index));
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
