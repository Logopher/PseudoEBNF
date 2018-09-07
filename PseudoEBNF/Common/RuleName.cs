using System;
using System.Collections.Generic;
using System.Text;

namespace PseudoEBNF.Common
{
    public static class RuleName
    {
        public static readonly string Whitespace = "ws";
        public static readonly string Identifier = "ident";
        public static readonly string String = "string";
        public static readonly string Regex = "regex";
        public static readonly string Literal = "literal";
        public new static readonly string Equals = "equals";
        public static readonly string Semicolon = "semicolon";
        public static readonly string Pipe = "pipe";
        public static readonly string Asterisk = "asterisk";
        public static readonly string QuestionMark = "question";
        public static readonly string ExclamationPoint = "exclamation";
        public static readonly string LeftParenthesis = "leftParen";
        public static readonly string RightParenthesis = "rightParen";

        public static readonly string And = "and";
        public static readonly string Or = "or";
        public static readonly string Not = "not";
        public static readonly string Optional = "opt";
        public static readonly string Repeat = "re";
        public static readonly string Group = "group";
        public static readonly string SimpleExpression = "simpleExpr";
        public static readonly string Expression = "expr";

        public static readonly string LineComment = "lineComment";
        public static readonly string Token = "token";
        public static readonly string Rule = "rule";
        public static readonly string Assignment = "assign";
        public static readonly string Root = "root";
        
        public static readonly string Implicit = "im";
    }
}
