using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Parsing.Rules;
using PseudoEBNF.Reporting;
using PseudoEBNF.Semantics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PseudoEBNF
{
    public class LexingParser : Parser
    {
        public Supervisor Super { get; }
        public Grammar Grammar { get; }
        public bool IsLocked => Grammar.IsLocked;

        public LexingParser()
            : base()
        {
            Super = new Supervisor();
            Grammar = new Grammar(this, Super);
        }

        public override void Lock()
        {
            if (!IsLocked)
            {
                Grammar.Lock();
            }
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

        public override NamedRule GetRule(string name)
        {
            if (!IsLocked)
            { throw new Exception(); }

            return Grammar.GetRule(name);
        }

        public override Token GetToken(string name)
        {
            if (!IsLocked)
            { throw new Exception(); }

            return Grammar.GetToken(name);
        }

        public override NameRule ReferenceRule(string name)
        {
            if (IsLocked)
            { throw new Exception(); }

            return new NameRule(this, Grammar, name);
        }

        public override ISemanticNode Parse(string input)
        {
            if (!IsLocked)
            { throw new Exception(); }

            var lexemes = Lex(input);

            var parseTree = ParseSyntax(lexemes);

            var semanticTree = ParseSemantics(parseTree);

            return semanticTree;
        }

        public override BranchParseNode ParseSyntax(string input)
        {
            var lexemes = Lex(input);

            return ParseSyntax(lexemes);
        }

        public override ISemanticNode ParseSemantics(BranchParseNode node)
        {
            if (!IsLocked)
            { throw new Exception(); }

            if (node.Rule is NamedRule named)
            { return named.Action(node, ParseSemantics); }
            else
            { throw new Exception(); }
        }

        public BranchParseNode ParseSyntax(IEnumerable<Lexeme> lexemes)
        {
            if (!IsLocked)
            { throw new Exception(); }

            var match = Grammar.RootRule.Match(lexemes.ToList());

            return match.Success ? (BranchParseNode)match.Result : null;
        }

        public IEnumerable<Lexeme> Lex(string input)
        {
            if (!IsLocked)
            { throw new Exception(); }

            var lexer = new Lexer(Super, Grammar);

            return lexer.Lex(input);
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
    }
}