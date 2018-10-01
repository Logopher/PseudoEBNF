using System;
using System.Collections.Generic;
using System.Text;

namespace PseudoEBNF.Common
{
    internal static class Utilities
    {
        public static List<T> List<T>(T first, params T[] rest)
        { return List(first, (IEnumerable<T>)rest); }

        public static List<T> List<T>(T first, IEnumerable<T> rest)
        {
            var result = new List<T> { first };
            result.AddRange(rest);
            return result;
        }

        public static List<T> List<T>(T first, T second, IEnumerable<T> rest)
        {
            var result = new List<T> { first, second };
            result.AddRange(rest);
            return result;
        }
    }
}
