using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Computer_Science_NEA.FunctionHandling.SymbolicMath
{
    //Expression for addition of nodes.
    public sealed class AddExpression : Expression
    {
        public IReadOnlyList<Expression> Terms { get; }

        private AddExpression(IReadOnlyList<Expression> terms)
        {
            Terms = terms;
        }

        public override int Precedence => 10;

        private static bool IsZero(Expression e) =>
            e is NumberExpression n && n.Value == 0m;

        private static bool IsNumber(Expression e) =>
            e is NumberExpression;

        private static decimal GetNumber(Expression e) =>
            e is NumberExpression n ? n.Value : 0m;
    }
}
