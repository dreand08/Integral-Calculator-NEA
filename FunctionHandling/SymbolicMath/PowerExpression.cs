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
            // x^0 => 1
            if (IsZero(e)) return new NumberExpression(1m);

            // x^1 => x
            if (IsOne(e)) return b;

            // 0^x => 0 (ignoring edge cases like 0^0)
            if (IsZero(b)) return new NumberExpression(0m);

            // 1^x => 1
            if (IsOne(b)) return new NumberExpression(1m);

            // Constant folding
            if (IsNumber(b, out var bv) && IsNumber(e, out var ev))
            {
                double result = Math.Pow((double)bv, (double)ev);
                return new NumberExpression((decimal)result);
            }

            return new PowerExpression(b, e);
        }

        public override Expression Simplify()
        {
            return Make(BaseExpr.Simplify(), Exponent.Simplify());
        }

        public override Expression Substitute(Expression target, Expression replacement)
        {
            if (Equals(target)) return replacement;

            return Make(
                BaseExpr.Substitute(target, replacement),
                Exponent.Substitute(target, replacement)
            );
        }

        public override string ToString()
        {
            return $"{WithParentsIfNeeded(BaseExpr)}^{WithParentsIfNeeded(Exponent)}";
        }
    }
}
