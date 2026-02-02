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
            
            // (u^a)^b => u^(a*b)
            if (b is PowerExpression innerPow && IsNumber(innerPow.Exponent, out var a) && IsNumber(e, out var bExp))
            {
                return Make(innerPow.BaseExpr, new NumberExpression(a * bExp));
            }

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

        public override Expression Differentiate(string variable)
        {
            // Only handle constant numeric exponent for now
            if (Exponent is NumberExpression n)
            {
                var u = BaseExpr;
                var du = u.Differentiate(variable);

                // If du is 0, whole thing is 0
                if (du is NumberExpression dn && dn.Value == 0m)
                    return new NumberExpression(0m);

                // d/dx(u^0) = d/dx(1) = 0 (already simplified by Make, but safe)
                if (n.Value == 0m) return new NumberExpression(0m);

                // n * u^(n-1) * u'
                return MultiplyExpression.Make(
                    new NumberExpression(n.Value),
                    PowerExpression.Make(u, new NumberExpression(n.Value - 1m)),
                    du
                ).Simplify();
            }

            // Not supported yet (u^v where v isn't constant)
            throw new NotSupportedException("Differentiate: non-constant exponent not supported yet.");
        }

        public override Expression Integrate(string variable)
        {
            // If constant w.r.t variable: C dx = Cx
            if (IsConstantWrt(variable))
                return MultiplyExpression.Make(this, new VariableExpression(variable)).Simplify();

            // Case 1: x^n where n is real
            if (BaseExpr is VariableExpression v && v.Name == variable && Exponent is NumberExpression n)
            {
                // Special case n = -1 -> ln|x|
                if (n.Value == -1m)
                    return LnExpression.Make(BaseExpr).Simplify();

                // x^(n+1) * 1/(n+1)
                var newExp = n.Value + 1m;
                var coeff = 1m / newExp;

                return MultiplyExpression.Make(new NumberExpression(coeff), PowerExpression.Make(BaseExpr, new NumberExpression(newExp))).Simplify();
            }

            // Case 2: a^x where a is a real constant
            // ∫ a^x dx = a^x / ln(a)
            if (BaseExpr is NumberExpression aNum && Exponent is VariableExpression vx && vx.Name == variable)
            {

            }
        }
    }
}
