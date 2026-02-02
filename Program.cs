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
            Console.WriteLine(LnExpression.Make(x).Differentiate("x"));
            Console.WriteLine(LnExpression.Make(PowerExpression.Make(x, new NumberExpression(2m))).Differentiate("x"));
        }
    }
}