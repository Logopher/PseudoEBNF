using System;
using System.Collections.Generic;
using System.Linq;
using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Parsing.Rules;
using PseudoEBNF.Semantics;

namespace PseudoEBNF
{
    public class LexingParser : Parser
    {
        public LexingParser(Grammar grammar)
            : base(grammar)
        {
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
    }
}