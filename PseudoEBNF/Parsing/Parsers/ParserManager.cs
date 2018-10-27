using System;
using PseudoEBNF.Common;
using PseudoEBNF.Parsing.Nodes;

namespace PseudoEBNF.Parsing.Parsers
{
    public class ParserManager : Parser
    {
        private Parser parser;

        public ParserManager(Grammar grammar, ParserSettings settings)
            : this(grammar, settings.Algorithm, settings.NestingType, settings.Unit)
        {
        }

        public ParserManager(Grammar grammar, Algorithm algo, NestingType nesting, Unit unit)
            : base(grammar)
        {
            switch (algo)
            {
                case Algorithm.LL:
                    if (nesting == NestingType.Recursion && unit == Unit.Lexeme)
                    {
                        parser = new LexingParser(Grammar);
                    }
                    else if (nesting == NestingType.Stack && unit == Unit.Character)
                    {
                        parser = new StackParser(Grammar);
                    }
                    break;
                default:
                    throw new Exception();
            }
        }

        public ParserManager(Algorithm algo, NestingType nesting, Unit unit)
            : base()
        {
            switch (algo)
            {
                case Algorithm.LL:
                    if (nesting == NestingType.Recursion && unit == Unit.Lexeme)
                    {
                        parser = new LexingParser(Grammar);
                    }
                    else if (nesting == NestingType.Stack && unit == Unit.Character)
                    {
                        parser = new StackParser(Grammar);
                    }
                    break;
                default:
                    throw new Exception();
            }
        }

        public override BranchParseNode ParseSyntax(string input) => parser.ParseSyntax(input);
    }
}
