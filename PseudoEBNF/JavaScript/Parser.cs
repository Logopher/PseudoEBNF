using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Parsing.Rules;
using PseudoEBNF.Semantics;
using System.Collections.Generic;
using System.Linq;

namespace PseudoEBNF.JavaScript
{
    public class Parser : IParser
    {
        global::PseudoEBNF.Parser parser;

        public Parser()
        {
            var grammar = $@"
functionKeyword = /(?<!\w)function(?!\w)/;
varKeyword = /(?<!\w)var(?!\w)/;
letKeyword = /(?<!\w)let(?!\w)/;
newKeyword = /(?<!\w)new(?!\w)/;

leftBracket = ""{{"";
rightBracket = ""}}"";

leftParen = ""("";
rightParen = "")"";

leftSquare = ""["";
rightSquare = ""]"";

comma = "","";
dot = ""."";

equals = ""="";
colon = "":"";
semicolon = "";"";

and = ""&&"";
or = ""||"";
not = ""!"";

strictEquality = ""==="";
strictInequality = ""!=="";
equality = ""=="";
inequality = ""!="";
lessThanOrEqual = ""<="";
greaterThanOrEqual = "">="";
lessThan = ""<"";
greaterThan = "">"";

ws = /\s+/;
ident = /(?:\$|\w)(?:\$|\w|\d)*/;
number = /\d+(?:\.\d+)?/;
doubleString = /""(?:\\\\""|\\\\[^""]|[^""\\\\])*""/;
singleString = /'(?:\\\\'|\\\\[^']|[^'\\\\])*'/;
regex = /\/(?:\\\\\/|\\\\|[^\/])+\/[A-Za-z]*/;
lineComment = /\/\/[^{"\r\n"}]*/;
blockComment = /\/\*([^*]|\*[^\/])*\*\//;

minusEquals = ""-="";
plusEquals = ""+="";
timesEquals = ""*="";
divideEquals = ""/="";
modulusEquals = ""%="";

bitAndEquals = ""&="";
bitOrEquals = ""|="";
bitXorEquals = ""^="";

minus = ""-"";
plus = ""+"";
times = ""*"";
divide = ""/"";
modulus = ""%"";

bitAnd = ""&"";
bitOr = ""|"";
bitNot = ""~"";
bitXor = ""^"";

question = ""?"";

string = doubleString | singleString;

paren = leftParen superExpr rightParen;

unaryOper = minus | plus;
unaryMath = unaryOper *unaryOper expr;

mathOper = minus | plus | times | divide;
math = simpleExpr mathOper expr;

logicOper = and | or;
logicNegation = not expr;
logic = simpleExpr logicOper expr;

bitOper = bitAnd | bitOr | bitXor;
bitNegation = bitNot expr;
bitwise = simpleExpr bitOper expr;

block = leftBracket *statement rightBracket;
paramList = leftParen ?(ident *(comma ident)) rightParen;
namedFunction = functionKeyword ident paramList block;
anonFunction = functionKeyword paramList block;

propertyDef = (ident | string) colon superExpr;
object = leftBracket ?(propertyDef *(comma propertyDef)) rightBracket;

dotRef = dot ident;
key = leftSquare superExpr rightSquare;
argList = leftParen ?(superExpr *(comma superExpr)) rightParen;
expressionFragment = dotRef | key | argList;

dotRefExpression = (expressionFragment dotRefExpression) | dotRef;
keyExpression = (expressionFragment keyExpression) | key;
argListExpression = (expressionFragment argListExpression) | argList;

functionCall = simpleExpr argListExpression;
constructor = newKeyword functionCall;
compositeExpression = simpleExpr (dotRefExpression | keyExpression);

ternary = expr question superExpr colon superExpr; 

assignOper = equals | minusEquals | plusEquals | timesEquals | divideEquals | modulusEquals | bitAndEquals | bitOrEquals | bitXorEquals;
localAssignment = ident assignOper superExpr;
propertyAssignment = simpleExpr (dotRefExpression | keyExpression) assignOper superExpr;
assignment = localAssignment | propertyAssignment;

variable = localAssignment | ident;
variableDecl = (varKeyword | letKeyword) variable *(comma variable) ?semicolon;

simpleExpr = anonFunction | ident | number | string | paren | unaryMath | logicNegation | bitNegation | object;
expr = math | logic | bitwise | constructor | functionCall | compositeExpression | simpleExpr;
superExpr = ternary | assignment | expr;
statement = namedFunction | block | semicolon | variableDecl | (superExpr ?semicolon);

root = statement *statement;
";

