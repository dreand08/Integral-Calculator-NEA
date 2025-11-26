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

        public static Expression Make(params Expression[] rawTerms)
        {
            var nonConst = new List<Expression>();
            decimal constSum = 0m;

            //1. Flatten and split constants from non constants
            foreach(var t in rawTerms)
            {
                if (t is AddExpression add)
                {
                    foreach (var sub in add.Terms)
                    {
                        if (IsNumber(sub)) constSum += GetNumber(sub);
                        else nonConst.Add(sub);
                    }
                }
                else if (IsNumber(t))
                {
                    constSum += GetNumber(t);
                }
                else
                {
                    nonConst.Add(t);
                }
            }

            //2. Drop +0
            var result = new List<Expression>();
            foreach (var t in nonConst)
            {
                if (!IsZero(t)) result.Add(t);
            }

            if (constSum != 0m || result.Count == 0)
            {
                result.Add(new NumberExpression(constSum));
            }

            //3. Sort for consistent printing.

            result = result.OrderBy(t => t.GetType().Name).ThenBy(t => t.ToString()).ToList();

            //4. if only 1 term, return it directly.
            if (result.Count == 1)
            {
                return result[0];
            }

            return new AddExpression(result);
        }

        public override Expression Simplify()
        {
            var simplified = Terms.Select(t => t.Simplify()).ToArray();
            return Make(simplified);
        }

        public override Expression Substitute(Expression target)
    }
}
