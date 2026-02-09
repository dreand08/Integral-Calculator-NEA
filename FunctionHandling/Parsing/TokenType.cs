using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Computer_Science_NEA.FunctionHandling.Parsing
{
    // What kind of token something is
    public enum TokenType
    {
        Number, 
        Identifier,

        Plus,
        Minus,
        Star,
        Caret,

        LParen, // (
        RParen, // )

        End             
    }
    
}
