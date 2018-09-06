using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Parsing.Rules;
using PseudoEBNF.Semantics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PseudoEBNF
{
    public class Parser
    {
        readonly Dictionary<string, NamedRule> rules = new Dictionary<string, NamedRule>();
        readonly List<string> insignificantRules = new List<string>();
        public IReadOnlyList<string> InsignificantRules => insignificantRules.Union(Lexer.InsignificantTokens).ToList();

        public Lexer Lexer { get; }

        public Parser(Lexer lexer)
        {
            Lexer = lexer;
            //Lexer.Lock();
        }

        public void Define(string name, IRule rule)
        {
            if (Lexer.GetToken(name) != null)
            {
                throw new Exception();
            }

            if (name == RuleName.Root)
            {
                var rules = InsignificantRules.Select(GetRule);
                rule = rule.And(new OptionalRule(new OrRule(rules)));
            }

            rules.Add(name, new NamedRule(name, rule));
        }

        public NamedRule GetRule(string name)
        {
            if (!rules.TryGetValue(name, out NamedRule result))
            {
                var temp = GetTokenRule(name);
                if (temp != null)
                {
                    if (!InsignificantRules.Contains(name))
                    {
                        temp = new NameRule(RuleName.Insignificant)
                            .And(temp);
                    }

                    result = new NamedRule(name, temp);
                    rules.Add(name, result);
                }
            }

            return result;
        }

        IRule GetTokenRule(string name)
        {
            if (name == null)
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

        public ISemanticNode Parse(string input)
        {
            var parseTree = ParseSyntax(input);

            var semanticTree = ParseSemantics(parseTree);

            return semanticTree;
        }

        public IParseNode ParseSyntax(string input)
        {
            Define(RuleName.Insignificant,
                new RepeatRule(
                    new OrRule(InsignificantRules
                        .Select(GetRule))));

            AttachAction(RuleName.Insignificant, (n, r) => null);

            var lexemes = Lexer.Lex(input).ToList();

            var parseTree = ParseSyntax(lexemes);

            if (parseTree == null || parseTree.Length != input.Length)
            {
                return null;
            }

            return parseTree;
        }

        IParseNode ParseSyntax(List<Lexeme> lexemes)
        {
            var rootRule = GetRule(RuleName.Root);
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

        ISemanticNode ParseSemantics(IParseNode node)
        {
            if (node.Rule is NamedRule named)
            {
                return named.Action(node, ParseSemantics);
            }
            else
            {
                throw new Exception();
            }
        }

        public void AttachAction(string name, Func<IParseNode, Func<IParseNode, ISemanticNode>, ISemanticNode> action)
        {
            GetRule(name).AttachAction(action);
        }

        public void MarkRuleInsignificant(string name)
        {
            if (Lexer.GetToken(name) != null)
            {
                Lexer.MarkTokenInsignificant(name);
            }
            else
            {
                insignificantRules.Add(name);
            }
        }
    }
}
