using Computer_Science_NEA.FunctionHandling;
using Computer_Science_NEA.Integration;
using System.Linq.Expressions;

namespace Computer_Science_NEA
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var f = new MathFunction("(1 + pow(x,2)) / (1 + exp(sin(x)))");
            var integral = new SimpsonIntegrator(f, -2, 2, 10000, "x");
            Console.WriteLine(integral.Integrate());

            var estimator = new IntegrationErrorEstimator(f, -2, 2, 10000, "x");
            Console.WriteLine(estimator.EstimateSimpsonPercentError(integral.Integrate()));
        }
    }
}
