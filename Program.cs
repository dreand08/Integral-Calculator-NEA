using Computer_Science_NEA.FunctionHandling;

namespace Computer_Science_NEA
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var f = new MathFunction("");
            var inputs = new Dictionary<string, double> { };
            var result = f.Evaluate(inputs);
            Console.WriteLine(result);
        }
    }
}
