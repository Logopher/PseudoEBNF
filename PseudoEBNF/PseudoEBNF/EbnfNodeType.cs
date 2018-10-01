using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PseudoEBNF.PseudoEBNF
{
    public enum EbnfNodeType
    {
        String,
        Regex,
        Identifier,

        And,
        Or,
        Not,
        Optional,
        Repeat,
        Group,

        Token,
        Rule,

        Root,
        None,
    }
}
