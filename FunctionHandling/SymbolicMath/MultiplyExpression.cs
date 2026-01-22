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

        private static bool TryGetNumericExponent(Expression factor, out Expression baseExpr, out decimal exp)
        {
            // x -> base x, exp 1
            if (factor is VariableExpression || factor is PowerExpression || factor is NumberExpression || factor is AddExpression || factor is MultiplyExpression)
            {
                // handled below
            }

            if (factor is PowerExpression p && p.Exponent is NumberExpression n)
            {
                baseExpr = p.BaseExpr;
                exp = n.Value;
                return true;
            }

            // Treat any non-power factor as exponent 1 (e.g. x == x^1)
            baseExpr = factor;
            exp = 1m;
            return true;
        }

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

            //x^a * x^b => x^(a+b) (only numberic)
            var combined = new List<Expression>();
            var exponentMap = new Dictionary<Expression, decimal>();

            foreach (var f in result)
            {
                if (f is NumberExpression)
                {
                    combined.Add(f);
                    continue;
                }

                if (TryGetNumericExponent(f, out var b, out var e))
                {
                    // Key point: uses Expression.Equals, so VariableExpression("x") matches itself,
                    // and structurally equal bases will combine if Equals is implemented for them.
                    if (exponentMap.ContainsKey(b)) exponentMap[b] += e;
                    else exponentMap[b] = e;
                }
                else
                {
                    combined.Add(f);
                }
            }

            // Rebuild combined power factors
            foreach (var kv in exponentMap)
            {
                var b = kv.Key;
                var e = kv.Value;

                if (e == 0m)
                {
                    // x^0 => 1
                    continue;
                }
                else if (e == 1m)
                {
                    combined.Add(b);
                }
                else
                {
                    combined.Add(PowerExpression.Make(b, new NumberExpression(e)));
                }
            }

            result = combined;


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

        public override Expression Integrate(string variable)
        {
            // If the whole thing is constant: C dx = Cx
            if (IsConstantWrt(variable))
                return MultiplyExpression.Make(this, new VariableExpression(variable)).Simplify();

            decimal constProduct = 1m;
            var nonConst = new List<Expression>();

            foreach (var f in Factors)
            {
                if (f is NumberExpression n)
                    constProduct *= n.Value;
                else
                    nonConst.Add(f);
            }

            // If there were no non-constants, it's just a constant
            if (nonConst.Count == 0)
                return MultiplyExpression.Make(new NumberExpression(constProduct), new VariableExpression(variable)).Simplify();

            // Only support pulling out constants times a sinle remaining expression for now
            if (nonConst.Count == 1)
            {
                var innerIntegral = nonConst[0].Integrate(variable);

                if (constProduct == 1m) return innerIntegral.Simplify();

                return MultiplyExpression.Make(new NumberExpression(constProduct), innerIntegral).Simplify();
            }

            throw new NotSupportedException("Integrate: product of multiple non-constant factors not supported yet.");
        }
    }
}
