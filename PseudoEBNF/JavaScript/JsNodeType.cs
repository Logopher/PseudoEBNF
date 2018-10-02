using System;
using System.Collections.Generic;
using System.Text;

namespace PseudoEBNF.JavaScript
{
    public enum JsNodeType
    {
        Root,
        Statement,
        Expression,
        Assignment,
        Variable,
        Identifier,
        FunctionCall,
        ArgumentList,
        Property,
        Object,
        PropertyDefinition,
        AnonymousFunction,
        NamedFunction,
        ParameterList,
        Block,
        Bitwise,
        BitwiseNegation,
        Logic,
        LogicNegation,
        Math,
        UnaryMath,
        Parenthetical,
        RegularExpression,
        Number,
        String,
        DotReference,
        KeyReference,
        Constructor,
    }
}
