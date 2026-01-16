using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Computer_Science_NEA.FunctionHandling.SymbolicMath
{
    //The purpose of this class is to flatten nested multiplications,
    //multiply constants together,
    //remove * 1,
    //and x * 0 -> 0.
    public sealed class MultiplyExpression : Expression
    {
        public IReadOnlyList<Expression> Factors { get; }

        private MultiplyExpression(IReadOnlyList<Expression> factors)
        {
            Factors = factors;
        }

        public override int Precedence => 20; //Higher precedence than add so multiplication takes priority.

        private static bool IsZero(Expression e) =>
            e is NumberExpression n && n.Value == 0m;

        private static bool IsOne(Expression e) =>
            e is NumberExpression n && n.Value == 1m;

        private static bool IsNumber(Expression e) =>
            e is NumberExpression;

        private static decimal GetNumber(Expression e) =>
            e is NumberExpression n ? n.Value : 1m;

        public static Expression Make(params Expression[] rawFactors)
        {
            var nonConst = new List<Expression>();
            decimal constProduct = 1m;

            foreach (var f in rawFactors) // Trying to flatten and split constants from non-constants.
            {
                if (f is MultiplyExpression mul)
                {
                    foreach (var sub in mul.Factors)
                    {
                        if (IsNumber(sub))
                        {
                            if (IsZero(sub)) return new NumberExpression(0m);
                            constProduct *= GetNumber(sub);
                        }
                        else
                        {
                            nonConst.Add(sub);
                        }
                    }
                }
                else if (IsNumber(f))
                {
                    if (IsZero(f)) return new NumberExpression(0m);
                    constProduct *= GetNumber(f);
                }
                else
                {
                    nonConst.Add(f);
                }
            }

            //Dropping the *1
            var result = new List<Expression>();
            foreach (var f in nonConst)
            {
                if (!IsOne(f)) result.Add(f);
            }

            // Add constant product back if needed
            if (constProduct != 1m || result.Count == 0)
            {
                result.Add(new NumberExpression(constProduct));
            }

            //Sort for consistent printing
            result = result
                .OrderBy(t => t.GetType().Name)
                .ThenBy(t => t.ToString())
                .ToList();

            //If only 1 factor then return it directly
            if (result.Count == 1)
                return result[0];

            return new MultiplyExpression(result);
        }

        public override Expression Simplify()
        {
            var simplified = Factors.Select(f => f.Simplify()).ToArray();
            return Make(simplified);
        }

        public override Expression Substitute(Expression target, Expression replacement)
        {
            if (Equals(target)) return replacement;

            var substituted = new List<Expression>(Factors.Count);
            foreach (var f in Factors)
                substituted.Add(f.Substitute(target, replacement));

            return Make(substituted.ToArray());
        }

        public override string ToString()
        {
            return string.Join(" * ", Factors.Select(f => WithParentsIfNeeded(f)));
        }

        public override Expression Differentiate(string variable)
        {
            // If there's only one factor, derivative is just that factor's derivative
            if (Factors.Count == 1)
                return Factors[0].Differentiate(variable);

            var sumTerms = new List<Expression>();

            for (int i = 0; i < Factors.Count; i++)
            {
                var di = Factors[i].Differentiate(variable);

                // If derivative of this factor is 0, skip this term
                if (di is NumberExpression n && n.Value == 0m)
                    continue;

                var termFactors = new List<Expression>(Factors.Count);
                for (int j = 0; j < Factors.Count; j++)
                {
                    termFactors.Add(j == i ? di : Factors[j]);
                }

                sumTerms.Add(MultiplyExpression.Make(termFactors.ToArray()));
            }

            // If all terms vanished, derivative is 0
            if (sumTerms.Count == 0)
                return new NumberExpression(0m);

            return AddExpression.Make(sumTerms.ToArray()).Simplify();
        }
    }
}
