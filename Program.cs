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
            var y = new VariableExpression("y");

            Console.WriteLine(PowerExpression.Make(x, new NumberExpression(5m)).Integrate("x"));
        }
    }
}