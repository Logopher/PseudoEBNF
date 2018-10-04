using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Parsing.Rules;
using PseudoEBNF.Semantics;
using System.Collections.Generic;

namespace PseudoEBNF
{
    public interface IParser : ICompatible
    {
        void Lock();

        ISemanticNode Parse(string input);

        NamedRule GetRule(string name);

        IToken GetToken(string name);

        IParseNode ParseSyntax(string input);

        IEnumerable<Lexeme> Lex(string input);

        BranchParseNode ParseSyntax(IEnumerable<Lexeme> lexemes);

        ISemanticNode ParseSemantics(BranchParseNode node);
    }
}