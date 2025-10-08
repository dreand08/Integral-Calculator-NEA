using Computer_Science_NEA.FunctionHandling;
using Computer_Science_NEA.Integration;
using System.Linq.Expressions;

namespace Computer_Science_NEA
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var f = new MathFunction("pow(x,2)");
            var integral = new TrapeziumIntegrator(f, 0, 3, 10000, "x");
            Console.WriteLine(integral.Integrate());
        }
    }
}
