using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Computer_Science_NEA.FunctionHandling.SymbolicMath
{
    //This is an abstract base class which will store the inside of the expression for special functions
    //such as sin, cos etc. It will handle simplification.
    public abstract class UnaryExpression : Expression
    {
        public Expression Inner { get; }

        protected UnaryExpression(Expression inner)
        {
            Inner = inner;
        }

        public override Expression Substitute(Expression target, Expression replacement)
        {
            if (Equals(target)) return replacement;
            return Rebuild(Inner.Substitute(target, replacement));
        }

        // Each subclass tells UnaryExpression how to rebuild itself
        protected abstract Expression Rebuild(Expression newInner);
    }
}
