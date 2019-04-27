using System;
using System.Collections.Generic;
using System.Linq;
using PseudoEBNF;
using PseudoEBNF.Common;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Semantics;

namespace ConsoleApp
{
    internal class CommandParser : Parser
    {
        private readonly Parser parser;
        private readonly Command command;

        public CommandParser(Parameters parameters)
        {
            command = new Command("", parameters);

            var grammar = $@"
equals = ""="";

shortOptions = /(?<= )-[_\w]+/;
shortOption = /(?<= )-[_\w]/;
longOption = /(?<= )--[_\w][-_\w]*/;
endOptions = /(?<= )--(?= )/;
doubleString = /""(?:\\\\""|\\\\[^""]|[^""\\\\])*""/;
singleString = /'(?:\\\\'|\\\\[^']|[^'\\\\])*'/;
identifier = /[_\w][-_\w]*/;
literal = /.+/;
ws = /\s+/;
path = /(([A-Za-z]:)?[\/\\\\])?[-_\\w.]+([\/\\\\][-_\\w.]+)*[\/\\\\]?/;

string = doubleString | singleString;
shortOptionWithValue = shortOption equals (identifier | string);
longOptionWithValue = longOption equals (identifier | string);

options = *(shortOptionWithValue | longOptionWithValue | shortOptions | longOption | identifier | string);

details = options ?(endOptions literal);

root = (string | path) ?details;
";

            var parserGen = new ParserGenerator();
            var settings = new ParserSettings
            {
                Algorithm = Algorithm.LL,
                NestingType = NestingType.Stack,
                Unit = Unit.Character,
            };

            parser = parserGen.SpawnParser(settings, grammar, "ws");

            parser.AttachAction("shortOptions", (branch, recurse) =>
            {
                var startIndex = branch.Leaf.StartIndex;
                IEnumerable<BranchSemanticNode> nodes = branch.Leaf.MatchedText
                    .Skip(1)
                    .Select((c, i) =>
                    {
                        return new BranchSemanticNode((int)CommandNodeType.Argument,
                            new LeafSemanticNode((int)CommandNodeType.ShortOption, startIndex + 1 + i, c.ToString()));
                    });

                return new BranchSemanticNode((int)CommandNodeType.Arguments, startIndex, nodes);
            });

            parser.AttachAction("shortOption", (branch, recurse) =>
            {
                LeafParseNode nameNode = branch.Leaf;
                var startIndex = nameNode.StartIndex;
                var name = nameNode.MatchedText[1].ToString();

                return new BranchSemanticNode((int)CommandNodeType.Argument,
                    new LeafSemanticNode((int)CommandNodeType.ShortOption, startIndex, name));
            });

            parser.AttachAction("shortOptionWithValue", (branch, recurse) =>
            {
                LeafParseNode nameNode = branch.GetDescendant(0).Leaf;
                var startIndex = nameNode.StartIndex;
                var name = nameNode.MatchedText[1].ToString();
                ISemanticNode value = recurse(branch.GetDescendant(2));

                return new BranchSemanticNode((int)CommandNodeType.Argument,
                    new LeafSemanticNode((int)CommandNodeType.ShortOption, startIndex, name),
                    value);
            });

            parser.AttachAction("longOption", (branch, recurse) =>
            {
                LeafParseNode nameNode = branch.Leaf;
                var startIndex = nameNode.StartIndex;
                var name = nameNode.MatchedText.Substring(2);

                return new BranchSemanticNode((int)CommandNodeType.Argument,
                    new LeafSemanticNode((int)CommandNodeType.LongOption, startIndex, name));
            });

            parser.AttachAction("longOptionWithValue", (branch, recurse) =>
            {
                LeafParseNode nameNode = branch.GetDescendant(0).Leaf;
                var startIndex = nameNode.StartIndex;
                var name = nameNode.MatchedText.Substring(2);
                ISemanticNode value = recurse(branch.GetDescendant(2));

                return new BranchSemanticNode((int)CommandNodeType.Argument,
                    new LeafSemanticNode((int)CommandNodeType.LongOption, startIndex, name),
                    value);
            });

            parser.AttachAction("identifier", (branch, recurse) =>
            {
                LeafParseNode nameNode = branch.Leaf;
                var startIndex = nameNode.StartIndex;
                var name = nameNode.MatchedText;

                return new LeafSemanticNode((int)CommandNodeType.String, startIndex, name);
            });

