using PseudoEBNF.Common;

namespace PseudoEBNF.Lexing
{
    public interface IToken
    {
        string Name { get; }

        Match<Lexeme> Match(string input, int index);
    }
}