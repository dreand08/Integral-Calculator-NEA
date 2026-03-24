using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Computer_Science_NEA.FunctionHandling.Formatting
{
    public static class ExpressionDisplayFormatter
    {
        public static string Format(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input ?? "";

            string s = input.Trim();

            s = RewriteExp(s);
            s = RewriteInsideBrackets(s);
            s = RewriteProductsAtTopLevel(s);
            s = ReplacePi(s);

            return s;
        }

        private static string ReplacePi(string input)
        {
            return input.Replace("3.14159265358979", "π");
        }

        private static string RewriteProductsAtTopLevel(string input)
        {
            var terms = SplitTopLevelByPlus(input);

            for (int i = 0; i < terms.Count; i++)
                terms[i] = RewriteSingleTerm(terms[i].Trim());

            return string.Join(" + ", terms.Where(t => !string.IsNullOrWhiteSpace(t)));
        }

        private static string RewriteSingleTerm(string term)
        {
            var factors = SplitTopLevel(term, '*')
                .Select(f => f.Trim())
                .Where(f => f.Length > 0)
                .ToList();

            if (factors.Count <= 1)
                return term.Trim();

            var numericFactors = new List<string>();
            var otherFactors = new List<string>();

            foreach (var f in factors)
            {
                if (decimal.TryParse(f, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
                    numericFactors.Add(f);
                else
                    otherFactors.Add(f);
            }

            decimal numericProduct = 1m;
            bool hasNumeric = false;

            foreach (var n in numericFactors)
            {
                if (decimal.TryParse(n, NumberStyles.Number, CultureInfo.InvariantCulture, out var val))
                {
                    numericProduct *= val;
                    hasNumeric = true;
                }
                else
                {
                    otherFactors.Add(n);
                }
            }

            var sb = new StringBuilder();

            if (hasNumeric)
            {
                if (numericProduct == -1m && otherFactors.Count > 0)
                {
                    sb.Append("-");
                }
                else if (numericProduct != 1m || otherFactors.Count == 0)
                {
                    sb.Append(numericProduct.ToString(CultureInfo.InvariantCulture));
                }
            }

            foreach (var factor in otherFactors)
            {
                string cleaned = factor.Trim();

                if (NeedsBrackets(cleaned))
                    cleaned = "(" + cleaned + ")";

                sb.Append(cleaned);
            }

            string result = sb.ToString();

            if (string.IsNullOrWhiteSpace(result))
                return "1";

            return result;
        }

        private static bool NeedsBrackets(string s)
        {
            int depth = 0;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '(') depth++;
                else if (c == ')') depth--;
                else if (c == '+' && depth == 0) return true;
            }
            return false;
        }

        private static List<string> SplitTopLevel(string input, char separator)
        {
            var parts = new List<string>();
            var sb = new StringBuilder();
            int depth = 0;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (c == '(') depth++;
                else if (c == ')') depth--;

                if (c == separator && depth == 0)
                {
                    parts.Add(sb.ToString());
                    sb.Clear();
                }
                else
                {
                    sb.Append(c);
                }
            }

            parts.Add(sb.ToString());
            return parts;
        }

        private static List<string> SplitTopLevelByPlus(string input)
        {
            var parts = new List<string>();
            var sb = new StringBuilder();
            int depth = 0;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (c == '(') depth++;
                else if (c == ')') depth--;

                if (c == '+' && depth == 0)
                {
                    parts.Add(sb.ToString());
                    sb.Clear();
                }
                else
                {
                    sb.Append(c);
                }
            }

            parts.Add(sb.ToString());
            return parts;
        }

        private static string RewriteExp(string input)
        {
            var sb = new StringBuilder();
            int i = 0;

            while (i < input.Length)
            {
                if (StartsWithExp(input, i))
                {
                    int openIndex = i + 3;
                    int closeIndex = FindMatchingCloseBracket(input, openIndex);

                    if (closeIndex != -1)
                    {
                        string inside = input.Substring(openIndex + 1, closeIndex - openIndex - 1);
                        inside = Format(inside);

                        if (CanUseWithoutBrackets(inside))
                            sb.Append("e^").Append(inside);
                        else
                            sb.Append("e^(").Append(inside).Append(")");

                        i = closeIndex + 1;
                        continue;
                    }
                }

                sb.Append(input[i]);
                i++;
            }

            return sb.ToString();
        }

        private static string RewriteInsideBrackets(string input)
        {
            var sb = new StringBuilder();
            int i = 0;

            while (i < input.Length)
            {
                if (input[i] == '(')
                {
                    int start = i;
                    int end = FindMatchingCloseBracket(input, i);

                    if (end != -1)
                    {
                        string inside = input.Substring(start + 1, end - start - 1);

                        // Recursively format inside
                        inside = Format(inside);

                        sb.Append("(").Append(inside).Append(")");
                        i = end + 1;
                        continue;
                    }
                }

                sb.Append(input[i]);
                i++;
            }

            return sb.ToString();
        }

        private static bool StartsWithExp(string s, int index)
        {
            return index + 4 <= s.Length &&
                   s[index] == 'e' &&
                   s[index + 1] == 'x' &&
                   s[index + 2] == 'p' &&
                   s[index + 3] == '(';
        }

        private static int FindMatchingCloseBracket(string s, int openIndex)
        {
            int depth = 0;

            for (int i = openIndex; i < s.Length; i++)
            {
                if (s[i] == '(') depth++;
                else if (s[i] == ')')
                {
                    depth--;
                    if (depth == 0)
                        return i;
                }
            }

            return -1;
        }

        private static bool CanUseWithoutBrackets(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return false;

            int depth = 0;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '(') depth++;
                else if (c == ')') depth--;
                else if ((c == '+' || c == ' ') && depth == 0)
                    return false;
            }

            return true;
        }
    }
}