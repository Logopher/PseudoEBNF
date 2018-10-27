using System;
using System.Collections.Generic;
using System.Linq;
using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Parsing.Rules;
using PseudoEBNF.Reporting;
using PseudoEBNF.Semantics;

namespace PseudoEBNF
{
    public class LexingParser : Parser
    {
        public Supervisor Super { get; }
        public override Grammar Grammar { get; }
        public bool IsLocked => Grammar.IsLocked;

        public LexingParser(Grammar grammar)
            : base(grammar)
        {
            Super = grammar.Super;
            Grammar = grammar;
        }

        public override void Lock()
        {
            if (!IsLocked)
            {
                Grammar.Lock();
            }
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

            return Grammar.ReferenceRule(name);
        }

        public override ISemanticNode Parse(string input)
        {
            if (!IsLocked)
            { throw new Exception(); }

            BranchParseNode parseTree = ParseSyntax(input);

            ISemanticNode semanticTree = ParseSemantics(parseTree);

            return semanticTree;
        }

        public override BranchParseNode ParseSyntax(string input)
        {
            IEnumerable<Lexeme> lexemes = Lex(input);

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

            Match<IParseNode> match = Grammar.RootRule.Match(lexemes.ToList());

            return match.Success ? (BranchParseNode)match.Result : null;
        }

        public IEnumerable<Lexeme> Lex(string input)
        {
            if (!IsLocked)
            { throw new Exception(); }

            var lexer = new Lexer(Grammar);

            return lexer.Lex(input);
        }

        public override void AttachAction(string name, Func<BranchParseNode, Func<BranchParseNode, ISemanticNode>, ISemanticNode> action)
        {
            if (IsLocked)
            { throw new Exception(); }

            Grammar.AttachAction(name, action);
        }

        public override void SetImplicit(string name)
        {
            if (IsLocked)
            { throw new Exception(); }

            Grammar.SetImplicit(name);
        }

        public override void DefineString(string name, string value)
        {
            if (IsLocked)
            { throw new Exception(); }

            Grammar.DefineString(name, value);
        }

        public override void DefineRegex(string name, string value)
        {
            if (IsLocked)
            { throw new Exception(); }

            Grammar.DefineRegex(name, value);
        }
    }
}