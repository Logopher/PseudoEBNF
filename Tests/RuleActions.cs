using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Semantics;
using System;

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
            return new LeafSemanticNode((int)EbnfNodeType.String, text);
        }

        internal static ISemanticNode Regex(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            var text = branch.Leaf.MatchedText;
            text = text
                .Substring(1, text.Length - 2)
                .Replace(@"\\", @"\")
                .Replace(@"\/", @"/");
            return new LeafSemanticNode((int)EbnfNodeType.Regex, text);
        }

        internal static ISemanticNode Identifier(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            var text = branch.Leaf.MatchedText;
            return new LeafSemanticNode((int)EbnfNodeType.Identifier, text);
        }

        internal static ISemanticNode Whitespace(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            return null;
        }

        internal static ISemanticNode Rule(BranchParseNode branch, Func<IParseNode, ISemanticNode> recurse)
        {
            var ident = branch.GetDescendant(0).Leaf.MatchedText;
            var name = new LeafSemanticNode((int)EbnfNodeType.Identifier, ident);

            var expr = recurse(branch.GetDescendant(2));

            return new BranchSemanticNode((int)EbnfNodeType.Rule, new[] { name, expr });
        }

        internal static ISemanticNode Token(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            var ident = branch.GetDescendant(0).Leaf.MatchedText;
            var name = new LeafSemanticNode((int)EbnfNodeType.Identifier, ident);

            var expr = recurse(branch.GetDescendant(2));

            return new BranchSemanticNode((int)EbnfNodeType.Token, new[] { name, expr });
        }

        internal static ISemanticNode Repeat(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            return new BranchSemanticNode((int)EbnfNodeType.Repeat, recurse(branch.GetDescendant(1)));
        }

        internal static ISemanticNode Optional(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            return new BranchSemanticNode((int)EbnfNodeType.Optional, recurse(branch.GetDescendant(1)));
        }

        internal static ISemanticNode Not(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            return new BranchSemanticNode((int)EbnfNodeType.Not, recurse(branch.GetDescendant(1)));
        }

        internal static ISemanticNode Group(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            return new BranchSemanticNode((int)EbnfNodeType.Group, recurse(branch.GetDescendant(1)));
        }

        internal static ISemanticNode Or(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            var left = recurse(branch.GetDescendant(0));
            var right = recurse(branch.GetDescendant(2));

            return new BranchSemanticNode((int)EbnfNodeType.Or, left, right);
        }

        internal static ISemanticNode And(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            branch = branch.Unwrap();

            var left = recurse(branch.GetDescendant(0));
            var right = recurse(branch.GetDescendant(1));

            return new BranchSemanticNode((int)EbnfNodeType.And, left, right);
        }
    }
}
