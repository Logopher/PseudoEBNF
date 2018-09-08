using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Semantics;

namespace PseudoEBNF.Parsing.Rules
{
    public class NamedRule : IRule
    {
        public string Name { get; }

        public IRule Rule { get; }

        public Func<IParseNode, Func<IParseNode, ISemanticNode>, ISemanticNode> Action { get; private set; } = DefaultAction;

        public NamedRule(string name, IRule rule)
        {
            Name = name;
            Rule = rule;
        }

        NamedRule(string name, IRule rule, Func<IParseNode, Func<IParseNode, ISemanticNode>, ISemanticNode> action)
            : this(name, rule)
        {
            AttachAction(action);
        }

        public Match<IParseNode> Match(Grammar grammar, List<Lexeme> lexemes)
        {
            Debug.WriteLine($"? {Name} {string.Join(" ", lexemes.Select(n => n.MatchedText))}");

            var match = Rule.Match(grammar, lexemes);
            if (match.Success)
            {
                Debug.WriteLine($"+ {Name}");
                return new Match<IParseNode>(new BranchParseNode(this, new[] { match.Result }), true);
            }
            else
            {
                Debug.WriteLine($"- {Name}");
                return new Match<IParseNode>(null, false);
            }
        }

        public IRule Clone()
        {
            return new NamedRule(Name, Rule.Clone(), Action);
        }

        public void AttachAction(Func<IParseNode, Func<IParseNode, ISemanticNode>, ISemanticNode> action)
        {
            if (Action != DefaultAction)
            { throw new InvalidOperationException("Action already attached."); }

            Action = action ?? throw new ArgumentNullException(nameof(action));
        }

        static ISemanticNode DefaultAction(IParseNode node, Func<IParseNode, ISemanticNode> recurse)
        {
            var branch = (BranchParseNode)node;
            var children = branch.Children
                .Select(c =>
                {
                    if (c is LeafParseNode)
                    {
                        throw new Exception("DefaultAction cannot handle leaf nodes.");
                    }
                    else if (c.Rule is NamedRule)
                    {
                        return recurse(c);
                    }
                    else
                    {
                        return DefaultAction(c, recurse);
                    }
                })
                .Where(c => c != null)
                .ToList();

            if (children.Count == 1)
            {
                return children[0];
            }
            else
            {
                return new BranchSemanticNode(0, children);
            }
        }
    }
}
