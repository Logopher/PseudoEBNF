using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Reporting;
using PseudoEBNF.Semantics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PseudoEBNF.Parsing.Rules
{
    public class NamedRule : IRule
    {
        public string Name { get; }

        public IRule Rule { get; }

        public Func<BranchParseNode, Func<BranchParseNode, ISemanticNode>, ISemanticNode> Action { get; private set; } = DefaultAction;

        public NamedRule(string name, IRule rule)
        {
            Name = name;
            Rule = rule;
        }

        NamedRule(string name, IRule rule, Func<BranchParseNode, Func<BranchParseNode, ISemanticNode>, ISemanticNode> action)
            : this(name, rule)
        {
            AttachAction(action);
        }

        public Match<IParseNode> Match(Supervisor super, Grammar grammar, List<Lexeme> lexemes)
        {
            super.ReportHypothesis(this, lexemes.FirstOrDefault()?.StartIndex);

            var match = Rule.Match(super, grammar, lexemes);
            if (match.Success)
            {
                super.ReportSuccess(this, match.Result.MatchedText);
                return new Match<IParseNode>(new BranchParseNode(this, new[] { match.Result }), true);
            }
            else
            {
                super.ReportFailure(this);
                return new Match<IParseNode>(null, false);
            }
        }

        public IRule Clone()
        {
            return new NamedRule(Name, Rule.Clone(), Action);
        }

        public void AttachAction(Func<BranchParseNode, Func<BranchParseNode, ISemanticNode>, ISemanticNode> action)
        {
            if (Action != DefaultAction)
            { throw new InvalidOperationException("Action already attached."); }

            Action = action ?? throw new ArgumentNullException(nameof(action));
        }

        static ISemanticNode DefaultAction(BranchParseNode branch, Func<BranchParseNode, ISemanticNode> recurse)
        {
            if (branch.Rule is NamedRule rule)
            { throw new Exception($"No action specified for named rule: {rule.Name}"); }

            branch = branch.Unwrap();

            if (branch.Rule is NamedRule)
            { return recurse(branch); }

            var children = branch.Elements
            .Select(n =>
            {
                if (n.Rule is NamedRule)
                { return recurse(n); }
                else
                { return DefaultAction(n, recurse); }
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
