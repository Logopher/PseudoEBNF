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
        private readonly Guid guid = Guid.NewGuid();

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
            DefineRule(RuleName.Implicit, new RepeatRule(this,
                new OrRule(this, ImplicitNames
                    .Select(GetRule)
                    .Where(r => r != null))));

            AttachAction(RuleName.Implicit, (n, r) => null);

            IsLocked = true;
        }

        public void DefineRule(string name, Rule rule)
        {
            if (IsLocked)
            {
                throw new Exception();
            }

            if (name == RuleName.Root)
            {
                rule = rule.And(new NameRule(this, this, RuleName.Implicit));
            }

            var named = new NamedRule(this, Super, name, rule);

            rules.Add(name, named);
        }

        public NamedRule GetRule(string name)
        {
            Rules.TryGetValue(name, out NamedRule result);
            return result;
        }

        public Token GetToken(string name)
        {
            Tokens.TryGetValue(name, out Token result);
            return result;
        }

        internal void SetImplicit(string name)
        {
            if (IsLocked)
            { throw new Exception(); }

            implicitNames.Add(name);
        }

        internal void AttachAction(string name, Func<BranchParseNode, Func<BranchParseNode, ISemanticNode>, ISemanticNode> action) => GetRule(name).AttachAction(action);

        public void DefineString(string name, string text) => DefineToken(name, new StringToken(this, name, text));

        public void DefineRegex(string name, string pattern) => DefineToken(name, new RegexToken(this, name, pattern));

        private void DefineToken(string name, Token token)
        {
            if (IsLocked)
            {
                throw new Exception();
            }

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
            if (IsLocked)
            { throw new Exception(); }

            return new NameRule(this, this, name);
        }
    }
}
