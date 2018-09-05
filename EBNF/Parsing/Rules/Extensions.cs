﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EBNF.Parsing.Rules
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
    }
}
