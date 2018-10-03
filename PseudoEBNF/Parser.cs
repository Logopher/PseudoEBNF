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
    public class Parser : IParser
    {
        readonly Supervisor super = new Supervisor();
        readonly Grammar grammar = new Grammar();

        public Parser()
        {
        }

        public void DefineRule(string name, IRule rule)
        {
            grammar.DefineRule(name, rule);
        }

        public void DefineString(string name, string value)
        {
            grammar.DefineString(name, value);
        }

        public void DefineRegex(string name, string value)
        {
            grammar.DefineRegex(name, value);
        }

        public NamedRule GetRule(string name)
        {
            return grammar.GetRule(name);
        }

        public IToken GetToken(string name)
        {
            return grammar.GetToken(name);
        }

        public ISemanticNode Parse(string input)
        {
            var grammar = ProduceGrammar();

            var lexemes = Lex(grammar, input);

            var parseTree = ParseSyntax(grammar, lexemes);

            var semanticTree = ParseSemantics(parseTree);

            return semanticTree;
        }

        public IParseNode ParseSyntax(string input)
        {
            var grammar = ProduceGrammar();

            var lexemes = Lex(grammar, input);

            return ParseSyntax(grammar, lexemes);
        }

        public IEnumerable<Lexeme> Lex(string input)
        {
            var grammar = ProduceGrammar();

            return Lex(grammar, input);
        }

        public BranchParseNode ParseSyntax(IEnumerable<Lexeme> lexemes)
        {
            var grammar = ProduceGrammar();

            return ParseSyntax(grammar, lexemes);
        }

        public ISemanticNode ParseSemantics(BranchParseNode node)
        {
            if (node.Rule is NamedRule named)
            { return named.Action(node, ParseSemantics); }
            else
            { throw new Exception(); }
        }

        BranchParseNode ParseSyntax(Grammar grammar, IEnumerable<Lexeme> lexemes)
        {
            var match = grammar.RootRule.Match(super, grammar, lexemes.ToList());

            return match.Success ? (BranchParseNode)match.Result : null;
        }

        Grammar ProduceGrammar()
        {
            var grammar = this.grammar.Clone();

            grammar.Lock();

            return grammar;
        }

        IEnumerable<Lexeme> Lex(Grammar grammar, string input)
        {
            var lexer = new Lexer(grammar);

            return lexer.Lex(super, input);
        }

        public void AttachAction(string name, Func<BranchParseNode, Func<BranchParseNode, ISemanticNode>, ISemanticNode> action)
        {
            grammar.AttachAction(name, action);
        }

        public void SetImplicit(string name)
        {
            grammar.SetImplicit(name);
        }
    }
}