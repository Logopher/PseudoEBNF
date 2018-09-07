using PseudoEBNF.Common;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Parsing.Rules;
using PseudoEBNF.Semantics;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace PseudoEBNF.PseudoEBNF
{
    public static class RuleActions
    {
        internal static ISemanticNode String(IParseNode node, Func<IParseNode, ISemanticNode> recurse)
        {
            var branch = (BranchParseNode)node.Unwrap();

            var text = branch.Children[1].MatchedText;
            text = text
                .Substring(1, text.Length - 2)
                .Replace(@"\\", @"\")
                .Replace(@"\""", @"""");

            return new LeafSemanticNode((int)EbnfNodeType.String, text);
        }

        internal static ISemanticNode Regex(IParseNode node, Func<IParseNode, ISemanticNode> recurse)
        {
            var branch = (BranchParseNode)node.Unwrap();

            var text = branch.Children[1].MatchedText;
            text = text
                .Substring(1, text.Length - 2)
                .Replace(@"\\", @"\")
                .Replace(@"\/", @"/");

            return new LeafSemanticNode((int)EbnfNodeType.Regex, text);
        }

        internal static ISemanticNode Identifier(IParseNode node, Func<IParseNode, ISemanticNode> recurse)
        {
            var branch = (BranchParseNode)node.Unwrap();

            var text = branch.Children[1].MatchedText;

            return new LeafSemanticNode((int)EbnfNodeType.Identifier, text);
        }

        internal static ISemanticNode Whitespace(IParseNode node, Func<IParseNode, ISemanticNode> recurse)
        {
            return null;
        }

        internal static ISemanticNode Rule(IParseNode node, Func<IParseNode, ISemanticNode> recurse)
        {
            var branch = (BranchParseNode)node.Unwrap();

            var name = recurse(branch.Children[0].Unwrap());
            var expr = recurse(branch.Children[2].Unwrap());

            return new BranchSemanticNode((int)EbnfNodeType.Rule, new[] { name, expr });
        }

        internal static ISemanticNode Token(IParseNode node, Func<IParseNode, ISemanticNode> recurse)
        {
            var branch = (BranchParseNode)node.Unwrap();

            var name = recurse(branch.Children[0].Unwrap());
            var expr = recurse(branch.Children[2].Unwrap());

            return new BranchSemanticNode((int)EbnfNodeType.Token, new[] { name, expr });
        }

        internal static ISemanticNode Repeat(IParseNode node, Func<IParseNode, ISemanticNode> recurse)
        {
            var branch = (BranchParseNode)node.Unwrap();
            return new BranchSemanticNode((int)EbnfNodeType.Repeat, recurse(branch.Children[1].Unwrap()));
        }

        internal static ISemanticNode Optional(IParseNode node, Func<IParseNode, ISemanticNode> recurse)
        {
            var branch = (BranchParseNode)node.Unwrap();
            return new BranchSemanticNode((int)EbnfNodeType.Optional, recurse(branch.Children[1].Unwrap()));
        }

        internal static ISemanticNode Not(IParseNode node, Func<IParseNode, ISemanticNode> recurse)
        {
            var branch = (BranchParseNode)node.Unwrap();
            return new BranchSemanticNode((int)EbnfNodeType.Not, recurse(branch.Children[1].Unwrap()));
        }

        internal static ISemanticNode Group(IParseNode node, Func<IParseNode, ISemanticNode> recurse)
        {
            var branch = (BranchParseNode)node.Unwrap();
            return new BranchSemanticNode((int)EbnfNodeType.Group, recurse(branch.Children[1].Unwrap()));
        }

        internal static ISemanticNode Or(IParseNode node, Func<IParseNode, ISemanticNode> recurse)
        {
            var branch = (BranchParseNode)node.Unwrap();

            var left = recurse(branch.Children[0].Unwrap());
            var right = recurse(branch.Children[2].Unwrap());

            return new BranchSemanticNode((int)EbnfNodeType.Or, left, right);
        }

        internal static ISemanticNode And(IParseNode node, Func<IParseNode, ISemanticNode> recurse)
        {
            var branch = (BranchParseNode)node.Unwrap();

            var left = recurse(branch.Children[0].Unwrap());
            var right = recurse(branch.Children[1].Unwrap());

            return new BranchSemanticNode((int)EbnfNodeType.And, left, right);
        }

        internal static ISemanticNode Root(IParseNode node, Func<IParseNode, ISemanticNode> recurse)
        {
            var branch = (BranchParseNode)node.Unwrap();

            var children = ((BranchParseNode)branch.Children[0].Unwrap())
                .Children
                .Select(Unwrap)
                .Select(recurse);

            return new BranchSemanticNode((int)EbnfNodeType.Root, children);
        }

        internal static ISemanticNode LineComment(IParseNode node, Func<IParseNode, ISemanticNode> recurse)
        {
            return null;
        }

        static IParseNode Unwrap(this IParseNode node)
        {
            if (node is BranchParseNode branch)
            {
                if (branch.Children.Count == 1)
                {
                    if (branch.Children[0] is BranchParseNode child)
                    {
                        if (child.Rule is NamedRule)
                        {
                            return child;
                        }
                        else
                        {
                            return Unwrap(child);
                        }
                    }
                }
                else
                {
                    return branch;
                }
            }

            throw new Exception("LeafParseNode unprotected by NamedRule actions.");
        }
    }
}
