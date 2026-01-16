using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Computer_Science_NEA.FunctionHandling.SymbolicMath
{
    //Variables like x, y etc.
    public sealed class VariableExpression : Expression
    {
        public string Name { get; }

        public VariableExpression(string name)
        {
            Name = name;
        }

        public override int Precedence => 90;

        public override Expression Simplify() => this;

        public override Expression Substitute(Expression target, Expression replacement)
        {
            return Equals(target) ? replacement : this;
        }

        public override string ToString() => Name;

        public override bool Equals(object obj) =>
            obj is VariableExpression v && v.Name == Name;

        public override int GetHashCode() => Name.GetHashCode();

        public override Expression Differentiate(string variable)
        {
            return Name == variable ? new NumberExpression(1m) : new NumberExpression(0m);
        }

    }
}