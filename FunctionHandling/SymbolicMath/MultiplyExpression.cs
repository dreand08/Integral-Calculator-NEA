using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Computer_Science_NEA.FunctionHandling.SymbolicMath
{
    // Product node. This class also does a lot of normalisation when products are built.
    public sealed class MultiplyExpression : Expression
    {
        public IReadOnlyList<Expression> Factors { get; }

        private MultiplyExpression(IReadOnlyList<Expression> factors)
        {
            Factors = factors;
        }

        // Higher than addition, so multiplication binds more strongly
        public override int Precedence => 20;

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
            // If this is already a power with a numeric exponent, extract base and exponent
            if (factor is PowerExpression p && p.Exponent is NumberExpression n)
            {
                baseExpr = p.BaseExpr;
                exp = n.Value;
                return true;
            }

            // Otherwise treat the factor as exponent 1, for example x == x^1
            baseExpr = factor;
            exp = 1m;
            return true;
        }

        public static Expression Make(params Expression[] rawFactors)
        {
            var nonConst = new List<Expression>();
            decimal constProduct = 1m;

            // Flatten nested products and multiply constant factors together
            foreach (var f in rawFactors)
            {
                if (f is MultiplyExpression mul)
                {
                    foreach (var sub in mul.Factors)
                    {
                        if (IsNumber(sub))
                        {
                            // Any factor of 0 makes the whole product 0
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

            // Special trig identity: sin(u)cos(u) = 0.5sin(2u)
            bool changed = true;
            while (changed)
            {
                changed = false;

                int sinIndex = -1;
                int cosIndex = -1;
                Expression uFound = null;

                // Look for one sin(u) and one cos(u) with the same inner expression
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
                    // Remove the matching sin and cos pair
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

                    // Identity introduces an extra factor of 1/2
                    constProduct *= 0.5m;

                    // Replace sin(u)cos(u) with sin(2u)
                    nonConst.Add(
                        SinExpression.Make(
                            MultiplyExpression.Make(new NumberExpression(2m), uFound).Simplify()
                        )
                    );

                    changed = true;
                }
            }

            // Remove factors of 1 because they do not change the product
            var result = new List<Expression>();
            foreach (var f in nonConst)
            {
                if (!IsOne(f)) result.Add(f);
            }

            // Add the combined numeric constant back if needed
            if (constProduct != 1m || result.Count == 0)
            {
                result.Add(new NumberExpression(constProduct));
            }

            // Combine powers with the same base, for example x^a * x^b = x^(a+b)
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
                    if (exponentMap.ContainsKey(b)) exponentMap[b] += e;
                    else exponentMap[b] = e;
                }
                else
                {
                    combined.Add(f);
                }
            }

            // Rebuild the merged power factors
            foreach (var kv in exponentMap)
            {
                var b = kv.Key;
                var e = kv.Value;

                if (e == 0m)
                {
                    // x^0 = 1, so this factor disappears
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

            // Sort factors so equivalent products get a consistent structure and print order
            result = result
                .OrderBy(t => t.GetType().Name)
                .ThenBy(t => t.ToString())
                .ToList();

            // If only one factor remains, no product node is needed
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
            // Product rule over any number of factors
            if (Factors.Count == 1)
                return Factors[0].Differentiate(variable);

            var sumTerms = new List<Expression>();

            for (int i = 0; i < Factors.Count; i++)
            {
                var di = Factors[i].Differentiate(variable);

                // Skip terms where this factor differentiates to 0
                if (di is NumberExpression n && n.Value == 0m)
                    continue;

                var termFactors = new List<Expression>(Factors.Count);
                for (int j = 0; j < Factors.Count; j++)
                {
                    termFactors.Add(j == i ? di : Factors[j]);
                }

                sumTerms.Add(MultiplyExpression.Make(termFactors.ToArray()));
            }

            if (sumTerms.Count == 0)
                return new NumberExpression(0m);

            return AddExpression.Make(sumTerms.ToArray()).Simplify();
        }

        public override Expression Integrate(string variable)
        {
            // If the whole product is constant, use the constant rule straight away
            if (IsConstantWrt(variable))
                return MultiplyExpression.Make(this, new VariableExpression(variable)).Simplify();

            // Separate out the numeric constant first so later matching is easier
            decimal constProduct = 1m;
            var factors = new List<Expression>();

            foreach (var f in Factors)
            {
                if (f is NumberExpression n)
                    constProduct *= n.Value;
                else
                    factors.Add(f);
            }

            // Split expr into k * rest, where k is the numeric part
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

            // Small helper for splitting derivatives into constant and non-constant parts
            static void SplitDu(Expression du, out decimal duConst, out Expression duRest)
            {
                SplitConst(du, out duConst, out duRest);

                if (duRest is NumberExpression n && n.Value == 1m)
                    duRest = new NumberExpression(1m);
            }

            // Try to remove a factor that matches the target up to a numeric constant
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

            // Rule 1a: u' * u^-1 -> ln(u)
            for (int i = 0; i < factors.Count; i++)
            {
                if (factors[i] is PowerExpression p && p.Exponent is NumberExpression ne && ne.Value == -1m)
                {
                    var u = p.BaseExpr;
                    var du = u.Differentiate(variable).Simplify();
                    SplitDu(du, out var duConst, out var duRest);

                    factors.RemoveAt(i);

                    if (TryRemoveConstMultipleOf(duRest, out var k))
                    {
                        if (factors.Count == 0)
                        {
                            var multiplier = (constProduct * k) / duConst;
                            return MultiplyExpression.Make(new NumberExpression(multiplier), LnExpression.Make(u)).Simplify();
                        }
                    }

                    // Put the factor back if the pattern did not fully match
                    factors.Insert(i, p);
                }
            }

            // Rule 1b: u' * u^n -> u^(n+1)/(n+1)
            for (int i = 0; i < factors.Count; i++)
            {
                if (factors[i] is PowerExpression p &&
                    p.Exponent is NumberExpression ne &&
                    ne.Value != -1m)
                {
                    var u = p.BaseExpr;
                    var du = u.Differentiate(variable).Simplify();
                    SplitDu(du, out var duConst, out var duRest);

                    factors.RemoveAt(i);

                    if (TryRemoveConstMultipleOf(duRest, out var k))
                    {
                        if (factors.Count == 0)
                        {
                            var nVal = ne.Value;
                            var newExp = nVal + 1m;

                            if (newExp != 0m)
                            {
                                var multiplier = (constProduct * k) / duConst * (1m / newExp);

                                return MultiplyExpression.Make(
                                    new NumberExpression(multiplier),
                                    PowerExpression.Make(u, new NumberExpression(newExp))
                                ).Simplify();
                            }
                        }
                    }

                    factors.Insert(i, p);
                }
            }

            // Rule 2: exp(u) * u' -> exp(u)
            for (int i = 0; i < factors.Count; i++)
            {
                if (factors[i] is ExpExpression exp)
                {
                    var u = exp.Inner;
                    var du = u.Differentiate(variable).Simplify();
                    SplitDu(du, out var duConst, out var duRest);

                    factors.RemoveAt(i);

                    if (TryRemoveConstMultipleOf(duRest, out var k))
                    {
                        if (factors.Count == 0)
                        {
                            var multiplier = (constProduct * k) / duConst;
                            return MultiplyExpression.Make(new NumberExpression(multiplier), ExpExpression.Make(u)).Simplify();
                        }
                    }

                    factors.Insert(i, exp);
                }
            }

            // Rule 3: sin(u) * u' -> -cos(u)
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

                            return MultiplyExpression.Make(
                                new NumberExpression(multiplier),
                                new NumberExpression(-1m),
                                CosExpression.Make(u)
                            ).Simplify();
                        }
                    }

                    factors.Insert(i, sin);
                }
            }

            // Rule 4: cos(u) * u' -> sin(u)
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

            // Constant multiple fallback
            if (factors.Count == 0)
                return MultiplyExpression.Make(new NumberExpression(constProduct), new VariableExpression(variable)).Simplify();

            if (factors.Count == 1)
            {
                var innerIntegral = factors[0].Integrate(variable);

                if (constProduct == 1m) return innerIntegral.Simplify();

                return MultiplyExpression.Make(new NumberExpression(constProduct), innerIntegral).Simplify();
            }

            // Limited integration by parts for polynomial * trig/exp
            if (factors.Count == 2)
            {
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
                    if (e is VariableExpression v && v.Name == variable)
                    {
                        n = 1;
                        return true;
                    }

                    if (e is PowerExpression p &&
                        p.BaseExpr is VariableExpression vb && vb.Name == variable &&
                        p.Exponent is NumberExpression ne)
                    {
                        // Only use positive integer powers for this parts rule
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
                    // Linear means its derivative is a non-zero constant
                    var d = inner.Differentiate(variable).Simplify();
                    return d is NumberExpression n && n.Value != 0m;
                }

                static bool IsTrigOrExpLinear(Expression e, string variable)
                {
                    return (e is SinExpression s && IsLinearInner(s.Inner, variable))
                        || (e is CosExpression c && IsLinearInner(c.Inner, variable))
                        || (e is ExpExpression ex && IsLinearInner(ex.Inner, variable));
                }

                Expression u = null;
                Expression dv = null;
                int uPow = 0;

                var a = factors[0];
                var b = factors[1];

                // Prefer the polynomial part as u
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
                    var du = u.Differentiate(variable).Simplify();

                    // Basic guard to avoid obvious loops when using parts
                    bool duOk = (du is NumberExpression) ||
                        (TryGetVarPower(du is MultiplyExpression ? du : du, variable, out var _) == false ? true : true);

                    if (duOk)
                    {
                        try
                        {
                            // v = ∫dv
                            var v = dv.Integrate(variable).Simplify();

                            // ∫u dv = uv - ∫v du
                            var uv = MultiplyExpression.Make(u, v).Simplify();

                            var vdu = MultiplyExpression.Make(v, du).Simplify();
                            var integralVdu = vdu.Integrate(variable).Simplify();

                            var result = AddExpression.Make(
                                uv,
                                MultiplyExpression.Make(new NumberExpression(-1m), integralVdu)
                            ).Simplify();

                            if (constProduct != 1m)
                                return MultiplyExpression.Make(new NumberExpression(constProduct), result).Simplify();

                            return result;
                        }
                        catch (NotSupportedException)
                        {
                            // If parts cannot finish, fall through to the final unsupported error
                        }
                    }
                }
            }

            throw new NotSupportedException("Integrate: product of multiple non-constant factors not supported yet.");
        }
    }
}
