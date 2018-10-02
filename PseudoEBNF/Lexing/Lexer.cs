using PseudoEBNF.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PseudoEBNF.Lexing
{
    public class Lexer
    {
        Grammar grammar;

        public Lexer(Grammar grammar)
        {
            this.grammar = grammar;
        }

        public Lexer()
            : this(new Grammar())
        {
        }

        public void SetImplicit(string name)
        {
            grammar.SetImplicit(name);
        }

        public RegexToken DefineRegex(string name, string pattern)
        {
            return grammar.DefineRegex(name, pattern);
        }

        public StringToken DefineString(string name, string text)
        {
            return grammar.DefineString(name, text);
        }

        public IEnumerable<Lexeme> Lex(string input)
        {
            var prevIndex = 0;
            var index = 0;
            while (index < input.Length)
            {
                foreach (var token in grammar.ImplicitNames.Select(GetToken).Where(t => t != null))
                {
                    var match = token.Match(input, index);
                    if (match.Success)
                    {
                        var lexeme = match.Result;
                        index += lexeme.Length;
                        yield return lexeme;
                    }
                }

                foreach (var pair in grammar.Tokens)
                {
                    var name = pair.Key;
                    var token = pair.Value;

                    var match = token.Match(input, index);
                    if (match.Success)
                    {
                        var lexeme = match.Result;
                        index += lexeme.Length;
                        yield return lexeme;
                        break;
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
            if (grammar.Tokens.TryGetValue(name, out IToken token))
            {
                return token;
            }
            else
            {
                return null;
            }
        }
    }
}