            parser.AttachAction("doubleString", (branch, recurse) =>
            {
                var text = branch.Leaf.MatchedText;
                text = text
                    .Substring(1, text.Length - 2)
                    .Replace(@"\\", @"\")
                    .Replace(@"\""", @"""");
                var startIndex = branch.Leaf.StartIndex;

                return new LeafSemanticNode((int)CommandNodeType.String, startIndex, text);
            });

            parser.AttachAction("singleString", (branch, recurse) =>
            {
                var text = branch.Leaf.MatchedText;
                text = text
                    .Substring(1, text.Length - 2)
                    .Replace(@"\\", @"\")
                    .Replace(@"\'", @"'");
                var startIndex = branch.Leaf.StartIndex;

                return new LeafSemanticNode((int)CommandNodeType.String, startIndex, text);
            });

            parser.AttachAction("string", (branch, recurse) => recurse(branch.GetDescendant(0)));

            parser.AttachAction("options", (branch, recurse) =>
            {
                IEnumerable<ISemanticNode> options = branch.GetDescendant(0)
                    ?.Elements
                    ?.Select(recurse);

                return new BranchSemanticNode((int)CommandNodeType.Options, branch.StartIndex, options ?? new ISemanticNode[0]);
            });

            parser.AttachAction("literal", (branch, recurse) =>
            {
                var value = branch.Leaf.MatchedText;

                return new LeafSemanticNode((int)CommandNodeType.String, branch.StartIndex, value);
            });

            parser.AttachAction("details", (branch, recurse) =>
            {
                BranchParseNode optionsNode = branch.GetDescendant(0);
                BranchParseNode literalNode = branch.GetDescendant(1, 1);

                var results = new List<ISemanticNode>();

                if (optionsNode != null)
                { results.Add(recurse(optionsNode)); }
                if (literalNode != null)
                { results.Add(recurse(literalNode)); }

                return new BranchSemanticNode((int)CommandNodeType.Details, branch.StartIndex, results);
            });

            parser.AttachAction("path", (branch, recurse) =>
            {
                var value = branch.MatchedText;

                return new LeafSemanticNode((int)CommandNodeType.String, branch.StartIndex, value);
            });

            parser.AttachAction("root", (branch, recurse) =>
            {
                ISemanticNode path = recurse(branch.GetDescendant(0));
                ISemanticNode details = recurse(branch.GetDescendant(1));

                return new BranchSemanticNode((int)CommandNodeType.Root, path, details);
            });
        }

        public override void Lock()
        {
            base.Lock();
            parser.Lock();
        }

        public new CommandInvokation Parse(string input)
        {
            var rootNode = (BranchSemanticNode)base.Parse(Environment.CommandLine);

            var pathNode = (LeafSemanticNode)rootNode.Children[0];

            var detailsNode = (BranchSemanticNode)rootNode.Children[1];

            var optionsNode = (BranchSemanticNode)detailsNode.Children[0];

            var literalNode = (LeafSemanticNode)(1 < detailsNode.Children.Count ? detailsNode.Children[1] : null);

            var positionalArguments = new List<string>();

            var parameters = command.Parameters;

            var arguments = new Dictionary<string, string>();

            void interpret(ISemanticNode node)
            {
                if (node is LeafSemanticNode leaf)
                {
                    if ((CommandNodeType)leaf.NodeType == CommandNodeType.String)
                    { positionalArguments.Add(leaf.Value); }
                    else
                    { throw new Exception(); }
                }
                else if (node is BranchSemanticNode branch)
                {
                    switch ((CommandNodeType)branch.NodeType)
                    {
                        case CommandNodeType.Arguments:
                            foreach (ISemanticNode child in branch.Children)
                            { interpret(child); }
                            break;
                        case CommandNodeType.Argument:
                            var optionNode = (LeafSemanticNode)branch.Children[0];
                            var option = optionNode.Value;

                            Parameter parameter;
                            switch ((CommandNodeType)optionNode.NodeType)
                            {
                                case CommandNodeType.ShortOption:
                                    parameter = parameters.GetParameter(option[1]);
                                    break;
                                case CommandNodeType.LongOption:
                                    parameter = parameters.GetParameter(option.Substring(2));
                                    break;
                                default:
                                    throw new Exception();
                            }

                            if (parameter == null)
                            { throw new Exception(); }

                            var valueNode = (LeafSemanticNode)(1 < branch.Children.Count ? branch.Children[1] : null);
                            var value = valueNode?.Value ?? "true";

                            arguments.Add(parameter.Name, value);

                            break;
                    }
                }
            }

            foreach (ISemanticNode node in optionsNode.Children)
            { interpret(node); }

            if (literalNode != null)
            { positionalArguments.Add(literalNode.Value); }

            var remainder = parameters.Where(p => !arguments.ContainsKey(p.Name)).ToList();

            foreach (Tuple<Parameter, string> t in remainder.Zip(positionalArguments, Tuple.Create).ToArray())
            {
                arguments.Add(t.Item1.Name, t.Item2);
                remainder.Remove(t.Item1);
                positionalArguments.Remove(t.Item2);
            }

            if (0 < positionalArguments.Count)
            { throw new Exception(); }

            if (remainder.Any(p => p.IsRequired))
            { throw new Exception(); }

            var path = pathNode.Value;

            return new CommandInvokation(path, arguments);
        }

        public override BranchParseNode ParseSyntax(string input) => parser.ParseSyntax(input);
    }
}
