using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Parsing.Rules;
using PseudoEBNF.Semantics;
using System;
using System.Collections.Generic;

namespace PseudoEBNF
{
    public abstract class Parser : Compatible
    {
        public Parser()
            : base(Guid.NewGuid())
        { }

        public abstract void Lock();

        public abstract ISemanticNode Parse(string input);

        public abstract NamedRule GetRule(string name);

        public abstract Token GetToken(string name);

        public abstract BranchParseNode ParseSyntax(string input);

        public abstract ISemanticNode ParseSemantics(BranchParseNode node);

        public abstract NameRule ReferenceRule(string name);
    }
}