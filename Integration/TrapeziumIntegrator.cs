using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Computer_Science_NEA.Integration
{
    class TrapeziumIntegrator : DefiniteIntegrator
    {
        //Trapezium method is the simplest numerical method for definite
        //integration and involves splitting a function into many rectangles
        //which approximate the height of the function. 
        //Summing these rectangles together gives an approximation for the area
        //under the curve. 
        //Depending on the number of rectangles used and the type of function 
        //there can be some error %.
        //Formula: A = h/2[y0 + 2(y1 + y2 + ... + y(n-1)) + yn]

        //Properties
        public int N { get; set; } //Number of rectangles - higher = more accurate
        public double LowerBound { get; set; } //Starting Value
        public double UpperBound { get; set; } //Ending Value

        public double Integrate()
        {

        }
    }
}
