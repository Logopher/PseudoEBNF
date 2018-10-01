using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Parsing.Rules;
using PseudoEBNF.Reporting;
using PseudoEBNF.Semantics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PseudoEBNF.Parsing
{
    public class Parser
    {
        readonly Supervisor super = new Supervisor();
        readonly Grammar grammar = new Grammar();
        public IReadOnlyList<string> ImplicitNames => grammar.ImplicitNames;
        
        public Parser()
        {
        }

        public IRule DefineRule(string name, IRule rule)
        {
            return grammar.DefineRule(name, rule);
        }

        internal NamedRule GetRule(string name)
        {
            return grammar.GetRule(name);
        }

        public ISemanticNode Parse(string input)
        {
            var parseTree = ParseSyntax(input);

            var semanticTree = ParseSemantics(parseTree);

            return semanticTree;
        }

        public BranchParseNode ParseSyntax(string input)
        {
            var grammar = this.grammar.Clone();

            grammar.DefineRule(RuleName.Implicit, new RepeatRule(
                new OrRule(grammar.ImplicitNames
                    .Select(GetRule)
                    .Where(r => r != null))));

            grammar.AttachAction(RuleName.Implicit, (n, r) => null);

            grammar.Lock();

            var lexer = new Lexer(grammar);
            var lexemes = lexer.Lex(input).ToList();

            var match = grammar.RootRule.Match(super, grammar, lexemes);
            var parseTree = match.Success ? (BranchParseNode)match.Result : null;

            if (parseTree == null || parseTree.Length != input.Length)
            {
                return null;
            }

            return parseTree;
        }

        ISemanticNode ParseSemantics(BranchParseNode node)
        {
            if (node.Rule is NamedRule named)
            {
                return named.Action(node, ParseSemantics);
            }
            else
            {
                throw new Exception();
            }
        }

        public void AttachAction(string name, Func<BranchParseNode, Func<BranchParseNode, ISemanticNode>, ISemanticNode> action)
        {
            grammar.AttachAction(name, action);
        }

        public void SetImplicit(string name)
        {
            grammar.SetImplicit(name);
        }

        public IRule DefineString(string name, string value)
        {
            return grammar.DefineString(name, value);
        }

        public IRule DefineRegex(string name, string value)
        {
            return grammar.DefineRegex(name, value);
        }
    }
}
