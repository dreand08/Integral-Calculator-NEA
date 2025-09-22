using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Mathos.Parser; //A Library used for parsing string into math.

namespace Computer_Science_NEA.FunctionHandling
{
    public class MathFunction
    {
        //Properties 
        public string Expression { get; set; }
        public MathParser Parser { get; set; }

        //I initially had a shared parser but i removed it because it was causing tests to affect one another.
        //Constructor
        public MathFunction(string expression)
        {
            Expression = expression;
            Parser = new MathParser();
            AddCustomMappings(Parser);
            //We must add mappings since MathParser only knows basic arithmetic functions.
            //In practice we are teaching the parser what all these different functions mean.
        }
        //Methods
        public double Evaluate(Dictionary<string, double> variables) // Acts as a calculator, i.e. what is f(x) when x = 3, useful for definite integration.
        {
            Parser.LocalVariables.Clear(); //Clears old variables

            foreach (var kvp in variables)//Iterates through each key-value pair in the dictionary.
            {
                Parser.LocalVariables[kvp.Key] = kvp.Value;
            }
            return Parser.Parse(Expression.ToLower());
        }
        private static void AddCustomMappings(MathParser parser)
        {
            //Custom Mappings
            if (!parser.LocalFunctions.ContainsKey("ln")) // A safety precaution to ensure we aren't adding the same mapping more than once.
            {
                parser.LocalFunctions.Add("ln", args =>
                {
                    if (args[0] <= 0) // Args is a list of arguments pssed into the function. args[0] is just the first value in this double array.
                    {
                        throw new ArgumentException("ln(x) only takes x > 0");
                    }
                    return Math.Log(args[0]);
                });
            }
            // Here we added ln, and threw an exception for invalid inputs. 
            //Now we just have to repeat for all functions.
            //List of possible inputs: sin, cos, tan, arctan, arcsin, arcos, sinh, cosh, tanh, arcsinh, arcosh, artanh, e, ln, log10, sqrt, abs, 
            if (!parser.LocalFunctions.ContainsKey("sin"))
            {
                parser.LocalFunctions.Add("sin", args => Math.Sin(args[0])); //Defined for all x.
            }
            if (!parser.LocalFunctions.ContainsKey("cos"))
            {
                parser.LocalFunctions.Add("cos", args => Math.Cos(args[0]));
            }
            if (!parser.LocalFunctions.ContainsKey("tan"))
            {
                parser.LocalFunctions.Add("tan", args => Math.Tan(args[0]));
            }
            if (!parser.LocalFunctions.ContainsKey("log"))
            {
                parser.LocalFunctions.Add("log", args =>
                {
                    if (args[0] <= 0)
                    {
                        throw new ArgumentException("log10(x) only takes x > 0");
                    }
                    return Math.Log10(args[0]);
                });
            }
            if (!parser.LocalFunctions.ContainsKey("abs"))
            {
                parser.LocalFunctions.Add("abs", args => Math.Abs(args[0]));
            }
            if (!parser.LocalFunctions.ContainsKey("sqrt"))
            {
                parser.LocalFunctions.Add("sqrt", args =>
                {
                    if (args[0] < 0)
                    {
                        throw new ArgumentException("Sqrt only takes x >= 0");
                    }
                    return Math.Sqrt(args[0]);
                });
            }
            if (!parser.LocalFunctions.ContainsKey("exp"))
            {
                parser.LocalFunctions.Add("exp", args => Math.Exp(args[0]));
            }
            if (!parser.LocalFunctions.ContainsKey("sinh"))
            {
                parser.LocalFunctions.Add("sinh", args => Math.Sinh(args[0]));
            }
            if (!parser.LocalFunctions.ContainsKey("cosh"))
            {
                parser.LocalFunctions.Add("cosh", args => Math.Cosh(args[0]));
            }
            if (!parser.LocalFunctions.ContainsKey("tanh"))
            {
                parser.LocalFunctions.Add("tanh", args => Math.Tanh(args[0]));
            }
            if (!parser.LocalFunctions.ContainsKey("arcsin"))
            {
                parser.LocalFunctions.Add("arcsin", args =>
                {
                    if (args[0] > 1 || args[0] < -1)
                    {
                        throw new ArgumentException("Arcsin only takes -1<x<1");
                    }
                    return Math.Asin(args[0]);
                });
            }
            if (!parser.LocalFunctions.ContainsKey("arcos"))
            {
                parser.LocalFunctions.Add("arcos", args =>
                {
                    if (args[0] > 1 || args[0] < -1)
                    {
                        throw new ArgumentException("Arcos only takes -1<x<1");
                    }
                    return Math.Acos(args[0]);
                });
            }
            if (!parser.LocalFunctions.ContainsKey("arctan"))
            {
                parser.LocalFunctions.Add("arctan", args => Math.Atan(args[0]));
            }
            if (!parser.LocalFunctions.ContainsKey("arcsinh"))
            {
                parser.LocalFunctions.Add("arcsinh", args => Math.Log(args[0] + Math.Sqrt(Math.Pow(args[0], 2) + 1)));
            }
            if (!parser.LocalFunctions.ContainsKey("arcosh"))
            {
                parser.LocalFunctions.Add("arcosh", args => // Arcosh, Arcsinh, Arctanh aren't defined in Math library so must be implemented manually.
                {
                    if (args[0] < 1)
                    {
                        throw new ArgumentException("Arcosh only takes x >= 1");
                    }
                    return Math.Log(args[0] + Math.Sqrt(Math.Pow(args[0], 2) - 1));
                });
            }
            if (!parser.LocalFunctions.ContainsKey("arctanh"))
            {
                parser.LocalFunctions.Add("arctanh", args =>
                {
                    if (args[0] <= -1 || args[0] >= 1)
                    {
                        throw new ArgumentException("Arctanh only takes -1<x<1");
                    }
                    return 0.5 * Math.Log((1 + args[0]) / (1 - args[0]));
                });
            }
            if (!parser.LocalFunctions.ContainsKey("sec"))
            {
                parser.LocalFunctions.Add("sec", args =>
                {
                    if (Math.Cos(args[0]) == 0)
                    {
                        throw new ArgumentException("Sec cannot take cosx = 0");
                    }
                    return 1 / Math.Cos(args[0]);
                });
            }
            if (!parser.LocalFunctions.ContainsKey("cosec"))
            {
                parser.LocalFunctions.Add("cosec", args =>
                {
                    if (args[0] == 0)
                    {
                        throw new ArgumentException("Cosec cannot take x = 0");
                    }
                    return 1 / Math.Sin(args[0]);
                });
            }
            if (!parser.LocalFunctions.ContainsKey("cot"))
            {
                parser.LocalFunctions.Add("cot", args =>
                {
                    if (args[0] == 0)
                    {
                        throw new ArgumentException("Cot cannot take x = 0");
                    }
                    return 1 / Math.Tan(args[0]);
                });
            }
            if (!parser.LocalFunctions.ContainsKey("floor"))
            {
                parser.LocalFunctions.Add("floor", args => Math.Floor(args[0]));
            }
            if (!parser.LocalFunctions.ContainsKey("ceil"))
            {
                parser.LocalFunctions.Add("ceil", args => Math.Ceiling(args[0]));
            }
            if (!parser.LocalFunctions.ContainsKey("round"))
            {
                parser.LocalFunctions.Add("round", args => Math.Round(args[0]));
            }
            if (!parser.LocalFunctions.ContainsKey("sign"))
            {
                parser.LocalFunctions.Add("sign", args => Math.Sign(args[0])); // Returns sign of argument
            }
            if (!parser.LocalFunctions.ContainsKey("min"))
            {
                parser.LocalFunctions.Add("min", args => args.Min()); // This allows for 1 or more values in the min/max functions.
            }
            if (!parser.LocalFunctions.ContainsKey("max"))
            {
                parser.LocalFunctions.Add("max", args => args.Max());
            }
            if (!parser.LocalFunctions.ContainsKey("pow"))
            {
                parser.LocalFunctions.Add("pow", args =>
                {
                    if (args.Length != 2)
                    {
                        throw new ArgumentException("Please input a value and its power in the form pow(x,y)");
                    }
                    return Math.Pow(args[0], args[1]);
                });
            }
            if (!parser.LocalFunctions.ContainsKey("arctan2"))
            {
                parser.LocalFunctions.Add("arctan2", args =>
                {
                    if (args.Length != 2)
                    {
                        throw new ArgumentException("Please input an x and y value for finding the arctan of x/y measured from x-axis.");
                    }
                    return Math.Atan2(args[1], args[0]); // y/x
                });
            }
        }
    }
}
