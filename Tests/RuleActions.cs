using PseudoEBNF.Common;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Parsing.Rules;
using PseudoEBNF.Semantics;
using System;
using System.Linq;
using System.Text.RegularExpressions;

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
            branch = branch.Unwrap();

            var ident = branch.Branches[0].MatchedText.Trim();
            var name = new LeafSemanticNode((int)EbnfNodeType.Identifier, ident);

            var expr = recurse(branch.Branches[2].Unwrap());

            return new BranchSemanticNode((int)EbnfNodeType.Rule, new[] { name, expr });
        }

        internal static ISemanticNode Token(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            branch = branch.Unwrap();

            var ident = branch.Branches[0].MatchedText.Trim();
            var name = new LeafSemanticNode((int)EbnfNodeType.Identifier, ident);

            var expr = recurse(branch.Branches[2].Unwrap());

            return new BranchSemanticNode((int)EbnfNodeType.Token, new[] { name, expr });
        }

        internal static ISemanticNode Repeat(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            branch = branch.Unwrap();
            return new BranchSemanticNode((int)EbnfNodeType.Repeat, recurse(branch.Branches[1].Unwrap()));
        }

        internal static ISemanticNode Optional(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            branch = branch.Unwrap();
            return new BranchSemanticNode((int)EbnfNodeType.Optional, recurse(branch.Branches[1].Unwrap()));
        }

        internal static ISemanticNode Not(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            branch = branch.Unwrap();
            return new BranchSemanticNode((int)EbnfNodeType.Not, recurse(branch.Branches[1].Unwrap()));
        }

        internal static ISemanticNode Group(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            branch = branch.Unwrap();
            return new BranchSemanticNode((int)EbnfNodeType.Group, recurse(branch.Branches[1].Unwrap()));
        }

        internal static ISemanticNode Or(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            branch = branch.Unwrap();

            var left = recurse(branch.Branches[0].Unwrap());
            var right = recurse(branch.Branches[2].Unwrap());

            return new BranchSemanticNode((int)EbnfNodeType.Or, left, right);
        }

        internal static ISemanticNode And(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            branch = branch.Unwrap();

            var left = recurse(branch.Branches[0].Unwrap());
            var right = recurse(branch.Branches[1].Unwrap());

            return new BranchSemanticNode((int)EbnfNodeType.And, left, right);
        }
    }
}
