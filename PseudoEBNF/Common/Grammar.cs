using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Parsing.Rules;
using PseudoEBNF.Semantics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PseudoEBNF.Common
{
    public class Grammar
    {
        Guid guid = Guid.NewGuid();

        public IRule RootRule => GetRule(RuleName.Root);

        readonly Dictionary<string, IToken> tokens;
        public IReadOnlyDictionary<string, IToken> Tokens => tokens;

        readonly Dictionary<string, NamedRule> rules;
        public IReadOnlyDictionary<string, NamedRule> Rules => rules;

        readonly List<string> implicitNames;
        public IReadOnlyList<string> ImplicitNames => implicitNames;

        public bool IsLocked { get; private set; }

        public Grammar()
        {
            tokens = new Dictionary<string, IToken>();
            rules = new Dictionary<string, NamedRule>();
            implicitNames = new List<string>();
        }

        Grammar(Dictionary<string, IToken> tokens, Dictionary<string, NamedRule> rules, List<string> implicitNames)
        {
            this.tokens = tokens;
            this.rules = rules;
            this.implicitNames = implicitNames;
        }

        public Grammar Clone()
        {
            return new Grammar(
                tokens.ToDictionary(p => p.Key, p => p.Value.Clone()),
                rules.ToDictionary(p => p.Key, p => (NamedRule)p.Value.Clone()),
                implicitNames.ToList());
        }

        public void Lock()
        {
            IsLocked = true;
        }

        public IRule DefineRule(string name, IRule rule)
        {
            if (IsLocked)
            {
                throw new Exception();
            }

            if (name == RuleName.Root)
            {
                rule = rule.And(new NameRule(RuleName.Implicit));
            }

            var named = new NamedRule(name, rule);

            rules.Add(name, named);

            return named;
        }

        public NamedRule GetRule(string name)
        {
            Rules.TryGetValue(name, out NamedRule result);
            return result;
        }

        internal void SetImplicit(string name)
        {
            if (IsLocked)
            {
                throw new Exception();
            }

            implicitNames.Add(name);
        }

        internal void AttachAction(string name, Func<BranchParseNode, Func<BranchParseNode, ISemanticNode>, ISemanticNode> action)
        {
            GetRule(name).AttachAction(action);
        }

        public StringToken DefineString(string name, string text)
        {
            return (StringToken)DefineToken(name, new StringToken(name, text));
        }

        public RegexToken DefineRegex(string name, string pattern)
        {
            return (RegexToken)DefineToken(name, new RegexToken(name, pattern));
        }

        IToken DefineToken(string name, IToken token)
        {
            if (IsLocked)
            {
                throw new Exception();
            }

            tokens.Add(name, token);

            IRule rule = new TokenRule(token);
            if (!ImplicitNames.Contains(name))
            {
                rule = new NameRule(RuleName.Implicit).And(rule);
            }

            DefineRule(name, rule);

            return token;
        }
    }
}
