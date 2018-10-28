using System.Linq;
using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Parsing.Parsers;
using PseudoEBNF.Semantics;

namespace PseudoEBNF.JavaScript
{
    public static class JavaScriptDefinition
    {
        public static Grammar GetGrammar()
        {
            var grammarString = $@"
varKeyword = /\b(var)\b/;               // -/
letKeyword = /\b(let)\b/;               // -/
newKeyword = /\b(new)\b/;               // -/
deleteKeyword = /\b(delete)\b/;         // X ?
instanceofKeyword = /\b(instanceof)\b/; // X ?
withKeyword = /\b(with)\b/;             // X
voidKeyword = /\b(void)\b/;             // X
typeofKeyword = /\b(typeof)\b/;         // X ?
thisKeyword = /\b(this)\b/;             // X ?
debuggerKeyword = /\b(debugger)\b/;     // X
importKeyword = /\b(import)\b/;         // X
classKeyword = /\b(class)\b/;           // X
constKeyword = /\b(const)\b/;           // X ?
extendsKeyword = /\b(extends)\b/;       // X
yieldKeyword = /\b(yield)\b/;           // X
superKeyword = /\b(super)\b/;           // X

functionKeyword = /\b(function)\b/;     // -/
tryKeyword = /\b(try)\b/;               // X ?
catchKeyword = /\b(catch)\b/;           // X ?
finallyKeyword = /\b(finally)\b/;       // X ?
throwKeyword = /\b(throw)\b/;           // X ?
returnKeyword = /\b(return)\b/;         // X ?

ifKeyword = /\b(if)\b/;                 // X ?
elseKeyword = /\b(else)\b/;             // X ?
whileKeyword = /\b(while)\b/;           // X ?
doKeyword = /\b(do)\b/;                 // X ?
forKeyword = /\b(for)\b/;               // X ?
inKeyword = /\b(in)\b/;                 // X ?
switchKeyword = /\b(switch)\b/;         // X ?
caseKeyword = /\b(case)\b/;             // X ?
defaultKeyword = /\b(default)\b/;       // X ?
breakKeyword = /\b(break)\b/;           // X ?
continueKeyword = /\b(continue)\b/;     // X ?

implementsKeyword = /\b(implements)\b/; // X
interfaceKeyword = /\b(interface)\b/;   // X
packageKeyword = /\b(package)\b/;       // X
privateKeyword = /\b(private)\b/;       // X
protectedKeyword = /\b(protected)\b/;   // X
publicKeyword = /\b(public)\b/;         // X
staticKeyword = /\b(static)\b/;         // X
awaitKeyword = /\b(await)\b/;           // X
enumKeyword = /\b(enum)\b/;             // X

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
ident = /\b[$A-Za-z_][$A-Za-z_0-9]*\b/;
number = /\b\d+(?:\.\d+)?\b/;
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

basicKeywords = varKeyword | letKeyword | newKeyword | deleteKeyword | instanceofKeyword | withKeyword | voidKeyword | typeofKeyword | thisKeyword | debuggerKeyword | importKeyword | classKeyword | constKeyword | extendsKeyword | yieldKeyword | superKeyword;
functionLevelKeywords = functionKeyword | tryKeyword | catchKeyword | finallyKeyword | throwKeyword | returnKeyword;
controlKeywords = ifKeyword | elseKeyword | whileKeyword | doKeyword | forKeyword | inKeyword | switchKeyword | caseKeyword | defaultKeyword | breakKeyword | continueKeyword;
futureKeywords = implementsKeyword | interfaceKeyword | packageKeyword | privateKeyword | protectedKeyword | publicKeyword | staticKeyword | awaitKeyword | enumKeyword;

validIdent = im !(basicKeywords | functionLevelKeywords | controlKeywords | futureKeywords) ident;

string = doubleString | singleString;

paren = leftParen expr3 rightParen;

unaryOper = minus | plus;
unaryMath = unaryOper *unaryOper expr2;

typeof = typeofKeyword expr2;

block = leftBracket *statement rightBracket;
paramList = leftParen ?(validIdent *(comma validIdent)) rightParen;
namedFunction = functionKeyword validIdent paramList block;
anonFunction = functionKeyword paramList block;

propertyDef = (validIdent | string) colon expr3;
object = leftBracket ?(propertyDef *(comma propertyDef)) rightBracket;

dotRef = dot validIdent;
key = leftSquare expr3 rightSquare;
argList = leftParen ?(expr3 *(comma expr3)) rightParen;
expressionFragment = dotRef | key | argList;

propertyFragments = (expressionFragment propertyFragments) | dotRef | key;
functionCallFragments = (expressionFragment functionCallFragments) | argList;

constructor = newKeyword functionCall;

// == END expr0 DEFINITIONS ==

functionCall = expr0 functionCallFragments;
propertyReference = expr0 propertyFragments;

// == END expr1 definitions ==

mathOper = minus | plus | times | divide;
math = expr1 mathOper expr2;

logicOper = and | or;
logicNegation = not expr2;
logic = expr1 logicOper expr2;

bitOper = bitAnd | bitOr | bitXor;
bitNegation = bitNot expr2;
bitwise = expr1 bitOper expr2;

instanceof = expr1 instanceofKeyword expr3;
in = expr1 inKeyword expr3;

// == END expr2 DEFINITIONS ==

assignOper = equals | minusEquals | plusEquals | timesEquals | divideEquals | modulusEquals | bitAndEquals | bitOrEquals | bitXorEquals;
localAssignment = validIdent assignOper expr3;
propertyAssignment = propertyReference assignOper expr3;
assignment = localAssignment | propertyAssignment;

ternary = expr2 question expr3 colon expr3;

// == END expr3 DEFINITIONS ==

variable = localAssignment | validIdent;
variableDecl = (varKeyword | letKeyword | constKeyword) variable *(comma variable);

break = breakKeyword;
continue = continueKeyword;
return = returnKeyword ?expr3;
throw = throwKeyword ?expr3;
delete = deleteKeyword propertyReference;
catch = catchKeyword block;
finally = finallyKeyword block;
try = tryKeyword block catch *catch ?finally;

default = defaultKeyword colon *statement;
case = caseKeyword (string | number | (validIdent *dotRef)) colon *statement;
switch = switchKeyword leftBracket *case ?default *case rightBracket;

else = elseKeyword statement;
if = ifKeyword statement ?else;
while = whileKeyword paren statement;
doWhile = doKeyword statement whileKeyword paren ?semicolon;
for = forKeyword leftParen ((variableDecl | expr3) semicolon expr3 semicolon expr3) rightParen statement;

expr0 = thisKeyword | anonFunction | validIdent | number | string | paren | unaryMath | logicNegation | bitNegation | constructor | object;
expr1 = functionCall | propertyReference | expr0;
expr2 = math | logic | instanceof | in | bitwise | expr1;
expr3 = ternary | assignment | expr2;
blockStatement = if | while | doWhile | forIn | for | switch | namedFunction | block;
statement = blockStatement | ((variableDecl | break | continue | return | throw | delete | expr3) ?semicolon) | semicolon;

root = statement *statement;
";

            var parserGen = new ParserGenerator();

            Grammar grammar = parserGen.BuildGrammar(grammarString, "ws", "lineComment", "blockComment");

            grammar.AttachAction("root", (branch, recurse) =>
            {
                ISemanticNode first = recurse(branch.GetDescendant(0));
                ISemanticNode[] rest = branch.GetDescendant(1)
                    .Elements
                    .Select(recurse)
                    .ToArray();

                return new BranchSemanticNode((int)JsNodeType.Root, first, rest);
            });

            grammar.AttachAction("statement", (branch, recurse) =>
            {
                ISemanticNode stmt = recurse(branch.GetDescendant(0));

                return new BranchSemanticNode((int)JsNodeType.Statement, stmt);
            });

            grammar.AttachAction("variableDecl", (branch, recurse) =>
            {
                ISemanticNode first = recurse(branch.GetDescendant(1, 0));
                ISemanticNode[] rest = branch.GetDescendant(2)
                    .Elements
                    .Select(n => recurse(n.GetDescendant(1, 0)))
                    .ToArray();

                return new BranchSemanticNode((int)JsNodeType.Variable, first, rest);
            });

            grammar.AttachAction("localAssignment", (branch, recurse) =>
            {
                ISemanticNode lvalue = recurse(branch.GetDescendant(0));
                ISemanticNode expr2 = recurse(branch.GetDescendant(2));

                return new BranchSemanticNode((int)JsNodeType.Assignment, lvalue, expr2);
            });

            grammar.AttachAction("propertyAssignment", RuleActions.PropertyAssignment);

            grammar.AttachAction("propertyReference", RuleActions.CompositeExpression);

            grammar.AttachAction("argList", (branch, recurse) =>
            {
                BranchParseNode args = branch.GetDescendant(1);

                if (args.Elements.Count == 0)
                { return new BranchSemanticNode((int)JsNodeType.ArgumentList, branch.StartIndex, new ISemanticNode[0]); }

                ISemanticNode first = recurse(args.GetDescendant(0));
                ISemanticNode[] rest = args
                    .GetDescendant(1)
                    .Elements
                    .Select(n => recurse(n.GetDescendant(1)))
                    .ToArray();

                return new BranchSemanticNode((int)JsNodeType.ArgumentList, branch.StartIndex, new[] { first }.Concat(rest));
            });

            grammar.AttachAction("dotRef", (branch, recurse) =>
            {
                ISemanticNode ident = recurse(branch.GetDescendant(1));

                return new BranchSemanticNode((int)JsNodeType.DotReference, ident);
            });

            grammar.AttachAction("key", (branch, recurse) =>
            {
                ISemanticNode key = recurse(branch.GetDescendant(1));

                return new BranchSemanticNode((int)JsNodeType.KeyReference, key);
            });

            grammar.AttachAction("functionCall", RuleActions.FunctionCall);

            grammar.AttachAction("constructor", RuleActions.Constructor);

            grammar.AttachAction("expressionFragment", RuleActions.Unwrap);

            grammar.AttachAction("object", (branch, recurse) =>
            {
                BranchParseNode firstNode = branch.GetDescendant(1, 0);

                if (firstNode == null)
                { return new BranchSemanticNode((int)JsNodeType.Object, branch.StartIndex, new ISemanticNode[0]); }

                ISemanticNode first = recurse(firstNode);
                ISemanticNode[] rest = branch.GetDescendant(1, 1)
                    .Elements
                    .Select(n => recurse(n.GetDescendant(1)))
                    .ToArray();

                return new BranchSemanticNode((int)JsNodeType.Object, branch.StartIndex, first, rest);
            });

            grammar.AttachAction("propertyDef", (branch, recurse) =>
            {
                ISemanticNode ident = recurse(branch.GetDescendant(0));
                ISemanticNode value = recurse(branch.GetDescendant(2));

                return new BranchSemanticNode((int)JsNodeType.PropertyDefinition, ident, value);
            });

            grammar.AttachAction("anonFunction", (branch, recurse) =>
            {
                ISemanticNode paramList = recurse(branch.GetDescendant(1));
                ISemanticNode body = recurse(branch.GetDescendant(2));

                return new BranchSemanticNode((int)JsNodeType.AnonymousFunction, paramList, body);
            });

            grammar.AttachAction("namedFunction", (branch, recurse) =>
            {
                ISemanticNode name = recurse(branch.GetDescendant(1));
                ISemanticNode paramList = recurse(branch.GetDescendant(2));
                ISemanticNode body = recurse(branch.GetDescendant(3));

                return new BranchSemanticNode((int)JsNodeType.NamedFunction, paramList, body);
            });

            grammar.AttachAction("paramList", (branch, recurse) =>
            {
                ISemanticNode first = recurse(branch.GetDescendant(1, 0));
                ISemanticNode[] rest = branch.GetDescendant(1, 1)
                    .Elements
                    .Select(n => recurse(n.GetDescendant(1)))
                    .ToArray();

                return new BranchSemanticNode((int)JsNodeType.ParameterList, first, rest);
            });

            grammar.AttachAction("block", (branch, recurse) =>
            {
                ISemanticNode[] stmts = branch.GetDescendant(1)
                    .Elements
                    .Select(recurse)
                    .ToArray();

                return new BranchSemanticNode((int)JsNodeType.Block, branch.StartIndex, stmts);
            });

            grammar.AttachAction("bitwise", (branch, recurse) =>
            {
                ISemanticNode left = recurse(branch.GetDescendant(0));
                ISemanticNode right = recurse(branch.GetDescendant(2));

                return new BranchSemanticNode((int)JsNodeType.Bitwise, left, right);
            });

            grammar.AttachAction("bitNegation", (branch, recurse) =>
            {
                ISemanticNode operand = recurse(branch.GetDescendant(1));

                return new BranchSemanticNode((int)JsNodeType.BitwiseNegation, operand);
            });

            grammar.AttachAction("logic", (branch, recurse) =>
            {
                ISemanticNode left = recurse(branch.GetDescendant(0));
                ISemanticNode right = recurse(branch.GetDescendant(2));

                return new BranchSemanticNode((int)JsNodeType.Logic, left, right);
            });

            grammar.AttachAction("logicNegation", (branch, recurse) =>
            {
                ISemanticNode operand = recurse(branch.GetDescendant(1));

                return new BranchSemanticNode((int)JsNodeType.LogicNegation, operand);
            });

            grammar.AttachAction("math", (branch, recurse) =>
            {
                ISemanticNode left = recurse(branch.GetDescendant(0));
                ISemanticNode right = recurse(branch.GetDescendant(2));

                return new BranchSemanticNode((int)JsNodeType.Math, left, right);
            });

            grammar.AttachAction("unaryMath", (branch, recurse) =>
            {
                ISemanticNode operand = recurse(branch.GetDescendant(1));

                return new BranchSemanticNode((int)JsNodeType.UnaryMath, operand);
            });

            grammar.AttachAction("paren", (branch, recurse) =>
            {
                ISemanticNode operand = recurse(branch.GetDescendant(1));

                return new BranchSemanticNode((int)JsNodeType.Parenthetical, operand);
            });

            grammar.AttachAction("validIdent", (branch, recurse) =>
            {
                branch = branch.GetDescendant(1);

                var ident = branch.Leaf.MatchedText;
                var startIndex = branch.Leaf.StartIndex;

                return new LeafSemanticNode((int)JsNodeType.Identifier, startIndex, ident);
            });

            grammar.AttachAction("doubleString", (branch, recurse) =>
            {
                var text = branch.Leaf.MatchedText;
                text = text
                    .Substring(1, text.Length - 2)
                    .Replace(@"\\", @"\")
                    .Replace(@"\""", @"""");
                var startIndex = branch.Leaf.StartIndex;

                return new LeafSemanticNode((int)JsNodeType.String, startIndex, text);
            });

            grammar.AttachAction("singleString", (branch, recurse) =>
            {
                var text = branch.Leaf.MatchedText;
                text = text
                    .Substring(1, text.Length - 2)
                    .Replace(@"\\", @"\")
                    .Replace(@"\'", @"'");
                var startIndex = branch.Leaf.StartIndex;

                return new LeafSemanticNode((int)JsNodeType.String, startIndex, text);
            });

            grammar.AttachAction("string", RuleActions.Unwrap);

            grammar.AttachAction("regex", (branch, recurse) =>
            {
                var pattern = branch.Leaf.MatchedText;
                pattern = pattern
                    .Substring(1, pattern.Length - 2)
                    .Replace(@"\\", @"\")
                    .Replace(@"\/", @"/");
                var startIndex = branch.Leaf.StartIndex;

                return new LeafSemanticNode((int)JsNodeType.RegularExpression, startIndex, pattern);
            });

            grammar.AttachAction("number", (branch, recurse) =>
            {
                var number = branch.Leaf.MatchedText;
                var startIndex = branch.Leaf.StartIndex;

                return new LeafSemanticNode((int)JsNodeType.Number, startIndex, number);
            });

            grammar.AttachAction("expr0", RuleActions.Unwrap);
            grammar.AttachAction("expr1", RuleActions.Unwrap);
            grammar.AttachAction("expr2", RuleActions.Unwrap);
            grammar.AttachAction("expr3", RuleActions.Unwrap);
            grammar.AttachAction("assignment", RuleActions.Unwrap);

            return grammar;
        }

        public static Parser GetParser()
        {
            var settings = new ParserSettings
            {
                Algorithm = Parser.Algorithm.LL,
                NestingType = Parser.NestingType.Stack,
                Unit = Parser.Unit.Character,
            };

            Parser parser = new ParserManager(GetGrammar(), settings);

            return parser;
        }

        public static Lexer GetLexer() => new Lexer(GetGrammar());
    }
}
