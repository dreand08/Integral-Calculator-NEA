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
            var expression = new MathFunction("(1 / Math.Sqrt(1 - Math.Pow(x, 2))) * (Math.Log((Math.Sqrt(1 + x) + 1) / (Math.Sqrt(1 + x) - 1)))");
            var integral = new TrapeziumIntegrator(expression, 0, 1, 1000, "x");
            Console.WriteLine(integral.Integrate());
        }
    }
}
