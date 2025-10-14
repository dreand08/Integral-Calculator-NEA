using Computer_Science_NEA.FunctionHandling;
using Computer_Science_NEA.Integration;
using System.Linq.Expressions;

namespace Computer_Science_NEA
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var f = new MathFunction("floor(x)");
            var integral1 = new SimpsonIntegrator(f, 0, 5, 10000, "x");
            Console.WriteLine(integral1.Integrate());
        }
    }
}
