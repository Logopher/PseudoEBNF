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
            return new AndRule(Utilities.List(rule, first, rest));
        }

        public static OrRule Or(this IRule rule, IRule first, params IRule[] rest)
        {
            return new OrRule(Utilities.List(rule, first, rest));
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
            return Merge(self, collisionBehavior, Utilities.List(first, rest));
        }
    }
}
