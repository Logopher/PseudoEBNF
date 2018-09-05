namespace EBNF.Lexing
{
    public class Lexeme
    {
        public IToken Token { get; }

        public string MatchedText { get; }

        public int StartIndex { get; }

        public int Length => MatchedText.Length;

        public Lexeme(IToken token, string matchedText, int index)
        {
            Token = token;
            MatchedText = matchedText;
            StartIndex = index;
        }
    }
}