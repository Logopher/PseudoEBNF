using PseudoEBNF.Parsing.Nodes;

namespace PseudoEBNF.Parsing.Parsers
{
    internal class BootstrapParser : Parser
    {
        private readonly LexingParser parser;

        public BootstrapParser()
        {
            parser = new LexingParser(Grammar);
        }

        public override BranchParseNode ParseSyntax(string input) => parser.ParseSyntax(input);
    }
}
