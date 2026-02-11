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
            VariableExpression x = new VariableExpression("x");
            Console.WriteLine(MultiplyExpression.Make(x, PowerExpression.Make(AddExpression.Make(new NumberExpression(1), PowerExpression.Make(x, new NumberExpression(2))), new NumberExpression(-0.5m))).Integrate("x"));
        }
    }
}