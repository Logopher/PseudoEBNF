using System;
using System.Collections.Generic;
using System.Linq;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Parsing.Rules;
using PseudoEBNF.Reporting;
using PseudoEBNF.Semantics;

namespace PseudoEBNF.Common
{
    public class Grammar : Compatible
    {
        public Rule RootRule => GetRule(RuleName.Root);

        public Supervisor Super { get; }

        private readonly Dictionary<string, Token> tokens;
        public IReadOnlyDictionary<string, Token> Tokens => tokens;

        private readonly Dictionary<string, NamedRule> rules;
        public IReadOnlyDictionary<string, NamedRule> Rules => rules;

        private readonly List<string> implicitNames;
        public IReadOnlyList<string> ImplicitNames => implicitNames;

        public bool IsLocked { get; private set; }

        internal Grammar()
            : base(Guid.NewGuid())
        {
            Super = new Supervisor();
            tokens = new Dictionary<string, Token>();
            rules = new Dictionary<string, NamedRule>();
            implicitNames = new List<string>();
        }

        internal Grammar(Compatible c, Supervisor super)
            : base(c)
        {
            Super = super;
            tokens = new Dictionary<string, Token>();
            rules = new Dictionary<string, NamedRule>();
            implicitNames = new List<string>();
        }

        internal Grammar(Compatible c)
            : base(c)
        {
            Super = new Supervisor();
            tokens = new Dictionary<string, Token>();
            rules = new Dictionary<string, NamedRule>();
            implicitNames = new List<string>();
        }

        private Grammar(Compatible c, Dictionary<string, Token> tokens, Dictionary<string, NamedRule> rules, List<string> implicitNames)
            : base(c)
        {
            this.tokens = tokens;
            this.rules = rules;
            this.implicitNames = implicitNames;
        }

        public Grammar Clone()
        {
            return new Grammar(
                this,
                tokens.ToDictionary(p => p.Key, p => p.Value.Clone()),
                rules.ToDictionary(p => p.Key, p => (NamedRule)p.Value.Clone()),
                implicitNames.ToList());
        }

        public void Lock()
        {
            AssertIsNotLocked();

            DefineRule(RuleName.Implicit, new RepeatRule(this,
                new OrRule(this, ImplicitNames
                    .Select(_GetRule)
                    .Where(r => r != null))));

            AttachAction(RuleName.Implicit, (n, r) => null);

            IsLocked = true;
        }

        private void AssertIsLocked()
        {
            if (!IsLocked)
            { throw new InvalidOperationException("Already locked."); }
        }

        private void AssertIsNotLocked()
        {
            if (IsLocked)
            { throw new InvalidOperationException("Already locked."); }
        }

        public void DefineRule(string name, Rule rule)
        {
            //var last = rule.Children.LastOrDefault();

            AssertIsNotLocked();

            if (name == RuleName.Root)
            { rule = rule.And(new NameRule(this, this, RuleName.Implicit)); }

            var named = new NamedRule(this, Super, name, rule);

            rules.Add(name, named);
        }

        NamedRule _GetRule(string name)
        {
            Rules.TryGetValue(name, out NamedRule result);
            return result;
        }

        Token _GetToken(string name)
        {
            Tokens.TryGetValue(name, out Token result);
            return result;
        }

        public NamedRule GetRule(string name)
        {
            AssertIsLocked();

            return _GetRule(name);
        }

        public Token GetToken(string name)
        {
            AssertIsLocked();

            return _GetToken(name);
        }

        internal void SetImplicit(string name)
        {
            AssertIsNotLocked();

            implicitNames.Add(name);
        }

        internal void AttachAction(string name, Func<BranchParseNode, Func<BranchParseNode, ISemanticNode>, ISemanticNode> action) => _GetRule(name).AttachAction(action);

        public void DefineString(string name, string text) => DefineToken(name, new StringToken(this, name, text));

        public void DefineRegex(string name, string pattern) => DefineToken(name, new RegexToken(this, name, pattern));

        private void DefineToken(string name, Token token)
        {
            AssertIsNotLocked();

            tokens.Add(name, token);

            Rule rule = new TokenRule(this, token);
            if (!ImplicitNames.Contains(name))
            {
                rule = new NameRule(this, this, RuleName.Implicit).And(rule);
            }

            DefineRule(name, rule);
        }

        public NameRule ReferenceRule(string name)
        {
            AssertIsNotLocked();

            return new NameRule(this, this, name);
        }
    }
}
