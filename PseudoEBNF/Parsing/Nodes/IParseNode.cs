using PseudoEBNF.Parsing.Rules;
using System;
using System.Collections.Generic;
using System.Text;

namespace PseudoEBNF.Parsing.Nodes
{
    public interface IParseNode
    {
        IRule Rule { get; }

        string MatchedText { get; }

        int Length { get; }

        int LexemeCount { get; }
    }
}
