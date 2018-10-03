using PseudoEBNF.Common;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Parsing.Rules;
using PseudoEBNF.Semantics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PseudoEBNF.JavaScript
{
    internal static class RuleActions
    {
        internal static ISemanticNode Unwrap(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            return recurse(branch.GetDescendant(0));
        }

        internal static ISemanticNode CompositeExpression(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            var first = recurse(branch.GetDescendant(0));

            var fragments = branch.GetDescendant(1);

            return CompositeExpression(first, fragments, recurse);
        }

        private static ISemanticNode CompositeExpression(ISemanticNode first, BranchParseNode fragments, Func<BranchParseNode, ISemanticNode> recurse)
        {
            var rest = ExpressionFragments(fragments, recurse);

            return CompositeExpression(first, rest);
        }

        internal static ISemanticNode PropertyAssignment(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            var first = recurse(branch.GetDescendant(0));

            var fragments = branch.GetDescendant(1);
            
            var lvalue = CompositeExpression(first, fragments, recurse);
            var expr = recurse(branch.GetDescendant(3));

            return new BranchSemanticNode((int)JsNodeType.Assignment, lvalue, expr);
        }

        static List<ISemanticNode> ExpressionFragments(BranchParseNode fragments, Func<BranchParseNode, ISemanticNode> recurse)
        {
            var named = (NamedRule)fragments.Rule;

            var first = recurse(fragments.GetDescendant(0));
            ISemanticNode[] rest;

            var restNode = fragments.GetDescendant(1);
            if (restNode != null)
            {
                rest = restNode
                    .Elements
                    .Select(recurse)
                    .ToArray();

                return new[] { first }.Concat(rest).ToList();
            }
            else
            {
                return new[] { first }.ToList();
            }
        }

        static ISemanticNode CompositeExpression(ISemanticNode first, params ISemanticNode[] rest)
        {
            return CompositeExpression(first, (IEnumerable<ISemanticNode>)rest);
        }

        static ISemanticNode CompositeExpression(ISemanticNode first, IEnumerable<ISemanticNode> rest)
        {
            var result = first;

            foreach (var fragment in rest)
            {
                JsNodeType nodeType;
                switch ((JsNodeType)fragment.NodeType)
                {
                    case JsNodeType.ArgumentList:
                        nodeType = JsNodeType.FunctionCall;
                        break;
                    case JsNodeType.DotReference:
                    case JsNodeType.KeyReference:
                        nodeType = JsNodeType.Property;
                        break;
                    default:
                        throw new Exception();
                }

                result = new BranchSemanticNode((int)nodeType, result, fragment);
            }

            return result;
        }

        internal static ISemanticNode FunctionCall(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            var @ref = recurse(branch.GetDescendant(0));

            var first = branch.GetDescendant(1, 0, 1, 0);

            var fragments = branch.GetDescendant(1, 0, 1, 1);

            if (first != null)
            {
                if (fragments != null)
                {
                    var args = CompositeExpression(recurse(first), fragments, recurse);
                    return new BranchSemanticNode((int)JsNodeType.FunctionCall, @ref, args);
                }
                else
                {
                    return new BranchSemanticNode((int)JsNodeType.FunctionCall, @ref, recurse(first));
                }
            }
            else
            {
                return new BranchSemanticNode((int)JsNodeType.FunctionCall, @ref);
            }
        }

        internal static ISemanticNode Constructor(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            var funCall = recurse(branch.GetDescendant(1));

            return new BranchSemanticNode((int)JsNodeType.Constructor, funCall);
        }
    }
}