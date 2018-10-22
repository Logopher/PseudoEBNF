using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Parsing.Rules;
using PseudoEBNF.Reporting;
using PseudoEBNF.Semantics;
using System;

namespace PseudoEBNF.Parsing.Parsers
{
    public class LLParser : Parser
    {
        public ParserType Type { get; }
        public Supervisor Super { get; }
        public Grammar Grammar { get; }

        public bool IsLocked => Grammar.IsLocked;

        public LLParser(ParserType type)
        {
            Type = type;
            Super = new Supervisor();
            Grammar = new Grammar(this, Super);
        }

        public override void Lock()
        {
            if (!IsLocked)
            { Grammar.Lock(); }
        }

        public void DefineRule(string name, Rule rule)
        {
            if (IsLocked)
            { throw new Exception(); }

            Grammar.DefineRule(name, rule);
        }

        public void DefineString(string name, string value)
        {
            if (IsLocked)
            { throw new Exception(); }

            Grammar.DefineString(name, value);
        }

        public void DefineRegex(string name, string value)
        {
            if (IsLocked)
            { throw new Exception(); }

            Grammar.DefineRegex(name, value);
        }

        public override NamedRule GetRule(string name)
        {
            return Grammar.GetRule(name);
        }

        public override Token GetToken(string name)
        {
            return Grammar.GetToken(name);
        }

        public override NameRule ReferenceRule(string name)
        {
            if (IsLocked)
            { throw new Exception(); }

            return new NameRule(this, Grammar, name);
        }

        public override ISemanticNode Parse(string input)
        {
            if (!IsLocked)
            { throw new Exception(); }

            var parseTree = ParseSyntax(input);

            var semanticTree = ParseSemantics(parseTree);

            return semanticTree;
        }

        public override BranchParseNode ParseSyntax(string input)
        {
            if (!IsLocked)
            { throw new Exception(); }

            switch (Type)
            {
                case ParserType.LL_Lex:
                    {
                        var parser = new LexingParser(Grammar);

                        return parser.ParseSyntax(input);
                    }
                case ParserType.LL_Stack:
                    {
                        var parser = new StackParser(Grammar);

                        return parser.ParseSyntax(input);
                    }
                default:
                    throw new Exception();
            }
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

        public void AttachAction(string name, Func<BranchParseNode, Func<BranchParseNode, ISemanticNode>, ISemanticNode> action)
        {
            if (IsLocked)
            { throw new Exception(); }

            Grammar.AttachAction(name, action);
        }

        public void SetImplicit(string name)
        {
            if (IsLocked)
            { throw new Exception(); }

            Grammar.SetImplicit(name);
        }
    }
}