using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Parsing.Parsers;
using PseudoEBNF.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PseudoEBNF.Parsing.Rules
{
    public class TokenRule : Rule
    {
        public override StackParser.Action SuccessAction { get; } = StackParser.Action.NextSibling;
        public override StackParser.Action FailureAction { get; } = StackParser.Action.Cancel;

        public Token Token { get; }

        public Supervisor Super { get; }

        public Grammar Grammar { get; }

        public override IReadOnlyList<Rule> Children { get; } = new Rule[0];

        public TokenRule(Compatible c, Token token)
            : base(c)
        {
            if (!IsCompatibleWith(token))
            { throw new Exception(); }

            Token = token;
        }

        public override Rule Clone()
        {
            return new TokenRule(this, Token.Clone());
        }

        public Match<IParseNode> Match(string input, int index)
        {
            var match = Token.Match(input, index);

            if(match.Success)
            {
                return new Match<IParseNode>(new LeafParseNode(this, index, match.Result), true);
            }

            return new Match<IParseNode>(null, false);
        }

        public override bool IsFull(IReadOnlyList<IParseNode> nodes)
        {
            throw new Exception();
        }

        public override bool IsComplete(IReadOnlyList<IParseNode> nodes)
        {
            throw new Exception();
        }

        public override bool IsExhausted(int ruleIndex)
        {
            throw new Exception();
        }

        public override string ToString()
        {
            return $"{{rule {Token}}}";
        }

        public override Match<IParseNode> Match(List<Lexeme> lexemes)
        {
            var first = lexemes.FirstOrDefault();
            if (first?.Token.Guid == Token.Guid)
            {
                return new Match<IParseNode>(new LeafParseNode(this, first.StartIndex, first), true);
            }
            else
            {
                return new Match<IParseNode>(null, false);
            }
        }
    }
}
