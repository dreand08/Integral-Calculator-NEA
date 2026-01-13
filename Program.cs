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
            var y = new VariableExpression("y");

            Console.WriteLine(PowerExpression.Make(x, new NumberExpression(0m))); // 1
            Console.WriteLine(PowerExpression.Make(x, new NumberExpression(1m))); // x
            Console.WriteLine(PowerExpression.Make(new NumberExpression(1m), x)); // 1
            Console.WriteLine(PowerExpression.Make(new NumberExpression(0m), x)); // 0

            Console.WriteLine(PowerExpression.Make(new NumberExpression(2m),new NumberExpression(3m))); // 8

            Console.WriteLine(PowerExpression.Make(x, new NumberExpression(-1m))); // x^-1

            Console.WriteLine(MultiplyExpression.Make(new NumberExpression(2m),PowerExpression.Make(x, new NumberExpression(3m)))); // 2 * x^3

            Console.WriteLine(PowerExpression.Make(AddExpression.Make(x, y),new NumberExpression(2m))); // (x + y)^2

            Console.WriteLine(PowerExpression.Make(PowerExpression.Make(x, new NumberExpression(1m)),new NumberExpression(1m))); // x

            Console.WriteLine(MultiplyExpression.Make(new NumberExpression(5m),PowerExpression.Make(x, new NumberExpression(0m)))); // 5



        }
    }
}
