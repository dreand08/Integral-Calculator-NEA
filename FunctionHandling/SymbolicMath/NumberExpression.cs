using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Computer_Science_NEA.FunctionHandling.SymbolicMath
{
    //This is a class for literal numbers. Integers and decimals
    public sealed class NumberExpression : Expression
    {
        //Properties
        public decimal Value { get; }
        
        //Constructor
        public NumberExpression(decimal value)
        {
            Value = value;
        }

        public override int Precedence => 90; //Strong Binding

        public override Expression Simplify() => this;

        public override Expression Substitute(Expression target, Expression replacement)
        {
            return Equals(target) ? replacement : this;
        }

        public override string ToString()
        {
            return Value % 1m == 0m ? ((int)Value).ToString() : Value.ToString();
        }

        public override bool Equals(object obj) =>
            obj is NumberExpression n && n.Value == Value;

        public override int GetHashCode() => Value.GetHashCode();

        public override Expression Differentiate(string variable)
        {
            return new NumberExpression(0m);
        }

        public override Expression Integrate(string variable)
        {
            return MultiplyExpression.Make(this, new VariableExpression(variable)).Simplify();
        }
    }
}
