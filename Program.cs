using Computer_Science_NEA.FunctionHandling;
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
            var x = new VariableExpression("x");
            Console.WriteLine(ExpExpression.Make(MultiplyExpression.Make(SinExpression.Make(x),CosExpression.Make(x))).Differentiate("x"));
        }
    }
}