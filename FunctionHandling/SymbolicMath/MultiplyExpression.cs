using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Computer_Science_NEA.FunctionHandling.SymbolicMath
{
    //The purpose of this class is to flatten nested multiplications,
    //multiply constants together,
    //remove * 1,
    //and x * 0 -> 0.
    public sealed class MultiplyExpression : Expression
    {
        public IReadOnlyList<Expression> Factors { get; }

        private MultiplyExpression(IReadOnlyList<Expression> factors)
        {
            Factors = factors;
        }

        public override int Precedence => 20; //Higher precedence than add so multiplication takes priority.

        private static bool IsZero(Expression e) =>
            e is NumberExpression n && n.Value == 0m;

        private static bool IsOne(Expression e) =>
            e is NumberExpression n && n.Value == 1m;

        private static bool IsNumber(Expression e) =>
            e is NumberExpression;

        private static decimal GetNumber(Expression e) =>
            e is NumberExpression n ? n.Value : 1m;

        public static Expression Make(params Expression[] rawFactors)
        {
            var nonConst = new List<Expression>();
            decimal constProduct = 1m;
            //Struggling to think through the logic, need to figure out how to fold constants, need to continue tonight.

            foreach (var f in rawFactors) // Trying to flatten and split constants from non-constants.
            {
                
            }

            //Dropping the *1
            var result = new List<Expression>();
            foreach (var f in nonConst)
            {

            }
        }
    }
}
