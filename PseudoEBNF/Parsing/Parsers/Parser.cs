using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Parsing.Rules;
using PseudoEBNF.Semantics;
using System;

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

        public Parser()
            : base(Guid.NewGuid())
        { }

        public Parser(Compatible c)
            : base(c)
        { }

        public abstract void Lock();

        public abstract ISemanticNode Parse(string input);

        public abstract NamedRule GetRule(string name);

        public abstract Token GetToken(string name);

        public abstract BranchParseNode ParseSyntax(string input);

        public abstract ISemanticNode ParseSemantics(BranchParseNode node);

        public abstract NameRule ReferenceRule(string name);

        public abstract void AttachAction(string name, Func<BranchParseNode, Func<BranchParseNode, ISemanticNode>, ISemanticNode> action);
    }
}