using System;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Semantics;

namespace Tests
{
    public static class RuleActions
    {
        internal static ISemanticNode String(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            var text = branch.Leaf.MatchedText;
            text = text
                .Substring(1, text.Length - 2)
                .Replace(@"\\", @"\")
                .Replace(@"\""", @"""");
            var startIndex = branch.Leaf.StartIndex;

            return new LeafSemanticNode((int)EbnfNodeType.String, startIndex, text);
        }

        internal static ISemanticNode Regex(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            var text = branch.Leaf.MatchedText;
            text = text
                .Substring(1, text.Length - 2)
                .Replace(@"\\", @"\")
                .Replace(@"\/", @"/");
            var startIndex = branch.Leaf.StartIndex;

            return new LeafSemanticNode((int)EbnfNodeType.Regex, startIndex, text);
        }

        internal static ISemanticNode Identifier(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            var text = branch.Leaf.MatchedText;
            var startIndex = branch.Leaf.StartIndex;

            return new LeafSemanticNode((int)EbnfNodeType.Identifier, startIndex, text);
        }

        internal static ISemanticNode Whitespace(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse) => null;

        internal static ISemanticNode Rule(BranchParseNode branch, Func<IParseNode, ISemanticNode> recurse)
        {
            BranchParseNode identNode = branch.GetDescendant(0);
            var ident = identNode.Leaf.MatchedText;
            var startIndex = identNode.Leaf.StartIndex;

            var name = new LeafSemanticNode((int)EbnfNodeType.Identifier, startIndex, ident);

            ISemanticNode expr = recurse(branch.GetDescendant(2));

            return new BranchSemanticNode((int)EbnfNodeType.Rule, name.StartIndex, new[] { name, expr });
        }

        internal static ISemanticNode Token(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            BranchParseNode identNode = branch.GetDescendant(0);
            var ident = identNode.Leaf.MatchedText;
            var startIndex = identNode.Leaf.StartIndex;

            var name = new LeafSemanticNode((int)EbnfNodeType.Identifier, startIndex, ident);

            ISemanticNode expr = recurse(branch.GetDescendant(2));

            return new BranchSemanticNode((int)EbnfNodeType.Token, name.StartIndex, new[] { name, expr });
        }

        internal static ISemanticNode Repeat(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse) => new BranchSemanticNode((int)EbnfNodeType.Repeat, recurse(branch.GetDescendant(1)));

        internal static ISemanticNode Optional(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse) => new BranchSemanticNode((int)EbnfNodeType.Optional, recurse(branch.GetDescendant(1)));

        internal static ISemanticNode Not(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse) => new BranchSemanticNode((int)EbnfNodeType.Not, recurse(branch.GetDescendant(1)));

        internal static ISemanticNode Group(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse) => new BranchSemanticNode((int)EbnfNodeType.Group, recurse(branch.GetDescendant(1)));

        internal static ISemanticNode Or(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            ISemanticNode left = recurse(branch.GetDescendant(0));
            ISemanticNode right = recurse(branch.GetDescendant(2));

            return new BranchSemanticNode((int)EbnfNodeType.Or, left, right);
        }

        internal static ISemanticNode And(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            branch = branch.Unwrap();

            ISemanticNode left = recurse(branch.GetDescendant(0));
            ISemanticNode right = recurse(branch.GetDescendant(1));

            return new BranchSemanticNode((int)EbnfNodeType.And, left, right);
        }
    }
}
