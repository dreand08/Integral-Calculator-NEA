using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Computer_Science_NEA.FunctionHandling.SymbolicMath
{
    public sealed class PowerExpression : Expression
    {
        public Expression BaseExpr { get; }
        public Expression Exponent { get; }

        private PowerExpression(Expression baseExpr, Expression exponent)
        {
            BaseExpr = baseExpr;
            Exponent = exponent;
        }

        public override int Precedence => 40; // Higher than multiply

        private static bool IsNumber(Expression e, out decimal v)
        {
            if (e is NumberExpression n)
            {
                v = n.Value;
                return true;
            }
            v = 0m;
            return false;
        }

        private static bool IsZero(Expression e) =>
            IsNumber(e, out var v) && v == 0m;

        private static bool IsOne(Expression e) =>
            IsNumber(e, out var v) && v == 1m;

        public static Expression Make(Expression b, Expression e)
        {
            if (IsZero(b)) 
        }
    }
}
