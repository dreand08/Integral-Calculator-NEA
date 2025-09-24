using Computer_Science_NEA.FunctionHandling;

namespace Computer_Science_NEA
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var f = new MathFunction("sqrt(max(y ,abs(sec(x * floor(a)) * arctan(2 + 9 * arctanh(min(ln(sin(a)), pow(pow(x,y),z))))))))");
            var inputs = new Dictionary<string, double> { {"a", Math.PI / 2 },{ "x", 3 }, { "y", 8 }, { "z", 2 } };
            var result = f.Evaluate(inputs);
            Console.WriteLine(result);
        }
    }
}
