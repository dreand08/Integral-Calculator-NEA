using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Computer_Science_NEA.FunctionHandling;

namespace Computer_Science_NEA.Integration
{
    public class IntegrationErrorEstimator 
    {
        //The purpose of this class is to find the % error of definite integration without having the true value.
        //This is done using absolute error formulas which involve differentiation.
        //Therefore we need to also build numerical differentiation.
        //For trapezium integration the formula is: abs(Error) = (b - a)(h^2)/12 * max(abs(f(2)(x)))
        //For simpson integration the formula is: abs(Error) = (b - a)(h^4)/180 * max(abs(f(4)(x)))

        //Properties
        public MathFunction Function { get; set; }
        public double LowerBound { get; set; }
        public double UpperBound { get; set; }
        public double N { get; set; }
        public string Variable { get; set; }

        //Constructor
        public IntegrationErrorEstimator(MathFunction function, double lowerBound, double upperBound, int n, string variable)
        {
            Function = function;
            LowerBound = lowerBound;
            UpperBound = upperBound;
            N = n;
            Variable = variable;
        }

        //Methods
        public double Evaluate(double x) => Function.Evaluate(new() { { "x", x } });
        //So we can evaluate within the class more easily

        public double SecondDerivative(double x, double h) //Using central difference formula for second derivative
        {
            return (Evaluate(x + h) - 2 * Evaluate(x) + Evaluate(x - h)) / (h * h);
        }

        public double FourthDerivative(double x, double h) //Using central difference formula for fourth derivative
        {
            return (Evaluate(x + 2 * h) - 4 * Evaluate(x + h) + 6 * Evaluate(x) - 4 * Evaluate(x - h) + Evaluate(x - 2 * h)) / (Math.Pow(h, 4.0));
        }

        //Max of the derivatives
        public double MaxAbsSecondDerivative(double step)
        {
            double maxValue = 0;
            for (double x = LowerBound; x <= UpperBound; x += step)
            {
                double value = Math.Abs(SecondDerivative(x, step / 10.0)); //Checks the second derivative repeatedly with very small increments
                if (!double.IsNaN(value) && value > maxValue)
                {
                    maxValue = value;
                }
            }
            return maxValue;
        }

        public double MaxAbsFourthDerivative(double step)
        {
            double maxValue = 0;
            for (double x = LowerBound; x <= UpperBound; x += step)
            {
                double value = Math.Abs(FourthDerivative(x, step / 10.0));
                if (!double.IsNaN(value) && value > maxValue)
                {
                    maxValue = value;
                }
            }
            return maxValue;
        }

        //Error Estimation Formulas

        public double EstimateTrapeziumAbsoluteError()
        {
            double h = (UpperBound - LowerBound) / N;
            double maxSecond = MaxAbsSecondDerivative(h);
            return ((UpperBound - LowerBound) * h * h / 12.0) * maxSecond;
        }

        public double EstimateSimpsonAbsoluteError()
        {
            double h = (UpperBound - LowerBound) / N;
            double maxFourth = MaxAbsFourthDerivative(h);
            return ((UpperBound - LowerBound) * Math.Pow(h, 4) / 180.0) * maxFourth;
        }

        //Percentage Error

        public double EstimateTrapeziumPercentError(double integralValue)
        {
            double absError = EstimateTrapeziumAbsoluteError();
            return Math.Abs(absError / integralValue) * 100.0;
        }

        public double EstimateSimpsonPercentError(double integralValue)
        {
            double absError = EstimateSimpsonAbsoluteError();
            return Math.Abs(absError / integralValue) * 100.0;
        }
    }
}
