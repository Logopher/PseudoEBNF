using EBNF.Parsing.Rules;
using System;
using System.Collections.Generic;
using System.Text;

namespace EBNF.Parsing.Nodes
{
    public interface INode
    {
        IRule Rule { get; }

        string MatchedText { get; }

        int Length { get; }

        int LexemeCount { get; }
    }
}
