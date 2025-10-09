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
            var integral1 = new SimpsonIntegrator(f, 0, 3, 1000, "x");
            var integral2 = new TrapeziumIntegrator(f, 0, 3, 1000, "x");
            int trueValue = 9;
            Console.WriteLine($"Percantage error between the two approximations is: {Math.Abs(((integral1.Integrate() - integral2.Integrate()) / trueValue) * 100)} with 1000 intervals");
        }
    }
}
