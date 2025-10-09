using Computer_Science_NEA.FunctionHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Computer_Science_NEA.Integration
{
    class TrapeziumIntegrator  : DefiniteIntegrator
    {
        //Trapezium method is the simplest numerical method for definite
        //integration and involves splitting a function into many rectangles
        //which approximate the height of the function. 
        //Summing these rectangles together gives an approximation for the area
        //under the curve. 
        //Depending on the number of rectangles used and the type of function 
        //there can be some error %.
        //Formula: A = h/2[y0 + 2(y1 + y2 + ... + y(n-1)) + yn]

        //Constructor
        public TrapeziumIntegrator(MathFunction function, double lowerbound, double upperbound, int n, string variable) : base(function, lowerbound, upperbound, n, variable)
        {
        }
        public override double Integrate()
        {
            double area = 0;
            double d = (UpperBound - LowerBound) / N;  //Height of each trapezium
            for (int i = 1; i < N; i++)
            {
                area = area + Function.Evaluate( new() { { Variable, LowerBound + i * d} });
            }
            area = (d / 2.0) * ((area * 2) + Function.Evaluate(new() { { Variable, LowerBound } }) + Function.Evaluate(new() { { Variable, UpperBound } })); // Formula
            return area;
        }
    }
}
