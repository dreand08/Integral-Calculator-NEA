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
            Console.WriteLine(MultiplyExpression.Make(x, new NumberExpression(1)));
            Console.WriteLine(MultiplyExpression.Make(x, new NumberExpression(0)));
            Console.WriteLine(MultiplyExpression.Make(x, new NumberExpression(2), new NumberExpression(3)));
            Console.WriteLine(MultiplyExpression.Make(new NumberExpression(2), AddExpression.Make(x, new NumberExpression(3), new NumberExpression(0)), new NumberExpression(10), AddExpression.Make(x, new NumberExpression(0))));
        }
    }
}
