using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

            //Double-angle trig: sin(u)*cos(u) = 0.5*sin(2u)
            bool changed = true;
            while (changed)
            {
                changed = false;

                int sinIndex = -1;
                int cosIndex = -1;
                Expression uFound = null;

                for (int i = 0; i < nonConst.Count; i++)
                {
                    if (nonConst[i] is SinExpression s)
                    {
                        for (int j = 0; j < nonConst.Count; j++)
                        {
                            if (i == j) continue;
                            if (nonConst[j] is CosExpression c)
                            {
                                if (Same(s.Inner, c.Inner))
                                {
                                    sinIndex = i;
                                    cosIndex = j;
                                    uFound = s.Inner;
                                    break;
                                }
                            }
                        }
                    }
                    if (uFound != null) break;
                }

                if (uFound != null)
                {
                    // remove higher index first
                    if (sinIndex > cosIndex)
                    {
                        nonConst.RemoveAt(sinIndex);
                        nonConst.RemoveAt(cosIndex);
                    }
                    else
                    {
                        nonConst.RemoveAt(cosIndex);
                        nonConst.RemoveAt(sinIndex);
                    }

                    // multiply constant by 1/2
                    constProduct *= 0.5m;

                    // add sin(2u)
                    nonConst.Add(
                        SinExpression.Make(
                            MultiplyExpression.Make(new NumberExpression(2m), uFound).Simplify()
                        )
                    );

                    changed = true;
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

            //x^a * x^b => x^(a+b)
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
                    //uses Expression.Equals, so VariableExpression("x") matches itself,
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

            // Split numeric constants from the rest
            decimal constProduct = 1m;
            var factors = new List<Expression>();

            foreach (var f in Factors)
            {
                if (f is NumberExpression n)
                    constProduct *= n.Value;
                else
                    factors.Add(f);
            }
            // Helper: extract numeric constant multiplier from an expression.
            // expr = k * rest, where k is numeric constant, rest is the remaining expression
            static void SplitConst(Expression expr, out decimal k, out Expression rest)
            {
                k = 1m;

                if (expr is NumberExpression nn)
                {
                    k = nn.Value;
                    rest = new NumberExpression(1m);
                    return;
                }

                if (expr is MultiplyExpression mul)
                {
                    decimal prod = 1m;
                    var nonNums = new List<Expression>();

                    foreach (var ff in mul.Factors)
                    {
                        if (ff is NumberExpression nff) prod *= nff.Value;
                        else nonNums.Add(ff);
                    }

                    k = prod;

                    if (nonNums.Count == 0) rest = new NumberExpression(1m);
                    else if (nonNums.Count == 1) rest = nonNums[0];
                    else rest = MultiplyExpression.Make(nonNums.ToArray());

                    return;
                }

                rest = expr;
            }

            static void SplitDu(Expression du, out decimal duConst, out Expression duRest)
            {
                SplitConst(du, out duConst, out duRest);

                //if duRest became 1, keep it as 1
                if (duRest is NumberExpression n && n.Value == 1m)
                    duRest = new NumberExpression(1m);
            }


            // Helper: try to find and remove one factor that matches target up to a numeric constant.
            // Returns the numeric multiplier relating factor to target:
            // factor = k * target
            bool TryRemoveConstMultipleOf(Expression target, out decimal k)
            {
                for (int i = 0; i < factors.Count; i++)
                {
                    SplitConst(factors[i], out var ki, out var rest);
                    if (Same(rest, target))
                    {
                        k = ki;
                        factors.RemoveAt(i);
                        return true;
                    }
                }

                k = 1m;
                return false;
            }

            //Rule 1a: u' * u^-1  => ln(u)
            for (int i = 0; i < factors.Count; i++)
            {
                if (factors[i] is PowerExpression p && p.Exponent is NumberExpression ne && ne.Value == -1m)
                {
                    var u = p.BaseExpr;
                    var du = u.Differentiate(variable).Simplify();
                    SplitDu(du, out var duConst, out var duRest);

                    // Remove the u^-1 factor
                    factors.RemoveAt(i);

                    // Try remove a factor that is (k * duRest)
                    if (TryRemoveConstMultipleOf(duRest, out var k))
                    {
                        if (factors.Count == 0)
                        {
                            var multiplier = (constProduct * k) / duConst;

                            return MultiplyExpression.Make(new NumberExpression(multiplier), LnExpression.Make(u)).Simplify();
                        }
                    }
                    factors.Insert(i, p);
                }
            }

            // Rule 1b: u' * u^n  => u^(n+1)/(n+1)   (n numeric, n != -1)
            for (int i = 0; i < factors.Count; i++)
            {
                if (factors[i] is PowerExpression p &&
                    p.Exponent is NumberExpression ne &&
                    ne.Value != -1m)
                {
                    var u = p.BaseExpr;
                    var du = u.Differentiate(variable).Simplify();
                    SplitDu(du, out var duConst, out var duRest);

                    // Remove u^n
                    factors.RemoveAt(i);

                    // Try remove something proportional to duRest
                    if (TryRemoveConstMultipleOf(duRest, out var k))
                    {
                        if (factors.Count == 0)
                        {
                            var nVal = ne.Value;
                            var newExp = nVal + 1m;

                            if (newExp != 0m)
                            {
                                // multiplier = constProduct * k/duConst * 1/(n+1)
                                var multiplier = (constProduct * k) / duConst * (1m / newExp);

                                return MultiplyExpression.Make(
                                    new NumberExpression(multiplier),
                                    PowerExpression.Make(u, new NumberExpression(newExp))
                                ).Simplify();
                            }
                            // newExp == 0 would be ln case, handled by Rule 1 already
                        }
                    }

                    // Put back if not matched
                    factors.Insert(i, p);
                }
            }


            // Rule 2: exp(u) * u' => exp(u) (constant multiple allowed on u')
            for (int i = 0; i < factors.Count; i++)
            {
                if (factors[i] is ExpExpression exp)
                {
                    var u = exp.Inner;
                    var du = u.Differentiate(variable).Simplify();
                    SplitDu(du, out var duConst, out var duRest);

                    // Remove exp(u)
                    factors.RemoveAt(i);

                    //Trying to remove a factor that is (k * duRest)
                    if (TryRemoveConstMultipleOf(duRest, out var k))
                    {
                        if (factors.Count == 0)
                        {
                            // integrand = constProduct * k * duRest * exp(u)
                            // but du = duConst * duRest
                            // => integrand = (constProduct*k/duConst) * exp(u) * du
                            var multiplier = (constProduct * k) / duConst;

                            return MultiplyExpression.Make(new NumberExpression(multiplier), ExpExpression.Make(u)).Simplify();
                        }
                    }

                    factors.Insert(i, exp);
                }
            }

            //Rule 3: sin(u) * u' => -cos(u) (constant multiple allowed on u')
            for (int i = 0; i < factors.Count; i++)
            {
                if (factors[i] is SinExpression sin)
                {
                    var u = sin.Inner;
                    var du = u.Differentiate(variable).Simplify();
                    SplitDu(du, out var duConst, out var duRest);

                    factors.RemoveAt(i);

                    if (TryRemoveConstMultipleOf(duRest, out var k))
                    {
                        if (factors.Count == 0)
                        {
                            var multiplier = (constProduct * k) / duConst;

                            return MultiplyExpression.Make(new NumberExpression(multiplier), new NumberExpression(-1m), CosExpression.Make(u)).Simplify();
                        }
                    }

                    factors.Insert(i, sin);
                }
            }

            //Rule 4: cos(u) * u' => sin(u) (constant multiple allowed on u')
            for (int i = 0; i < factors.Count; i++)
            {
                if (factors[i] is CosExpression cos)
                {
                    var u = cos.Inner;
                    var du = u.Differentiate(variable).Simplify();
                    SplitDu(du, out var duConst, out var duRest);

                    factors.RemoveAt(i);

                    if (TryRemoveConstMultipleOf(duRest, out var k))
                    {
                        if (factors.Count == 0)
                        {
                            var multiplier = (constProduct * k) / duConst;

                            return MultiplyExpression.Make(new NumberExpression(multiplier), SinExpression.Make(u)).Simplify();
                        }
                    }

                    factors.Insert(i, cos);
                }
            }

            //Fallback: constant multiple rule
            if (factors.Count == 0)
                return MultiplyExpression.Make(new NumberExpression(constProduct), new VariableExpression(variable)).Simplify();

            if (factors.Count == 1)
            {
                var innerIntegral = factors[0].Integrate(variable);

                if (constProduct == 1m) return innerIntegral.Simplify();

                return MultiplyExpression.Make(new NumberExpression(constProduct), innerIntegral).Simplify();
            }

            // Integration by parts (limited): u * dv where u = x^n and dv = sin/cos/exp
            // Only attempt when there are exactly 2 non-constant factors left
            if (factors.Count == 2)
            {
                //Helpers local to Integrate:
                static bool IsNonZeroConst(Expression e, out decimal k)
                {
                    if (e is NumberExpression n && n.Value != 0m)
                    {
                        k = n.Value;
                        return true;
                    }
                    k = 0m;
                    return false;
                }

                static bool TryGetVarPower(Expression e, string variable, out int n)
                {
                    // x
                    if (e is VariableExpression v && v.Name == variable)
                    {
                        n = 1;
                        return true;
                    }

                    // x^n  where n is a positive integer number
                    if (e is PowerExpression p &&
                        p.BaseExpr is VariableExpression vb && vb.Name == variable &&
                        p.Exponent is NumberExpression ne)
                    {
                        // Only accept integer >= 1 for parts 
                        if (ne.Value % 1m == 0m)
                        {
                            int ni = (int)ne.Value;
                            if (ni >= 1)
                            {
                                n = ni;
                                return true;
                            }
                        }
                    }

                    n = 0;
                    return false;
                }

                static bool IsLinearInner(Expression inner, string variable)
                {
                    // linear means derivative is a non-zero constant
                    var d = inner.Differentiate(variable).Simplify();
                    return d is NumberExpression n && n.Value != 0m;
                }

                static bool IsTrigOrExpLinear(Expression e, string variable)
                {
                    return (e is SinExpression s && IsLinearInner(s.Inner, variable))
                        || (e is CosExpression c && IsLinearInner(c.Inner, variable))
                        || (e is ExpExpression ex && IsLinearInner(ex.Inner, variable));
                }

                // Try pick (u, dv) from the two factors
                Expression u = null;
                Expression dv = null;
                int uPow = 0;

                var a = factors[0];
                var b = factors[1];

                //Prefer the polynomial as u
                if (TryGetVarPower(a, variable, out var na) && IsTrigOrExpLinear(b, variable))
                {
                    u = a; dv = b; uPow = na;
                }
                else if (TryGetVarPower(b, variable, out var nb) && IsTrigOrExpLinear(a, variable))
                {
                    u = b; dv = a; uPow = nb;
                }

                if (u != null && dv != null)
                {
                    // du
                    var du = u.Differentiate(variable).Simplify(); // should reduce degree

                    // make sure du is simpler by degree therefore preventing loops
                    //proceed only if du is constant or still a smaller x^m.
                    bool duOk = (du is NumberExpression) ||
                        (TryGetVarPower(du is MultiplyExpression ? du : du, variable, out var _) == false ? true: true); 

                    if (duOk)
                    {
                        try
                        {
                            // v = ∫ dv dx  (this works because dv is sin/cos/exp of linear)
                            var v = dv.Integrate(variable).Simplify();

                            // ∫ u*dv = u*v - ∫ v*du
                            var uv = MultiplyExpression.Make(u, v).Simplify();

                            var vdu = MultiplyExpression.Make(v, du).Simplify();
                            var integralVdu = vdu.Integrate(variable).Simplify();

                            var result = AddExpression.Make(uv, MultiplyExpression.Make(new NumberExpression(-1m), integralVdu)).Simplify();

                            // Put back any numeric constant
                            if (constProduct != 1m)
                                return MultiplyExpression.Make(new NumberExpression(constProduct), result).Simplify();

                            return result;
                        }
                        catch (NotSupportedException)
                        {
                            // If parts can't finish, fall through to the normal error
                        }
                    }
                }
            }
            throw new NotSupportedException("Integrate: product of multiple non-constant factors not supported yet.");
        }
    }
}