            var parserGen = new ParserGenerator();
            parser = parserGen.SpawnParser(grammar, "ws", "lineComment", "blockComment");

            parser.SetImplicit("ws");
            parser.SetImplicit("blockComment");
            parser.SetImplicit("lineComment");

            //*
            parser.AttachAction("root", (branch, recurse) =>
            {
                var first = recurse(branch.GetDescendant(0));
                var rest = branch.GetDescendant(1)
                    .Elements
                    .Select(recurse)
                    .ToArray();

                return new BranchSemanticNode((int)JsNodeType.Root, first, rest);
            });

            parser.AttachAction("statement", (branch, recurse) =>
            {
                var stmt = recurse(branch.GetDescendant(0));

                return new BranchSemanticNode((int)JsNodeType.Statement, stmt);
            });

            parser.AttachAction("variableDecl", (branch, recurse) =>
            {
                var first = recurse(branch.GetDescendant(1, 0));
                var rest = branch.GetDescendant(2)
                    .Elements
                    .Select(n => recurse(n.GetDescendant(1, 0)))
                    .ToArray();

                return new BranchSemanticNode((int)JsNodeType.Variable, first, rest);
            });

            parser.AttachAction("localAssignment", (branch, recurse) =>
            {
                var lvalue = recurse(branch.GetDescendant(0));
                var expr = recurse(branch.GetDescendant(2));

                return new BranchSemanticNode((int)JsNodeType.Assignment, lvalue, expr);
            });

            parser.AttachAction("propertyAssignment", RuleActions.PropertyAssignment);

            parser.AttachAction("compositeExpression", RuleActions.CompositeExpression);

            parser.AttachAction("argList", (branch, recurse) =>
            {
                var args = branch.GetDescendant(1);

                if (args.Elements.Count == 0)
                { return new BranchSemanticNode((int)JsNodeType.ArgumentList, new ISemanticNode[0]); }

                var first = recurse(args.GetDescendant(0));
                var rest = args
                    .GetDescendant(1)
                    .Elements
                    .Select(n => recurse(n.GetDescendant(1)))
                    .ToArray();

                return new BranchSemanticNode((int)JsNodeType.ArgumentList, new[] { first }.Concat(rest));
            });

            parser.AttachAction("dotRef", (branch, recurse) =>
            {
                var ident = recurse(branch.GetDescendant(1));

                return new BranchSemanticNode((int)JsNodeType.DotReference, ident);
            });

            parser.AttachAction("key", (branch, recurse) =>
            {
                var key = recurse(branch.GetDescendant(1));

                return new BranchSemanticNode((int)JsNodeType.KeyReference, key);
            });

            parser.AttachAction("functionCall", RuleActions.FunctionCall);

            parser.AttachAction("constructor", RuleActions.Constructor);

            parser.AttachAction("expressionFragment", RuleActions.Unwrap);

            parser.AttachAction("object", (branch, recurse) =>
            {
                var firstNode = branch.GetDescendant(1, 0);

                if (firstNode == null)
                { return new BranchSemanticNode((int)JsNodeType.Object, new ISemanticNode[0]); }

                var first = recurse(firstNode);
                var rest = branch.GetDescendant(1, 1)
                    .Elements
                    .Select(n => recurse(n.GetDescendant(1)))
                    .ToArray();

                return new BranchSemanticNode((int)JsNodeType.Object, first, rest);
            });

            parser.AttachAction("propertyDef", (branch, recurse) =>
            {
                var ident = recurse(branch.GetDescendant(0));
                var value = recurse(branch.GetDescendant(2));

                return new BranchSemanticNode((int)JsNodeType.PropertyDefinition, ident, value);
            });

            parser.AttachAction("anonFunction", (branch, recurse) =>
            {
                var paramList = recurse(branch.GetDescendant(1));
                var body = recurse(branch.GetDescendant(2));

                return new BranchSemanticNode((int)JsNodeType.AnonymousFunction, paramList, body);
            });

            parser.AttachAction("namedFunction", (branch, recurse) =>
            {
                var name = recurse(branch.GetDescendant(1));
                var paramList = recurse(branch.GetDescendant(2));
                var body = recurse(branch.GetDescendant(3));

                return new BranchSemanticNode((int)JsNodeType.NamedFunction, paramList, body);
            });

            parser.AttachAction("paramList", (branch, recurse) =>
            {
                var first = recurse(branch.GetDescendant(1, 0));
                var rest = branch.GetDescendant(1, 1)
                    .Elements
                    .Select(n => recurse(n.GetDescendant(1)))
                    .ToArray();

                return new BranchSemanticNode((int)JsNodeType.ParameterList, first, rest);
            });

