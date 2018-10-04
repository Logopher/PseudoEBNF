using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Parsing.Rules;
using PseudoEBNF.Reporting;
using PseudoEBNF.Semantics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PseudoEBNF.Common
{
    public class Grammar:ICompatible
    {
        public Guid CompatibilityGuid { get; }

        Guid guid = Guid.NewGuid();

        public IRule RootRule => GetRule(RuleName.Root);

        public Supervisor Super { get; }

        readonly Dictionary<string, IToken> tokens;
        public IReadOnlyDictionary<string, IToken> Tokens => tokens;

        readonly Dictionary<string, NamedRule> rules;
        public IReadOnlyDictionary<string, NamedRule> Rules => rules;

        readonly List<string> implicitNames;
        public IReadOnlyList<string> ImplicitNames => implicitNames;

        public bool IsLocked { get; private set; }

        internal Grammar(Guid compatibilityGuid, Supervisor super)
        {
            CompatibilityGuid = compatibilityGuid;
            Super = super;
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
            DefineRule(RuleName.Implicit, new RepeatRule(CompatibilityGuid,
                new OrRule(CompatibilityGuid, ImplicitNames
                    .Select(GetRule)
                    .Where(r => r != null))));

            AttachAction(RuleName.Implicit, (n, r) => null);

            IsLocked = true;
        }

        public void DefineRule(string name, IRule rule)
        {
            if (IsLocked)
            {
                throw new Exception();
            }

            if (name == RuleName.Root)
            {
                rule = rule.And(new NameRule(CompatibilityGuid, this, RuleName.Implicit));
            }

            var named = new NamedRule(CompatibilityGuid, Super, name, rule);

            rules.Add(name, named);
        }

        public NamedRule GetRule(string name)
        {
            Rules.TryGetValue(name, out NamedRule result);
            return result;
        }

        public IToken GetToken(string name)
        {
            Tokens.TryGetValue(name, out IToken result);
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

        public void DefineString(string name, string text)
        {
            DefineToken(name, new StringToken(CompatibilityGuid, name, text));
        }

        public void DefineRegex(string name, string pattern)
        {
            DefineToken(name, new RegexToken(CompatibilityGuid, name, pattern));
        }

        void DefineToken(string name, IToken token)
        {
            if (IsLocked)
            {
                throw new Exception();
            }

            tokens.Add(name, token);

            IRule rule = new TokenRule(CompatibilityGuid, token);
            if (!ImplicitNames.Contains(name))
            {
                rule = new NameRule(CompatibilityGuid, this, RuleName.Implicit).And(rule);
            }

            DefineRule(name, rule);
        }
    }
}
