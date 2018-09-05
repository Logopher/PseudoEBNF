using EBNF.Common;

namespace EBNF.Lexing
{
    public interface IToken
    {
        string Name { get; }

        Match<Lexeme> Match(string input, int index);
    }
}