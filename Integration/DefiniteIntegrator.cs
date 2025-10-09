using Computer_Science_NEA.FunctionHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Computer_Science_NEA.Integration
{
    public abstract class DefiniteIntegrator
    {
        //Properties
        public double N { get; set; } //Number of rectangles - higher = more accurate
        public double LowerBound { get; set; } //Starting Value
        public double UpperBound { get; set; } //Ending Value
        public MathFunction Function { get; set; }
        public string Variable { get; set; } // Which variable are we integrating with respect to.

        //Constructor
        public DefiniteIntegrator(MathFunction function, double lowerbound, double upperbound, int n, string variable)
        {
            Function = function;
            LowerBound = lowerbound;
            UpperBound = upperbound;
            if (n <= 0)
            {
                throw new Exception("N must be greater than 0");
            }
            else
            {
                N = n;
            }
            Variable = variable;
        }
        //Methods
        public abstract double Integrate();

        public double EvaluateAt(double x)
        {
            return Function.Evaluate(new() { { Variable, x } });
        }
    }
}
