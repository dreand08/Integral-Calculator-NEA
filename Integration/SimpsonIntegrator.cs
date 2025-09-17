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
        public double Integrate()
        {
            throw new NotImplementedException();
        }
    }
}
