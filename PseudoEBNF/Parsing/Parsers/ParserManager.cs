using System;
using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Parsing.Rules;
using PseudoEBNF.Reporting;
using PseudoEBNF.Semantics;

namespace PseudoEBNF.Parsing.Parsers
{
    public class ParserManager : Parser
    {
        public Supervisor Super { get; }
        public override Grammar Grammar { get; }
        public bool IsLocked => Grammar.IsLocked;

        private Parser parser;

        public ParserManager(Grammar grammar, ParserSettings settings)
            : this(grammar, settings.Algorithm, settings.NestingType, settings.Unit)
        {
        }

        public ParserManager(Grammar grammar, Algorithm algo, NestingType nesting, Unit unit)
        {
            Super = grammar.Super;
            Grammar = grammar;

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
        {
            Super = new Supervisor();
            Grammar = new Grammar(this, Super);

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

        public void DefineRule(string name, Rule rule)
        {
            if (IsLocked)
            { throw new Exception(); }

            Grammar.DefineRule(name, rule);
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

        public override void Lock() => parser.Lock();

        public override NamedRule GetRule(string name) => parser.GetRule(name);

        public override Token GetToken(string name) => parser.GetToken(name);

        public override NameRule ReferenceRule(string name) => parser.ReferenceRule(name);

        public override ISemanticNode Parse(string input) => parser.Parse(input);

        public override ISemanticNode ParseSemantics(BranchParseNode node) => parser.ParseSemantics(node);

        public override BranchParseNode ParseSyntax(string input) => parser.ParseSyntax(input);
    }
}
