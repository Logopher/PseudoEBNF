using System;
using System.Collections.Generic;
using System.Linq;
using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Parsing.Rules;
using PseudoEBNF.Reporting;
using PseudoEBNF.Semantics;

namespace PseudoEBNF.Parsing.Parsers
{
    internal class BootstrapParser : Parser
    {
        public Supervisor Super { get; }
        public Grammar Grammar { get; }
        public bool IsLocked => Grammar.IsLocked;

        private readonly LexingParser parser;

        public BootstrapParser()
        {
            Super = new Supervisor();
            Grammar = new Grammar(this, Super);

            parser = new LexingParser(Grammar);
        }

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

        public override void AttachAction(string name, Func<BranchParseNode, Func<BranchParseNode, ISemanticNode>, ISemanticNode> action)
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

        public override void Lock() => parser.Lock();

        public override NamedRule GetRule(string name) => parser.GetRule(name);

        public override Token GetToken(string name) => parser.GetToken(name);

        public override NameRule ReferenceRule(string name) => parser.ReferenceRule(name);

        public override ISemanticNode Parse(string input) => parser.Parse(input);

        public override ISemanticNode ParseSemantics(BranchParseNode node) => parser.ParseSemantics(node);

        public override BranchParseNode ParseSyntax(string input) => parser.ParseSyntax(input);
    }
}
