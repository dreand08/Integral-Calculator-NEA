using Computer_Science_NEA.FunctionHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Computer_Science_NEA.Integration
{
    class SimpsonIntegrator : DefiniteIntegrator
    {
        //Simpson rule is more complex than trapezium rule and involves splitting the function
        //into rectangles and approximating the area within each rectangle using a parabola which passes
        //through the corners of the rectangle.
        //Summing these approximations together gives an approximation for the whole area.
        //Generally, it is more accurate than trapezium rule.
        //Formula: A = h/3[y0 + 4(y1 + y3 + ... + y(n-1)) + 2(y2 + y4 + ... + y(n-2)) + yn]
        
        public SimpsonIntegrator(MathFunction function, double lowerbound, double upperbound, int n, string variable) : base(function, lowerbound, upperbound, n, variable)
        {
            if (n % 2 != 0)
            {
                throw new Exception("Simpsons Rule only takes an even number of intervals");
            }
        }
        public override double Integrate()
        {
            double area = 0;
            double d = (UpperBound - LowerBound) / N;
            for (int i = 1; i < N; i++)
            {
                if (i % 2 == 0)
                {
                    area = area + 2 * Function.Evaluate(new() { { Variable, LowerBound + i * d } });
                }
                else
                {
                    area = area + 4 * Function.Evaluate(new() { { Variable, LowerBound + i * d } });
                }
            }
            area = (d / 3.0) * (area + Function.Evaluate(new() { { Variable, LowerBound } }) + Function.Evaluate(new() { { Variable, UpperBound } }));
            return area;
        }
    }
}
