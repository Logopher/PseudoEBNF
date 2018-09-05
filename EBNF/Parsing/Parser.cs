using EBNF.Common;
using EBNF.Lexing;
using EBNF.Parsing.Nodes;
using EBNF.Parsing.Rules;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EBNF
{
    public class Parser
    {
        readonly Dictionary<string, IRule> rules = new Dictionary<string, IRule>();

        public Lexer Lexer { get; }

        public Parser(Lexer lexer)
        {
            Lexer = lexer;
            Lexer.Lock();
        }

        public void Define(string name, IRule rule)
        {
            if(Lexer.GetToken(name) != null)
            {
                throw new Exception();
            }

            if(Lexer.ImplicitWhitespace && name == RuleName.Root)
            {
                rule = rule.And(
                    new OptionalRule(
                        GetTokenRule(RuleName.Whitespace)));
            }

            rules.Add(name, rule);
        }

        public IRule GetRule(string name)
        {
            if (!rules.TryGetValue(name, out IRule result))
            {
                result = GetTokenRule(name);
                if (result != null)
                {
                    if (Lexer.ImplicitWhitespace && name != RuleName.Whitespace)
                    {
                        result = new OptionalRule(
                                GetTokenRule(RuleName.Whitespace))
                            .And(result);
                    }

                    rules.Add(name, result);
                }
            }
            return result;
        }

        IRule GetTokenRule(string name)
        {
            if(name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var token = Lexer.GetToken(name);
            if (token != null)
            {
                return GetTokenRule(token);
            }
            else
            {
                return null;
            }
        }

        IRule GetTokenRule(IToken token)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return new TokenRule(token);
        }

        public INode Parse(string input)
        {
            var lexemes = Lexer.Lex(input).ToList();

            var result = Parse(lexemes);

            if(result == null || result.Length < input.Length)
            {
                return null;
            }

            return result;
        }

        INode Parse(List<Lexeme> lexemes)
        {
            var rootRule = new NameRule(RuleName.Root);
            var match = rootRule.Match(this, lexemes);
            if (match.Success)
            {
                return match.Result;
            }
            else
            {
                return null;
            }
        }
    }
}
