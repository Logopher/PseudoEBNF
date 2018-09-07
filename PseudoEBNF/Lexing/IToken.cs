using PseudoEBNF.Common;
using System;

namespace PseudoEBNF.Lexing
{
    public interface IToken
    {
        Guid Guid { get; }

        string Name { get; }

        Match<Lexeme> Match(string input, int index);

        IToken Clone();
    }
}