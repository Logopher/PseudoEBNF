using PseudoEBNF.Common;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Parsing.Rules;
using PseudoEBNF.Semantics;
using System.Linq;

namespace PseudoEBNF.JavaScript
{
    public class Parser
    {
        Parsing.Parser parser;

        public Parser()
        {
            var grammar = $@"
ws = /\s+/;
ident = /\w(?:\w|\d)*/;
number = /\d+(?:\.\d+)?/;
doubleString = /""(?:\\\\""|\\\\|[^""])+""/;
singleString = /'(?:\\\\'|\\\\|[^'])+'/;
regex = /\/(?:\\\\\/|\\\\|[^\/])+\//;
lineComment = /\/\/[^{"\r\n"}]*/;
blockComment = /\/\*([^*]|\*[^\/])*\*\//;

leftBracket = ""{{"";
rightBracket = ""}}"";

leftParen = ""("";
rightParen = "")"";

leftSquare = ""["";
rightSquare = ""]"";

functionKeyword = ""function"";
varKeyword = ""var"";
letKeyword = ""let"";

comma = "","";
dot = ""."";

equals = ""="";
semicolon = "";"";

and = ""&&"";
or = ""||"";
not = ""!"";

minus = ""-"";
plus = ""+"";
times = ""*"";
divide = ""/"";

bitAnd = ""&"";
bitOr = ""|"";
bitNot = ""~"";
bitXor = ""^"";

string = doubleString | singleString;

paren = leftParen expr rightParen;

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

propertyDef = (ident | string) colon expr;
object = leftBracket ?(propertyDef *(comma propertyDef)) rightBracket;

dotRef = dot ident;
key = leftSquare expr rightSquare;
argList = leftParen ?(expr *(comma expr)) rightParen;
compositeExpression = simpleExpr *(dotRef | key | argList);

variableDecl = (varKeyword | letKeyword) identAssignment *(comma identAssignment) ?semicolon;

simpleExpr = anonFunction | ident | number | string | paren | unaryMath | logicNegation | bitNegation | object;
expr = math | logic | bitwise | compositeExpression;
assignment = compositeExpression equals expr *(equals expr);
statement = namedFunction | block | semicolon | variableDecl | (assignment ?semicolon) | (expr ?semicolon);

root = statement *statement;
";

            var parserGen = new ParserGenerator();
            parser = parserGen.SpawnParser(grammar, "ws", "lineComment", "blockComment");

            parser.SetImplicit("ws");
            parser.SetImplicit("blockComment");
            parser.SetImplicit("lineComment");

            /*
            parser.AttachAction("root", (node, recurse) =>
            {
                var first = recurse(node.GetDescendant(0));
                var rest = ((BranchParseNode)node.GetDescendant(1))
                    .Children.Select(recurse);

                return new BranchSemanticNode((int)JsNodeType.Root, first, rest);
            });

            parser.AttachAction("statement", (node, recurse) =>
            {
                if (!(node.Rule is NamedRule)) // expr
                {
                    node = node.GetDescendant(0);
                }

                var stmt = recurse(node);

                return new BranchSemanticNode((int)JsNodeType.Statement, stmt);
            });

            parser.AttachAction("variableDecl", (node, recurse) =>
            {
                var first = recurse(node.GetDescendant(1));
                var rest = ((BranchParseNode)node.GetDescendant(2))
                    .Children.Select(n => recurse(n.GetDescendant(1)));

                return new BranchSemanticNode((int)JsNodeType.Variable, first, rest);
            });

            parser.AttachAction("assignment", (node, recurse) =>
            {
                var lvalue = recurse(node.GetDescendant(0));
                var expr = recurse(node.GetDescendant(1, 1));

                return new BranchSemanticNode((int)JsNodeType.Assignment, lvalue, expr);
            });

            parser.AttachAction("functionCall", (node, recurse) =>
            {
                var name = recurse(node.GetDescendant(0));
                var args = recurse(node.GetDescendant(2));

                return new BranchSemanticNode((int)JsNodeType.FunctionCall, name, args);
            });

            parser.AttachAction("argList", (node, recurse) =>
            {
                var first = recurse(node.GetDescendant(0));
                var rest = ((BranchParseNode)node.GetDescendant(1))
                    .Children.Select(n => recurse(n.GetDescendant(1)));

                return new BranchSemanticNode((int)JsNodeType.ArgumentList, first, rest);
            });

            parser.AttachAction("property", (node, recurse) =>
            {
                var obj = recurse(node.GetDescendant(0));
                var ident = recurse(node.GetDescendant(1, 1));

                return new BranchSemanticNode((int)JsNodeType.Property, obj, ident);
            });

            parser.AttachAction("object", (node, recurse) =>
            {
                var first = recurse(node.GetDescendant(1, 0));
                var rest = ((BranchParseNode)node.GetDescendant(1, 1))
                    .Children.Select(n => recurse(n.GetDescendant(1)));

                return new BranchSemanticNode((int)JsNodeType.Object, first, rest);
            });

            parser.AttachAction("propertyDef", (node, recurse) =>
            {
                var ident = recurse(node.GetDescendant(0));
                var value = recurse(node.GetDescendant(2));

                return new BranchSemanticNode((int)JsNodeType.PropertyDefinition, ident, value);
            });

            parser.AttachAction("anonFunction", (node, recurse) =>
            {
                var paramList = recurse(node.GetDescendant(1));
                var body = recurse(node.GetDescendant(2));

                return new BranchSemanticNode((int)JsNodeType.AnonymousFunction, paramList, body);
            });

            parser.AttachAction("namedFunction", (node, recurse) =>
            {
                var name = recurse(node.GetDescendant(1));
                var paramList = recurse(node.GetDescendant(2));
                var body = recurse(node.GetDescendant(3));

                return new BranchSemanticNode((int)JsNodeType.NamedFunction, paramList, body);
            });

            parser.AttachAction("paramList", (node, recurse) =>
            {
                var first = recurse(node.GetDescendant(1, 0));
                var rest = ((BranchParseNode)node.GetDescendant(1, 1))
                    .Children.Select(n => recurse(n.GetDescendant(1)));

                return new BranchSemanticNode((int)JsNodeType.ParameterList, first, rest);
            });

            parser.AttachAction("block", (node, recurse) =>
            {
                var stmts = ((BranchParseNode)node.GetDescendant(1))
                    .Children.Select(recurse);

                return new BranchSemanticNode((int)JsNodeType.Block, stmts);
            });

            parser.AttachAction("bitwise", (node, recurse) =>
            {
                var left = recurse(node.GetDescendant(0));
                var right = recurse(node.GetDescendant(2));

                return new BranchSemanticNode((int)JsNodeType.Bitwise, left, right);
            });

            parser.AttachAction("bitNegation", (node, recurse) =>
            {
                var operand = recurse(node.GetDescendant(1));

                return new BranchSemanticNode((int)JsNodeType.BitwiseNegation, operand);
            });

            parser.AttachAction("logic", (node, recurse) =>
            {
                var left = recurse(node.GetDescendant(0));
                var right = recurse(node.GetDescendant(2));

                return new BranchSemanticNode((int)JsNodeType.Logic, left, right);
            });

            parser.AttachAction("logicNegation", (node, recurse) =>
            {
                var operand = recurse(node.GetDescendant(1));

                return new BranchSemanticNode((int)JsNodeType.LogicNegation, operand);
            });

            parser.AttachAction("math", (node, recurse) =>
            {
                var left = recurse(node.GetDescendant(0));
                var right = recurse(node.GetDescendant(2));

                return new BranchSemanticNode((int)JsNodeType.Math, left, right);
            });

            parser.AttachAction("unaryMath", (node, recurse) =>
            {
                var operand = recurse(node.GetDescendant(1));

                return new BranchSemanticNode((int)JsNodeType.UnaryMath, operand);
            });

            parser.AttachAction("paren", (node, recurse) =>
            {
                var operand = recurse(node.GetDescendant(1));

                return new BranchSemanticNode((int)JsNodeType.Parenthetical, operand);
            });

            parser.AttachAction("ident", (node, recurse) =>
            {
                var ident = node.Unwrap().MatchedText;

                return new LeafSemanticNode((int)JsNodeType.Identifier, ident);
            });

            parser.AttachAction("string", (node, recurse) =>
            {
                var text = node.Unwrap().Unwrap().MatchedText;
                text = text
                    .Substring(1, text.Length - 2)
                    .Replace(@"\\", @"\")
                    .Replace(@"\""", @"""");

                return new LeafSemanticNode((int)JsNodeType.String, text);
            });

            parser.AttachAction("regex", (node, recurse) =>
            {
                var pattern = node.Unwrap().MatchedText;
                pattern = pattern
                    .Substring(1, pattern.Length - 2)
                    .Replace(@"\\", @"\")
                    .Replace(@"\/", @"/");

                return new LeafSemanticNode((int)JsNodeType.RegularExpression, pattern);
            });

            parser.AttachAction("number", (node, recurse) =>
            {
                var number = node.Unwrap().MatchedText;

                return new LeafSemanticNode((int)JsNodeType.Number, number);
            });
            //*/
        }

        public ISemanticNode Parse(string input)
        {
            return parser.Parse(input);
        }
    }
}
