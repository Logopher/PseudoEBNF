using System;
using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Parsing.Rules;
using PseudoEBNF.Reporting;
using PseudoEBNF.Semantics;

namespace PseudoEBNF
{
    public abstract class Parser : Compatible
    {
        public enum Algorithm
        {
            LL,
        }

        public enum NestingType
        {
            Recursion,
            Stack,
        }

        public enum Unit
        {
            Lexeme,
            Character,
        }

        public Supervisor Super { get; }
        public Grammar Grammar { get; }
        public bool IsLocked => Grammar.IsLocked;

        public Parser()
            : this(new Grammar())
        {
        }

        public Parser(Grammar g)
            : base(g)
        {
            Grammar = g;
            Super = Grammar.Super;
        }
        
        public ISemanticNode Parse(string input)
        {
            if (!IsLocked)
            { throw new Exception(); }

            BranchParseNode parseTree = ParseSyntax(input);

            ISemanticNode semanticTree = ParseSemantics(parseTree);

            return semanticTree;
        }

        public abstract BranchParseNode ParseSyntax(string input);

        public abstract ISemanticNode ParseSemantics(BranchParseNode node);

        public void DefineRule(string name, Rule rule)
        {
            if (IsLocked)
            { throw new Exception(); }

            Grammar.DefineRule(name, rule);
        }

        public void DefineString(string name, string value)
        {
            if (IsLocked)
            { throw new Exception(); }

            Grammar.DefineString(name, value);
        }

        public void DefineRegex(string name, string value)
        {
            if (IsLocked)
            { throw new Exception(); }

            Grammar.DefineRegex(name, value);
        }

        public void AttachAction(string name, Func<BranchParseNode, Func<BranchParseNode, ISemanticNode>, ISemanticNode> action)
        {
            if (IsLocked)
            { throw new Exception(); }

            Grammar.AttachAction(name, action);
        }

        public void SetImplicit(string name)
        {
            if (IsLocked)
            { throw new Exception(); }

            Grammar.SetImplicit(name);
        }

        public void Lock()
        {
            if (!IsLocked)
            {
                Grammar.Lock();
            }
        }

        public NamedRule GetRule(string name)
        {
            if (!IsLocked)
            { throw new Exception(); }

            return Grammar.GetRule(name);
        }

        public Token GetToken(string name)
        {
            if (!IsLocked)
            { throw new Exception(); }

            return Grammar.GetToken(name);
        }

        public NameRule ReferenceRule(string name)
        {
            if (IsLocked)
            { throw new Exception(); }

            return Grammar.ReferenceRule(name);
        }
    }
}