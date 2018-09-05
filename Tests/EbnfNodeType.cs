using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
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
    }
}
