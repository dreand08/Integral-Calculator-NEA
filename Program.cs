using Computer_Science_NEA.FunctionHandling;
using Computer_Science_NEA.Integration;
using System.Linq.Expressions;

namespace Computer_Science_NEA
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var f = new MathFunction("exp(arctanh(x)) * (2.0 / x) * arctanh((2 * x) / (1 + 2 * pow(x,2)))");
            var integral1 = new TrapeziumIntegrator(f, -0.9, 0.9, 200, "x");
            var integral2 = new SimpsonIntegrator(f, -0.9, 0.9, 200, "x");

            double result1 = integral1.Integrate();
            double result2 = integral2.Integrate();

            var estimator = new IntegrationErrorEstimator(f, -0.9, 0.9, 200, "x");

            Console.WriteLine($"Using trapezium integration: {result1}");
            Console.WriteLine($"The percentage error of the estimation: {estimator.EstimateTrapeziumPercentError(result1)}");

            Console.WriteLine($"Using simpson integration: {result2}");
            Console.WriteLine($"The percentage error of the estimation: {estimator.EstimateTrapeziumPercentError(result2)}");
        }
    }
}
