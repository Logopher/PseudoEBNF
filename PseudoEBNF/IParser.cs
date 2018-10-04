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

        public abstract IParseNode ParseSyntax(string input);

        public abstract IEnumerable<Lexeme> Lex(string input);

        public abstract BranchParseNode ParseSyntax(IEnumerable<Lexeme> lexemes);

        public abstract ISemanticNode ParseSemantics(BranchParseNode node);
    }
}