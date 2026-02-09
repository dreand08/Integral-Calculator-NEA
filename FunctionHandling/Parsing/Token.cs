using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Computer_Science_NEA.FunctionHandling.Parsing
{
    // A specific token found in the input string
    public readonly record struct Token(TokenType Type, string Text, int Pos);
}
