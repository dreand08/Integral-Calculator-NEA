using Computer_Science_NEA.FunctionHandling;
using Computer_Science_NEA.FunctionHandling.Parsing;
using Computer_Science_NEA.FunctionHandling.SymbolicMath;
using Computer_Science_NEA.Integration;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace Computer_Science_NEA
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var expr = ExpressionParser.Parse("x(1 + x^2)^-0.5");
            Console.WriteLine(expr);
            Console.WriteLine(expr.Integrate("x"));
        }
    }
}