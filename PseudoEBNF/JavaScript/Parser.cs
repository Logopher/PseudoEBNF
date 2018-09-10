using PseudoEBNF.Common;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Parsing.Rules;
using PseudoEBNF.Semantics;
using System;
using System.Collections.Generic;
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
expressionFragment = dotRef | key | argList;

dotRefExpression = (expressionFragment dotRefExpression) | dotRef;
keyExpression = (expressionFragment keyExpression) | key;
argListExpression = (expressionFragment argListExpression) | argList;

compositeExpression = simpleExpr (dotRefExpression | keyExpression | argListExpression);

localAssignment = ident equals expr;
propertyAssignment = simpleExpr (dotRefExpression | keyExpression) equals expr;
assignment = localAssignment | propertyAssignment;

variable = localAssignment | ident;
variableDecl = (varKeyword | letKeyword) variable *(comma variable) ?semicolon;

simpleExpr = anonFunction | ident | number | string | paren | unaryMath | logicNegation | bitNegation | object;
expr = math | logic | bitwise | assignment | compositeExpression | simpleExpr;
statement = namedFunction | block | semicolon | variableDecl | (expr ?semicolon);

root = statement *statement;
";

            var parserGen = new ParserGenerator();
            parser = parserGen.SpawnParser(grammar, "ws", "lineComment", "blockComment");

            parser.SetImplicit("ws");
            parser.SetImplicit("blockComment");
            parser.SetImplicit("lineComment");

            //*
            parser.AttachAction("root", (node, recurse) =>
            {
                var first = recurse(node.GetDescendant(0));
                var rest = ((BranchParseNode)node.GetDescendant(1))
                    .Children.Select(recurse);

                return new BranchSemanticNode((int)JsNodeType.Root, first, rest);
            });

            parser.AttachAction("statement", (node, recurse) =>
            {
                var stmt = recurse(node.GetDescendant(0));

                return new BranchSemanticNode((int)JsNodeType.Statement, stmt);
            });

            parser.AttachAction("variableDecl", (node, recurse) =>
            {
                var first = recurse(node.GetDescendant(1, 0));
                var rest = ((BranchParseNode)node.GetDescendant(2))
                    .Children.Select(n => recurse(n.GetDescendant(1, 0)));

                return new BranchSemanticNode((int)JsNodeType.Variable, first, rest);
            });

            parser.AttachAction("localAssignment", (node, recurse) =>
            {
                var lvalue = recurse(node.GetDescendant(0));
                var expr = recurse(node.GetDescendant(2));

                return new BranchSemanticNode((int)JsNodeType.Assignment, lvalue, expr);
            });

            parser.AttachAction("propertyAssignment", (node, recurse) =>
            {
                var first = recurse(node.GetDescendant(0));

                var fragments = node.GetDescendant(1);

                var temp = ExpressionFragments(fragments, recurse);
                var nodeType = temp.Item1;
                var rest = temp.Item2;

                var lvalue = new BranchSemanticNode((int)nodeType, first, rest);
                var expr = recurse(node.GetDescendant(3));

                return new BranchSemanticNode((int)JsNodeType.Assignment, lvalue, expr);
            });

            parser.AttachAction("compositeExpression", (node, recurse) =>
            {
                var first = recurse(node.GetDescendant(0));

                var fragments = node.GetDescendant(1);

                var temp = ExpressionFragments(fragments, recurse);
                var nodeType = temp.Item1;
                var rest = temp.Item2;

                return new BranchSemanticNode((int)nodeType, first, rest);
            });

            parser.AttachAction("argList", (node, recurse) =>
            {
                var first = recurse(node.GetDescendant(1, 0));
                var rest = ((BranchParseNode)node.GetDescendant(1, 1))
                    .Children.Select(n => recurse(n.GetDescendant(1)));

                return new BranchSemanticNode((int)JsNodeType.ArgumentList, first, rest);
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

        Tuple<JsNodeType, List<ISemanticNode>> ExpressionFragments(IParseNode fragments, Func<IParseNode, ISemanticNode> recurse)
        {
            JsNodeType nodeType;
            var semanticNodes = new List<ISemanticNode>();
            var named = (NamedRule)fragments.Rule;
            switch (named.Name)
            {
                case "dotRef":
                case "key":
                    semanticNodes.Add(recurse(fragments.GetDescendant(1)));
                    nodeType = JsNodeType.Property;
                    break;
                case "argList":
                    var all = (BranchParseNode)fragments.GetDescendant(1);
                    if (all.Children.Count != 0)
                    {
                        var first = recurse(all.GetDescendant(0));
                        var rest = ((BranchParseNode)all.GetDescendant(1))
                            .Children.Select(n => recurse(n.GetDescendant(1)));

                        semanticNodes.Add(first);
                        semanticNodes.AddRange(rest);
                    }

                    nodeType = JsNodeType.FunctionCall;
                    break;
                default:
                    {
                        var fragmentBranch = (BranchParseNode)((BranchParseNode)fragments).Children[0];

                        if (fragmentBranch.Children.Count == 1)
                        {
                            return ExpressionFragments(fragmentBranch.Children[0], recurse);
                        }

                        var argsNode = fragmentBranch.GetDescendant(0, 0);

                        var first = fragmentBranch.GetDescendant(0);

                        var temp = ExpressionFragments(fragmentBranch.GetDescendant(1), recurse);
                        var rest = temp.Item2;

                        semanticNodes.Add(recurse(first.GetDescendant(0, 1)));
                        semanticNodes.AddRange(rest);

                        var rule = (NamedRule)fragments.Rule;
                        switch (rule.Name)
                        {
                            case "dotRefExpression":
                            case "keyExpression":
                                nodeType = JsNodeType.Property;
                                break;
                            case "argListExpression":
                                nodeType = JsNodeType.FunctionCall;
                                break;
                            default:
                                throw new Exception();
                        }

                        var result = new BranchSemanticNode((int)temp.Item1, semanticNodes);
                        semanticNodes.Clear();
                        semanticNodes.Add(result);
                        break;
                    }
            }

            return Tuple.Create(nodeType, semanticNodes);
        }

        public ISemanticNode Parse(string input)
        {
            return parser.Parse(input);
        }
    }
}