            parser.AttachAction("block", (branch, recurse) =>
            {
                var stmts = branch.GetDescendant(1)
                    .Elements
                    .Select(recurse)
                    .ToArray();

                return new BranchSemanticNode((int)JsNodeType.Block, stmts);
            });

            parser.AttachAction("bitwise", (branch, recurse) =>
            {
                var left = recurse(branch.GetDescendant(0));
                var right = recurse(branch.GetDescendant(2));

                return new BranchSemanticNode((int)JsNodeType.Bitwise, left, right);
            });

            parser.AttachAction("bitNegation", (branch, recurse) =>
            {
                var operand = recurse(branch.GetDescendant(1));

                return new BranchSemanticNode((int)JsNodeType.BitwiseNegation, operand);
            });

            parser.AttachAction("logic", (branch, recurse) =>
            {
                var left = recurse(branch.GetDescendant(0));
                var right = recurse(branch.GetDescendant(2));

                return new BranchSemanticNode((int)JsNodeType.Logic, left, right);
            });

            parser.AttachAction("logicNegation", (branch, recurse) =>
            {
                var operand = recurse(branch.GetDescendant(1));

                return new BranchSemanticNode((int)JsNodeType.LogicNegation, operand);
            });

            parser.AttachAction("math", (branch, recurse) =>
            {
                var left = recurse(branch.GetDescendant(0));
                var right = recurse(branch.GetDescendant(2));

                return new BranchSemanticNode((int)JsNodeType.Math, left, right);
            });

            parser.AttachAction("unaryMath", (branch, recurse) =>
            {
                var operand = recurse(branch.GetDescendant(1));

                return new BranchSemanticNode((int)JsNodeType.UnaryMath, operand);
            });

            parser.AttachAction("paren", (branch, recurse) =>
            {
                var operand = recurse(branch.GetDescendant(1));

                return new BranchSemanticNode((int)JsNodeType.Parenthetical, operand);
            });

            parser.AttachAction("ident", (branch, recurse) =>
            {
                var ident = branch.Leaf.MatchedText;

                return new LeafSemanticNode((int)JsNodeType.Identifier, ident);
            });

            parser.AttachAction("doubleString", (branch, recurse) =>
            {
                var text = branch.Leaf.MatchedText;
                text = text
                    .Substring(1, text.Length - 2)
                    .Replace(@"\\", @"\")
                    .Replace(@"\""", @"""");

                return new LeafSemanticNode((int)JsNodeType.String, text);
            });

            parser.AttachAction("singleString", (branch, recurse) =>
            {
                var text = branch.Leaf.MatchedText;
                text = text
                    .Substring(1, text.Length - 2)
                    .Replace(@"\\", @"\")
                    .Replace(@"\'", @"'");

                return new LeafSemanticNode((int)JsNodeType.String, text);
            });

            parser.AttachAction("string", RuleActions.Unwrap);

            parser.AttachAction("regex", (branch, recurse) =>
            {
                var pattern = branch.Leaf.MatchedText;
                pattern = pattern
                    .Substring(1, pattern.Length - 2)
                    .Replace(@"\\", @"\")
                    .Replace(@"\/", @"/");

                return new LeafSemanticNode((int)JsNodeType.RegularExpression, pattern);
            });

            parser.AttachAction("number", (branch, recurse) =>
            {
                var number = branch.Leaf.MatchedText;

                return new LeafSemanticNode((int)JsNodeType.Number, number);
            });

            parser.AttachAction("superExpr", RuleActions.Unwrap);
            parser.AttachAction("expr", RuleActions.Unwrap);
            parser.AttachAction("simpleExpr", RuleActions.Unwrap);
            parser.AttachAction("assignment", RuleActions.Unwrap);
            //*/
        }

        public NamedRule GetRule(string name)
        {
            return parser.GetRule(name);
        }

        public IToken GetToken(string name)
        {
            return parser.GetToken(name);
        }

        public IEnumerable<Lexeme> Lex(string input)
        {
            return parser.Lex(input);
        }

        public ISemanticNode Parse(string input)
        {
            return parser.Parse(input);
        }

        public ISemanticNode ParseSemantics(BranchParseNode node)
        {
            return parser.ParseSemantics(node);
        }

        public IParseNode ParseSyntax(string input)
        {
            return parser.ParseSyntax(input);
        }

        public BranchParseNode ParseSyntax(IEnumerable<Lexeme> lexemes)
        {
            return parser.ParseSyntax(lexemes);
        }
    }
}
