using System;
using System.Collections.Generic;
using System.Linq;
using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;

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

            BranchParseNode result = ParseSyntax(lexemes);

            if(result.MatchedText.Length != input.Length)
            { throw new Exception(); }

            return result;
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