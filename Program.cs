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

            // 1) x^-1
            Console.WriteLine(PowerExpression.Make(x, new NumberExpression(-1m)).Integrate("x")); // ln(x)

            // 2) exp(x^2) * 2x
            var expr1 = MultiplyExpression.Make(
                ExpExpression.Make(PowerExpression.Make(x, new NumberExpression(2m))),
                new NumberExpression(2m),
                x
            );
            Console.WriteLine(expr1.Simplify().Integrate("x")); // exp(x^2)

            // 3) sin(x^3) * 3x^2
            var expr2 = MultiplyExpression.Make(
                SinExpression.Make(PowerExpression.Make(x, new NumberExpression(3m))),
                new NumberExpression(3m),
                PowerExpression.Make(x, new NumberExpression(2m))
            );
            Console.WriteLine(expr2.Simplify().Integrate("x")); // -cos(x^3)

            // 4) tan(x) = sin(x) * cos(x)^-1
            var tanAsSinOverCos = MultiplyExpression.Make(
                SinExpression.Make(x),
                PowerExpression.Make(CosExpression.Make(x), new NumberExpression(-1m))
            );
            Console.WriteLine(tanAsSinOverCos.Simplify().Integrate("x")); // -ln(cos(x))
        }
    }
}