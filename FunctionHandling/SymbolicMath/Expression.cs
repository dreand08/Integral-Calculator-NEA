using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Computer_Science_NEA.FunctionHandling.SymbolicMath
{
    public abstract class Expression //This is the class in function handling which is going to transform the function into an expression tree.
    {
        public virtual int Precedence => 100; //Operations have different precedence determining it's 'binding strength'

        public abstract Expression Simplify(); //Returns simplified version of expression

        public virtual Expression Substitute(Expression target, Expression replacement) //Used to substitute nodes with different expressions
        {
            return Equals(target) ? replacement : this;
        }

        public virtual void Walk(Action<Expression> visit) //Pre-Order traversal
        {
            visit?.Invoke(this);
        }

        protected string WithParentsIfNeeded(Expression child) //Helps with pretty-printing child expressions with brackets when needed.
        {
            if (child == null) return String.Empty;
            var s = child.ToString();
            return child.Precedence < this.Precedence ? $"({s})" : s;
        }
        public override string ToString() => GetType().Name;

        public abstract Expression Differentiate(string variable); //So we can do reverse chain rule.
    }
}
