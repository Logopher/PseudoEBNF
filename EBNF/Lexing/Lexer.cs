using EBNF.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace EBNF.Lexing
{
    public class Lexer
    {
        IToken Whitespace => GetToken(RuleName.Whitespace);
        readonly Dictionary<string, IToken> tokens = new Dictionary<string, IToken>();
        public bool ImplicitWhitespace { get; }
        public bool IsLocked { get; private set; }

        public Lexer(bool implicitWhitespace = false)
        {
            ImplicitWhitespace = implicitWhitespace;
        }

        public IEnumerable<Lexeme> Lex(string input)
        {
            var prevIndex = 0;
            var index = 0;
            while (index < input.Length)
            {
                if (ImplicitWhitespace)
                {
                    var match = Whitespace.Match(input, index);
                    if (match.Success)
                    {
                        var lexeme = match.Result;
                        index += lexeme.Length;
                        yield return lexeme;
                    }
                }

                foreach (var pair in tokens)
                {
                    var name = pair.Key;
                    var token = pair.Value;

                    var match = token.Match(input, index);
                    if (match.Success)
                    {
                        var lexeme = match.Result;
                        index += lexeme.Length;
                        yield return lexeme;
                        continue;
                    }
                }

                if (index == prevIndex)
                {
                    throw new Exception();
                }
                prevIndex = index;
            }
        }

        public IToken GetToken(string name)
        {
            if (tokens.TryGetValue(name, out IToken token))
            {
                return token;
            }
            else
            {
                return null;
            }
        }

        public IToken DefineString(string name, string text)
        {
            return Define(name, new StringToken(name, text));
        }

        public IToken DefineRegex(string name, string pattern)
        {
            return Define(name, new RegexToken(name, pattern));
        }

        IToken Define(string name, IToken token)
        {
            if (IsLocked)
            {
                throw new Exception();
            }

            tokens.Add(name, token);
            return token;
        }

        public void Lock()
        {
            IsLocked = true;
        }
    }
}
