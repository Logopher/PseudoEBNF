using PseudoEBNF.Common;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Semantics;
using System;
using System.Linq;

namespace PseudoEBNF.PseudoEBNF
{
    public static class RuleActions
    {
        internal static ISemanticNode String(IParseNode node, Func<IParseNode, ISemanticNode> recurse)
        {
            var text = node.Unwrap().MatchedText;
            text = text
                .Substring(1, text.Length - 2)
                .Replace(@"\\", @"\")
                .Replace(@"\""", @"""");

            return new LeafSemanticNode((int)EbnfNodeType.String, text);
        }

        internal static ISemanticNode Regex(IParseNode node, Func<IParseNode, ISemanticNode> recurse)
        {
            var text = node.Unwrap().MatchedText;
            text = text
                .Substring(1, text.Length - 2)
                .Replace(@"\\", @"\")
                .Replace(@"\/", @"/");

            return new LeafSemanticNode((int)EbnfNodeType.Regex, text);
        }

        internal static ISemanticNode Identifier(IParseNode node, Func<IParseNode, ISemanticNode> recurse)
        {
            var text = node.Unwrap().MatchedText;

            return new LeafSemanticNode((int)EbnfNodeType.Identifier, text);
        }

        internal static ISemanticNode Whitespace(IParseNode node, Func<IParseNode, ISemanticNode> recurse)
        {
            return null;
        }

        internal static ISemanticNode Rule(IParseNode node, Func<IParseNode, ISemanticNode> recurse)
        {
            var name = recurse(node.GetDescendant(0));
            var expr = recurse(node.GetDescendant(2));

            return new BranchSemanticNode((int)EbnfNodeType.Rule, new[] { name, expr });
        }

        internal static ISemanticNode Token(IParseNode node, Func<IParseNode, ISemanticNode> recurse)
        {
            var name = recurse(node.GetDescendant(0));
            var expr = recurse(node.GetDescendant(2));

            return new BranchSemanticNode((int)EbnfNodeType.Token, new[] { name, expr });
        }

        internal static ISemanticNode Repeat(IParseNode node, Func<IParseNode, ISemanticNode> recurse)
        {
            var repeat = recurse(node.GetDescendant(1));

            return new BranchSemanticNode((int)EbnfNodeType.Repeat, repeat);
        }

        internal static ISemanticNode Optional(IParseNode node, Func<IParseNode, ISemanticNode> recurse)
        {
            var opt = recurse(node.GetDescendant(1));

            return new BranchSemanticNode((int)EbnfNodeType.Optional, opt);
        }

        internal static ISemanticNode Not(IParseNode node, Func<IParseNode, ISemanticNode> recurse)
        {
            var not = recurse(node.GetDescendant(1));

            return new BranchSemanticNode((int)EbnfNodeType.Not, not);
        }

        internal static ISemanticNode Group(IParseNode node, Func<IParseNode, ISemanticNode> recurse)
        {
            var group = recurse(node.GetDescendant(1));

            return new BranchSemanticNode((int)EbnfNodeType.Group, group);
        }

        internal static ISemanticNode Or(IParseNode node, Func<IParseNode, ISemanticNode> recurse)
        {
            var left = recurse(node.GetDescendant(0));
            var right = recurse(node.GetDescendant(2));

            return new BranchSemanticNode((int)EbnfNodeType.Or, left, right);
        }

        internal static ISemanticNode And(IParseNode node, Func<IParseNode, ISemanticNode> recurse)
        {
            var left = recurse(node.GetDescendant(0));
            var right = recurse(node.GetDescendant(1));

            return new BranchSemanticNode((int)EbnfNodeType.And, left, right);
        }

        internal static ISemanticNode Root(IParseNode node, Func<IParseNode, ISemanticNode> recurse)
        {
            var children = ((BranchParseNode)node.GetDescendant(0))
                .Children.Select(n => recurse(n.Unwrap()))
                .ToList();

            return new BranchSemanticNode((int)EbnfNodeType.Root, children);
        }

        internal static ISemanticNode LineComment(IParseNode node, Func<IParseNode, ISemanticNode> recurse)
        {
            return null;
        }
    }
}
