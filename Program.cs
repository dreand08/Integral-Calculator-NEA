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

            // x * exp(2x)
            var t1 = MultiplyExpression.Make(x, ExpExpression.Make(MultiplyExpression.Make(new NumberExpression(2m), x))).Simplify();
            Console.WriteLine(t1);
            Console.WriteLine(t1.Integrate("x"));

            // x * sin(3x)
            var t2 = MultiplyExpression.Make(x, SinExpression.Make(MultiplyExpression.Make(new NumberExpression(3m), x))).Simplify();
            Console.WriteLine(t2);
            Console.WriteLine(t2.Integrate("x"));

            // x * cos(5x + 1)
            var t3 = MultiplyExpression.Make(x, CosExpression.Make(AddExpression.Make(MultiplyExpression.Make(new NumberExpression(5m), x), new NumberExpression(1m)))).Simplify();
            Console.WriteLine(t3);
            Console.WriteLine(t3.Integrate("x"));


        }
    }
}