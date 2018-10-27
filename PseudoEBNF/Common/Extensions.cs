using System;
using System.Collections.Generic;
using System.Linq;
using PseudoEBNF.Parsing.Rules;

namespace PseudoEBNF.Common
{
    public static class Extensions
    {
        public static AndRule And(this Rule rule, Rule first, params Rule[] rest) => new AndRule(rule, new[] { rule, first }.Concat(rest));

        public static OrRule Or(this Rule rule, Rule first, params Rule[] rest) => new OrRule(rule, new[] { rule, first }.Concat(rest));

        public static Dictionary<K, V> Merge<K, V>(this IDictionary<K, V> self, DictionaryMergeCollisionBehavior collisionBehavior, IEnumerable<IDictionary<K, V>> rest)
        {
            var result = self.ToDictionary(p => p.Key, p => p.Value);

            foreach (IDictionary<K, V> d in rest)
            {
                foreach (KeyValuePair<K, V> pair in d)
                {
                    K key = pair.Key;
                    V value = pair.Value;

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

        public static Dictionary<K, V> Merge<K, V>(this IDictionary<K, V> self, DictionaryMergeCollisionBehavior collisionBehavior, IDictionary<K, V> first, params IDictionary<K, V>[] rest) => Merge(self, collisionBehavior, new[] { first }.Concat(rest));
    }
}
