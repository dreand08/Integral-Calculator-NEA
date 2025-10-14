using Computer_Science_NEA.FunctionHandling;
using Computer_Science_NEA.Integration;
using System.Linq.Expressions;

namespace Computer_Science_NEA
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var f = new MathFunction("sin(pow(x,2))");
            var integral1 = new SimpsonIntegrator(f, 0, 3, 1000, "x");
            Console.WriteLine(integral1.Integrate());
        }
    }
}
