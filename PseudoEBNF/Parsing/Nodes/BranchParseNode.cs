using PseudoEBNF.Common;
using PseudoEBNF.Parsing.Rules;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PseudoEBNF.Parsing.Nodes
{
    public class BranchParseNode : IParseNode
    {
        private readonly LeafParseNode _leaf;
        private readonly IReadOnlyList<BranchParseNode> _elements;
        private readonly IReadOnlyList<BranchParseNode> _branches;

        public int StartIndex { get; }
        public Rule Rule { get; }
        public string MatchedText { get; }
        public int Length { get; }
        public int LexemeCount { get; }
        public bool IsTwig { get; }

        public LeafParseNode Leaf => IsTwig ? _leaf : throw new Exception();
        public IReadOnlyList<BranchParseNode> Branches => !IsTwig ? _branches : throw new Exception();
        public IReadOnlyList<BranchParseNode> Elements => !IsTwig ? _elements : throw new Exception();

        public BranchParseNode(Rule rule, int index, params IParseNode[] nodes)
            : this(rule, index, (IEnumerable<IParseNode>)nodes)
        {
        }

        public BranchParseNode(Rule rule, int index, IEnumerable<IParseNode> nodes)
        {
            Rule = rule;
            var children = nodes
                .Where(n => n != null)
                .ToArray();
            var withoutImplicit = children
                .Where(n => !(n.Rule is NamedRule named && named.Name == RuleName.Implicit))
                .ToArray();

            if (withoutImplicit.Length == 1 && withoutImplicit.Single() is LeafParseNode leaf)
            {
                IsTwig = true;
                _leaf = leaf;
            }
            else if (withoutImplicit.Length == 1 && withoutImplicit.Single() is BranchParseNode branch && !(branch.Rule is NamedRule) && branch.IsTwig)
            {
                IsTwig = true;
                _leaf = branch.Leaf;
            }
            else
            {
                IsTwig = false;
                _branches = withoutImplicit
                    .Cast<BranchParseNode>()
                    .ToArray();

                if (Branches.Count == 1 && !(rule is RepeatRule))
                {
                    if (rule is NamedRule)
                    {
                        branch = Branches
                            .Single()
                            .Unwrap();

                        if (branch.Rule is NamedRule || branch.IsTwig)
                        { _elements = new[] { branch }; }
                        else
                        { _elements = branch.Elements; }
                    }
                    else
                    {
                        branch = Unwrap();

                        if (branch.IsTwig)
                        { _elements = new[] { branch }; }
                        else
                        { _elements = branch.Elements; }
                    }
                }
                else
                {
                    _elements = Branches
                        .Select(b => b.Unwrap())
                        .ToArray();
                }
            }

            StartIndex = index;
            MatchedText = string.Join("", children.Select(n => n.MatchedText));
            Length = children.Sum(n => n.Length);
            LexemeCount = children.Sum(n => n.LexemeCount);
        }

        public BranchParseNode Unwrap()
        {
            if (IsTwig)
            { return this; }
            else if (Rule is NamedRule)
            { return this; }
            else if (Rule is RepeatRule)
            { return this; }
            else if (Branches.Count == 1)
            { return Branches.Single().Unwrap(); }
            else
            { return this; }
        }

        public BranchParseNode GetDescendant(int first, params int[] rest)
        {
            var address = new[] { first }.Concat(rest);

            BranchParseNode branch;
            if (Rule is NamedRule)
            { branch = this; }
            else
            { branch = Unwrap(); }

            foreach (var index in address)
            {
                if (!branch.IsTwig && index < branch.Elements.Count)
                { branch = branch.Elements[index]; }
                else
                { return null; }
            }

            return branch;
        }
    }
}
