using PseudoEBNF.Common;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Semantics;
using System;
using System.Linq;

namespace PseudoEBNF.PseudoEBNF
{
    public static class RuleActions
    {
        internal static ISemanticNode String(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            branch = branch.Unwrap();

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
            branch = branch.Unwrap();

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

        internal static ISemanticNode Unwrap(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            return recurse(branch.GetDescendant(0));
        }

        internal static ISemanticNode Whitespace(BranchParseNode node, Func<BranchParseNode, ISemanticNode> recurse)
        {
            return null;
        }

        internal static ISemanticNode Rule(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            var name = recurse(branch.GetDescendant(0));
            var expr = recurse(branch.GetDescendant(2));

            return new BranchSemanticNode((int)EbnfNodeType.Rule, name.StartIndex, new[] { name, expr });
        }

        internal static ISemanticNode Token(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            var name = recurse(branch.GetDescendant(0));
            var expr = recurse(branch.GetDescendant(2));

            return new BranchSemanticNode((int)EbnfNodeType.Token, name.StartIndex, new[] { name, expr });
        }

        internal static ISemanticNode Repeat(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            var repeat = recurse(branch.GetDescendant(1));

            return new BranchSemanticNode((int)EbnfNodeType.Repeat, repeat);
        }

        internal static ISemanticNode Optional(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            var opt = recurse(branch.GetDescendant(1));

            return new BranchSemanticNode((int)EbnfNodeType.Optional, opt);
        }

        internal static ISemanticNode Not(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            var not = recurse(branch.GetDescendant(1));

            return new BranchSemanticNode((int)EbnfNodeType.Not, not);
        }

        internal static ISemanticNode Group(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            var group = recurse(branch.GetDescendant(1));

            return new BranchSemanticNode((int)EbnfNodeType.Group, group);
        }

        internal static ISemanticNode Or(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            var left = recurse(branch.GetDescendant(0));
            var right = recurse(branch.GetDescendant(2));

            return new BranchSemanticNode((int)EbnfNodeType.Or, left, right);
        }

        internal static ISemanticNode And(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            var left = recurse(branch.GetDescendant(0));
            var right = recurse(branch.GetDescendant(1));

            return new BranchSemanticNode((int)EbnfNodeType.And, left, right);
        }

        internal static ISemanticNode Root(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            var elements = branch.Elements;

            var children = elements
                .Select(recurse)
                .ToArray();

            return new BranchSemanticNode((int)EbnfNodeType.Root, branch.StartIndex, children);
        }

        internal static ISemanticNode LineComment(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            return null;
        }
    }
}
