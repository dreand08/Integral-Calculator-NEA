using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Computer_Science_NEA.FunctionHandling.SymbolicMath
{
    public sealed class LnExpression : UnaryExpression
    {
        private LnExpression(Expression inner) : base(inner) { }

        public override int Precedence => 80;

        public static Expression Make(Expression inner)
        {
            // ln(1) = 0
            if (inner is NumberExpression n && n.Value == 1m)
                return new NumberExpression(0m);

            return new LnExpression(inner);
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
            // (ln u)' = u' * u^(-1)
            return MultiplyExpression.Make(Inner.Differentiate(variable), PowerExpression.Make(Inner, new NumberExpression(-1m))).Simplify();
        }

        public override Expression Integrate(string variable)
        {
            throw new NotSupportedException("Integrate: ln not supported yet.");
        }

        public override string ToString() => $"ln({Inner})";
    }
}
