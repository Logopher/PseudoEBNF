using System;
using System.Collections.Generic;
using System.Linq;
using PseudoEBNF.Common;
using PseudoEBNF.Reporting;

namespace PseudoEBNF.Lexing
{
    public class Lexer : Compatible
    {
        public Supervisor Super { get; }

        public Grammar Grammar { get; }

        public Lexer(Grammar grammar)
            : base(grammar)
        {
            Super = grammar.Super;
            Grammar = grammar;
        }

        public Lexer()
            : base(Guid.NewGuid())
        {
            Super = new Supervisor();
            Grammar = new Grammar(this, Super);
        }

        public void SetImplicit(string name) => Grammar.SetImplicit(name);

        public void DefineRegex(string name, string pattern) => Grammar.DefineRegex(name, pattern);

        public void DefineString(string name, string text) => Grammar.DefineString(name, text);

        public IEnumerable<Lexeme> Lex(string input)
        {
            var results = new List<Lexeme>();

            if (Grammar.Tokens.Any(pair => pair.Value.CompatibilityGuid != CompatibilityGuid))
            { throw new Exception(); }

            Token[] @implicit = Grammar.ImplicitNames
                .Select(GetToken)
                .Where(t => t != null)
                .ToArray();

            var prevIndex = 0;
            var index = 0;
            while (index < input.Length)
            {
                foreach (Token token in @implicit)
                {
                    Match<Lexeme> match = token.Match(input, index);
                    if (match.Success)
                    {
                        Lexeme lexeme = match.Result;
                        index += lexeme.Length;
                        results.Add(lexeme);
                    }
                }

                foreach (KeyValuePair<string, Token> pair in Grammar.Tokens)
                {
                    var name = pair.Key;
                    Token token = pair.Value;

                    Super.ReportHypothesis(token, index);

                    Match<Lexeme> match = token.Match(input, index);
                    if (match.Success)
                    {
                        Lexeme lexeme = match.Result;
                        index += lexeme.Length;
                        results.Add(lexeme);
                        Super.ReportSuccess(token, lexeme.MatchedText);
                        break;
                    }
                }

                if (index == prevIndex)
                {
                    throw new Exception();
                }
                prevIndex = index;
            }

            return results;
        }

        public Token GetToken(string name)
        {
            if (Grammar.Tokens.TryGetValue(name, out Token token))
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
