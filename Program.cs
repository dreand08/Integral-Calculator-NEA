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

            Console.WriteLine(AddExpression.Make(x, new NumberExpression(5m)).Differentiate("x"));

            Console.WriteLine(MultiplyExpression.Make(new NumberExpression(3m), x).Differentiate("x"));

            Console.WriteLine(PowerExpression.Make(x, new NumberExpression(4m)).Differentiate("x"));

            Console.WriteLine(PowerExpression.Make(AddExpression.Make(x, new NumberExpression(1m)), new NumberExpression(3m)).Differentiate("x"));

            Console.WriteLine(MultiplyExpression.Make(new NumberExpression(4m), PowerExpression.Make(AddExpression.Make(x, new NumberExpression(2m)), new NumberExpression(6m))).Differentiate("x"));
        }
    }
}
