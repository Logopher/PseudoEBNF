using System;
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
        private readonly LexingParser parser;

        public BootstrapParser()
        {
            parser = new LexingParser(Grammar);
        }

        public override ISemanticNode ParseSemantics(BranchParseNode node) => parser.ParseSemantics(node);

        public override BranchParseNode ParseSyntax(string input) => parser.ParseSyntax(input);
    }
}
