using PseudoEBNF.Common;
using PseudoEBNF.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PseudoEBNF.Lexing
{
    public class Lexer : ICompatible
    {
        public Guid CompatibilityGuid { get; }

        public Supervisor Super { get; }

        public Grammar Grammar { get; }

        public Lexer(Supervisor super, Grammar grammar)
        {
            CompatibilityGuid = grammar.CompatibilityGuid;
            Super = super;
            Grammar = grammar;
        }

        public Lexer()
        {
            CompatibilityGuid = Guid.NewGuid();
            Super = new Supervisor();
            Grammar = new Grammar(CompatibilityGuid, Super);
        }

        public void SetImplicit(string name)
        {
            Grammar.SetImplicit(name);
        }

        public void DefineRegex(string name, string pattern)
        {
            Grammar.DefineRegex(name, pattern);
        }

        public void DefineString(string name, string text)
        {
            Grammar.DefineString(name, text);
        }

        public IEnumerable<Lexeme> Lex(Supervisor super, string input)
        {
            var results = new List<Lexeme>();

            if (Grammar.Tokens.Any(pair => pair.Value.CompatibilityGuid != CompatibilityGuid))
            { throw new Exception(); }

            var @implicit = Grammar.ImplicitNames
                .Select(GetToken)
                .Where(t => t != null)
                .ToArray();

            var prevIndex = 0;
            var index = 0;
            while (index < input.Length)
            {
                foreach (var token in @implicit)
                {
                    var match = token.Match(input, index);
                    if (match.Success)
                    {
                        var lexeme = match.Result;
                        index += lexeme.Length;
                        results.Add(lexeme);
                    }
                }

                foreach (var pair in Grammar.Tokens)
                {
                    var name = pair.Key;
                    var token = pair.Value;

                    super.ReportHypothesis(token, index);

                    var match = token.Match(input, index);
                    if (match.Success)
                    {
                        var lexeme = match.Result;
                        index += lexeme.Length;
                        results.Add(lexeme);
                        super.ReportSuccess(token, lexeme.MatchedText);
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

        public IToken GetToken(string name)
        {
            if (Grammar.Tokens.TryGetValue(name, out IToken token))
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
