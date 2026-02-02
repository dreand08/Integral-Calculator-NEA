using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Computer_Science_NEA.FunctionHandling.SymbolicMath
{
    public sealed class CosExpression : UnaryExpression
    {
        private CosExpression(Expression inner) : base(inner) { }

        public override int Precedence => 80;

        public static Expression Make(Expression inner)
        {
            // cos(0) = 1
            if (inner is NumberExpression n && n.Value == 0m)
                return new NumberExpression(1m);

            return new CosExpression(inner);
        }

        protected override Expression Rebuild(Expression newInner)
        {
            return Make(newInner);
        }

        public override Expression Simplify()
        {
            return Make(Inner.Simplify());
        }

        public override Expression Differentiate(string variable)
        {
            // (cos u)' = -sin(u) * u'
            return MultiplyExpression.Make(new NumberExpression(-1m), SinExpression.Make(Inner), Inner.Differentiate(variable)).Simplify();
        }
        public override Expression Integrate(string variable)
        {
            // ∫ cos(x) dx = sin(x)
            if (Inner is VariableExpression v && v.Name == variable)
            {
                return SinExpression.Make(Inner).Simplify();
            }

            throw new NotSupportedException("Integrate: only cos(x) supported directly (chain rule handled in MultiplyExpression).");
        }

        public override string ToString() => $"cos({Inner})";
    }
}
