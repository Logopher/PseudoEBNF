using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Parsing.Rules;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PseudoEBNF.Common
{
    public static class Extensions
    {
        public static AndRule And(this IRule rule, IRule first, params IRule[] rest)
        {
            return new AndRule(new[] { rule, first }.Concat(rest));
        }

        public static OrRule Or(this IRule rule, IRule first, params IRule[] rest)
        {
            return new OrRule(new[] { rule, first }.Concat(rest));
        }

        public static Dictionary<K, V> Merge<K, V>(this IDictionary<K, V> self, DictionaryMergeCollisionBehavior collisionBehavior, IEnumerable<IDictionary<K, V>> rest)
        {
            var result = self.ToDictionary(p => p.Key, p => p.Value);

            foreach (var d in rest)
            {
                foreach (var pair in d)
                {
                    var key = pair.Key;
                    var value = pair.Value;

                    if (result.ContainsKey(key))
                    {
                        switch (collisionBehavior)
                        {
                            case DictionaryMergeCollisionBehavior.Exception:
                                throw new Exception();
                            case DictionaryMergeCollisionBehavior.ChooseLeft:
                                value = result[key];
                                break;
                            case DictionaryMergeCollisionBehavior.ChooseRight:
                                break;
                        }
                    }

                    result[key] = value;
                }
            }

            return result;
        }

        public static Dictionary<K, V> Merge<K, V>(this IDictionary<K, V> self, DictionaryMergeCollisionBehavior collisionBehavior, IDictionary<K, V> first, params IDictionary<K, V>[] rest)
        {
            return Merge(self, collisionBehavior, new[] { first }.Concat(rest));
        }

        public static IParseNode Unwrap(this IParseNode node)
        {
            if (node is BranchParseNode branch)
            {
                if (branch.Children.Count == 0)
                {
                    return null;
                }
                else if (branch.Children.Count == 1)
                {
                    if (branch.Children[0] is BranchParseNode child)
                    {
                        if (child.Rule is NamedRule)
                        {
                            return child;
                        }
                        else
                        {
                            return child.Unwrap();
                        }
                    }
                    else
                    {
                        return branch.Children[0];
                    }
                }
                else if (branch.Children.Count == 2 && branch.Children[0].Rule is NameRule named && named.Name == "im")
                {
                    return branch.Children[1].Unwrap();
                }
                else
                {
                    return branch;
                }
            }
            else
            {
                return node;
            }
        }

        public static IParseNode GetDescendant(this IParseNode node, int first, params int[] rest)
        {
            var address = new[] { first }.Concat(rest).ToArray();

            node = node.Unwrap();

            foreach (var index in address)
            {
                if(node is BranchParseNode branch && index < branch.Children.Count)
                {
                    node = branch.Children[index];

                    if(node.Rule is NameRule)
                    {
                        node = ((BranchParseNode)node).Children[0];
                    }
                }
                else
                {
                    return null;
                }
            }

            return node;
        }
    }
}
