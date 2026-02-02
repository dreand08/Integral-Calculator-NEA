using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Computer_Science_NEA.FunctionHandling.SymbolicMath
{
    public sealed class ExpExpression : UnaryExpression
    {
        private ExpExpression(Expression inner) : base(inner) { }

        public override int Precedence => 80;

        public static Expression Make(Expression inner)
        {
            // exp(0) = 1
            if (inner is NumberExpression n && n.Value == 0m)
                return new NumberExpression(1m);

            return new ExpExpression(inner);
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
            // (exp u)' = exp(u) * u'
            return MultiplyExpression.Make(ExpExpression.Make(Inner), Inner.Differentiate(variable)).Simplify();
        }

        public override Expression Integrate(string variable)
        {
            // ∫ exp(x) dx = exp(x)
            if (Inner is VariableExpression v && v.Name == variable)
            {
                return ExpExpression.Make(Inner).Simplify();
            }

            throw new NotSupportedException("Integrate: only exp(x) supported directly (chain rule handled in MultiplyExpression).");
        }

        public override string ToString() => $"exp({Inner})";
    }
}
