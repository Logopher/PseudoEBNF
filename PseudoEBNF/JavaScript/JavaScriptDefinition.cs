using System.Linq;
using PseudoEBNF;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Parsing.Parsers;
using PseudoEBNF.Semantics;

namespace PseudoEBNF.JavaScript
{
    public static class JavaScriptDefinition
    {
        public static Parser GetParser()
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
            var settings = new ParserSettings
            {
                Algorithm = Parser.Algorithm.LL,
                NestingType = Parser.NestingType.Stack,
                Unit = Parser.Unit.Character,
            };
            Parser parser = parserGen.SpawnParser(settings, grammar, "ws", "lineComment", "blockComment");

            //*
            parser.AttachAction("root", (branch, recurse) =>
            {
                ISemanticNode first = recurse(branch.GetDescendant(0));
                ISemanticNode[] rest = branch.GetDescendant(1)
                    .Elements
                    .Select(recurse)
                    .ToArray();

                return new BranchSemanticNode((int)JsNodeType.Root, first, rest);
            });

            parser.AttachAction("statement", (branch, recurse) =>
            {
                ISemanticNode stmt = recurse(branch.GetDescendant(0));

                return new BranchSemanticNode((int)JsNodeType.Statement, stmt);
            });

            parser.AttachAction("variableDecl", (branch, recurse) =>
            {
                ISemanticNode first = recurse(branch.GetDescendant(1, 0));
                ISemanticNode[] rest = branch.GetDescendant(2)
                    .Elements
                    .Select(n => recurse(n.GetDescendant(1, 0)))
                    .ToArray();

                return new BranchSemanticNode((int)JsNodeType.Variable, first, rest);
            });

            parser.AttachAction("localAssignment", (branch, recurse) =>
            {
                ISemanticNode lvalue = recurse(branch.GetDescendant(0));
                ISemanticNode expr = recurse(branch.GetDescendant(2));

                return new BranchSemanticNode((int)JsNodeType.Assignment, lvalue, expr);
            });

            parser.AttachAction("propertyAssignment", RuleActions.PropertyAssignment);

            parser.AttachAction("compositeExpression", RuleActions.CompositeExpression);

            parser.AttachAction("argList", (branch, recurse) =>
            {
                BranchParseNode args = branch.GetDescendant(1);

                if (args.Elements.Count == 0)
                { return new BranchSemanticNode((int)JsNodeType.ArgumentList, new ISemanticNode[0]); }

                ISemanticNode first = recurse(args.GetDescendant(0));
                ISemanticNode[] rest = args
                    .GetDescendant(1)
                    .Elements
                    .Select(n => recurse(n.GetDescendant(1)))
                    .ToArray();

                return new BranchSemanticNode((int)JsNodeType.ArgumentList, new[] { first }.Concat(rest));
            });

            parser.AttachAction("dotRef", (branch, recurse) =>
            {
                ISemanticNode ident = recurse(branch.GetDescendant(1));

                return new BranchSemanticNode((int)JsNodeType.DotReference, ident);
            });

            parser.AttachAction("key", (branch, recurse) =>
            {
                ISemanticNode key = recurse(branch.GetDescendant(1));

                return new BranchSemanticNode((int)JsNodeType.KeyReference, key);
            });

            parser.AttachAction("functionCall", RuleActions.FunctionCall);

            parser.AttachAction("constructor", RuleActions.Constructor);

            parser.AttachAction("expressionFragment", RuleActions.Unwrap);

            parser.AttachAction("object", (branch, recurse) =>
            {
                BranchParseNode firstNode = branch.GetDescendant(1, 0);

                if (firstNode == null)
                { return new BranchSemanticNode((int)JsNodeType.Object, new ISemanticNode[0]); }

                ISemanticNode first = recurse(firstNode);
                ISemanticNode[] rest = branch.GetDescendant(1, 1)
                    .Elements
                    .Select(n => recurse(n.GetDescendant(1)))
                    .ToArray();

                return new BranchSemanticNode((int)JsNodeType.Object, first, rest);
            });

            parser.AttachAction("propertyDef", (branch, recurse) =>
            {
                ISemanticNode ident = recurse(branch.GetDescendant(0));
                ISemanticNode value = recurse(branch.GetDescendant(2));

                return new BranchSemanticNode((int)JsNodeType.PropertyDefinition, ident, value);
            });

            parser.AttachAction("anonFunction", (branch, recurse) =>
            {
                ISemanticNode paramList = recurse(branch.GetDescendant(1));
                ISemanticNode body = recurse(branch.GetDescendant(2));

                return new BranchSemanticNode((int)JsNodeType.AnonymousFunction, paramList, body);
            });

            parser.AttachAction("namedFunction", (branch, recurse) =>
            {
                ISemanticNode name = recurse(branch.GetDescendant(1));
                ISemanticNode paramList = recurse(branch.GetDescendant(2));
                ISemanticNode body = recurse(branch.GetDescendant(3));

                return new BranchSemanticNode((int)JsNodeType.NamedFunction, paramList, body);
            });

            parser.AttachAction("paramList", (branch, recurse) =>
            {
                ISemanticNode first = recurse(branch.GetDescendant(1, 0));
                ISemanticNode[] rest = branch.GetDescendant(1, 1)
                    .Elements
                    .Select(n => recurse(n.GetDescendant(1)))
                    .ToArray();

                return new BranchSemanticNode((int)JsNodeType.ParameterList, first, rest);
            });

            parser.AttachAction("block", (branch, recurse) =>
            {
                ISemanticNode[] stmts = branch.GetDescendant(1)
                    .Elements
                    .Select(recurse)
                    .ToArray();

                return new BranchSemanticNode((int)JsNodeType.Block, stmts);
            });

            parser.AttachAction("bitwise", (branch, recurse) =>
            {
                ISemanticNode left = recurse(branch.GetDescendant(0));
                ISemanticNode right = recurse(branch.GetDescendant(2));

                return new BranchSemanticNode((int)JsNodeType.Bitwise, left, right);
            });

            parser.AttachAction("bitNegation", (branch, recurse) =>
            {
                ISemanticNode operand = recurse(branch.GetDescendant(1));

                return new BranchSemanticNode((int)JsNodeType.BitwiseNegation, operand);
            });

            parser.AttachAction("logic", (branch, recurse) =>
            {
                ISemanticNode left = recurse(branch.GetDescendant(0));
                ISemanticNode right = recurse(branch.GetDescendant(2));

                return new BranchSemanticNode((int)JsNodeType.Logic, left, right);
            });

            parser.AttachAction("logicNegation", (branch, recurse) =>
            {
                ISemanticNode operand = recurse(branch.GetDescendant(1));

                return new BranchSemanticNode((int)JsNodeType.LogicNegation, operand);
            });

            parser.AttachAction("math", (branch, recurse) =>
            {
                ISemanticNode left = recurse(branch.GetDescendant(0));
                ISemanticNode right = recurse(branch.GetDescendant(2));

                return new BranchSemanticNode((int)JsNodeType.Math, left, right);
            });

            parser.AttachAction("unaryMath", (branch, recurse) =>
            {
                ISemanticNode operand = recurse(branch.GetDescendant(1));

                return new BranchSemanticNode((int)JsNodeType.UnaryMath, operand);
            });

            parser.AttachAction("paren", (branch, recurse) =>
            {
                ISemanticNode operand = recurse(branch.GetDescendant(1));

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

            return parser;
        }
    }
}
