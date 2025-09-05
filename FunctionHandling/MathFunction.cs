using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mathos.Parser; //A Library used for parsing string into math.

namespace Computer_Science_NEA.FunctionHandling
{
    class MathFunction
    {
        //Properties 
        //Read-Only
        public string Expression { get; private set; }
        public MathParser Parser { get; private set; }

        //Constructor
        public MathFunction(string expression)
        {
            Expression = expression;
            Parser = new MathParser();

            //We must add mappings since MathParser only knows basic arithmetic functions.
            //Custom Mappings
            Parser.LocalFunctions.Add("ln", args =>
            {
                if (args[0] <= 0) // Args is a list of arguments pssed into the function. args[0] is just the first value in this double array.
                {
                    throw new ArgumentException("ln(x) only takes x > 0");
                }
                return Math.Log(args[0]);
            }); // Here we added ln, and threw an exception for invalid inputs. 
            //Now we just have to repeat for all functions.
            //List of possible inputs: sin, cos, tan, arctan, arcsin, arcos, sinh, cosh, tanh, arcsinh, arcosh, artanh, e, ln, log10, sqrt, abs, 
            Parser.LocalFunctions.Add("sin", args => Math.Sin(args[0])); //Defined for all x.
            Parser.LocalFunctions.Add("cos", args => Math.Cos(args[0]));
            Parser.LocalFunctions.Add("tan", args => Math.Tan(args[0]));
            Parser.LocalFunctions.Add("log", args =>
            {
                if (args[0] <= 0)
                {
                    throw new ArgumentException("log10(x) only takes x > 0");
                }
                return Math.Log10(args[0]);
            });
            Parser.LocalFunctions.Add("abs", args => Math.Abs(args[0]));
            Parser.LocalFunctions.Add("sqrt", args =>
            {
                if (args[0] < 0)
                {
                    throw new ArgumentException("Sqrt only takes x >= 0");
                }
                return Math.Sqrt(args[0]);
            });
            Parser.LocalFunctions.Add("exp", args => Math.Exp(args[0]));
            Parser.LocalFunctions.Add("sinh", args => Math.Sinh(args[0]));
            Parser.LocalFunctions.Add("cosh", args => Math.Cosh(args[0]));
            Parser.LocalFunctions.Add("tanh", args => Math.Tanh(args[0]));
            Parser.LocalFunctions.Add("arcsin", args => Math.Asin(args[0]));
        }
    }
}
