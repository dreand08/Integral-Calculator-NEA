using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Computer_Science_NEA.FunctionHandling.SymbolicMath
{
    public sealed class SinExpression : UnaryExpression 
    {
        private SinExpression(Expression inner) : base(inner) { }

        public override int Precedence => 80;

        public static Expression Make(Expression inner)
        {
            // sin(0) = 0
            if (inner is NumberExpression n && n.Value == 0m)
                return new NumberExpression(0m);

            return new SinExpression(inner);
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
            // (sin u)' = cos(u) * u'
            return MultiplyExpression.Make(CosExpression.Make(Inner), Inner.Differentiate(variable)).Simplify();
        }

        public override Expression Integrate(string variable)
        {
            // u' must be a non-zero constant k
            var du = Inner.Differentiate(variable).Simplify();

            if (du is NumberExpression k && k.Value != 0m)
            {
                // ∫ sin(u) dx = -cos(u) * (1/k)
                var inv = new NumberExpression(1m / k.Value); //Inverse 

                return MultiplyExpression.Make(inv, new NumberExpression(-1m), CosExpression.Make(Inner)).Simplify();
            }
            throw new NotSupportedException("Integrate: only sin(x) supported directly (chain rule handled in MultiplyExpression).");
        }

        public override string ToString() => $"sin({Inner})";
    }
}
