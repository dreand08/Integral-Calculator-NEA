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

            // sin(3x)
            Console.WriteLine(
                SinExpression.Make(MultiplyExpression.Make(new NumberExpression(3m), x))
                    .Integrate("x")
            ); // expect: (-1/3) * cos(3*x)

            // cos(5x + 1)
            Console.WriteLine(
                CosExpression.Make(AddExpression.Make(
                    MultiplyExpression.Make(new NumberExpression(5m), x),
                    new NumberExpression(1m)
                )).Integrate("x")
            ); // expect: (1/5) * sin(5*x + 1)

            // exp(2x)
            Console.WriteLine(
                ExpExpression.Make(MultiplyExpression.Make(new NumberExpression(2m), x))
                    .Integrate("x")
            ); // expect: (1/2) * exp(2*x)
        }
    }
}