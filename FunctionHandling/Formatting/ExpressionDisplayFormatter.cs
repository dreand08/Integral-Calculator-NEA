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
            // Handle empty/null input safely
            if (string.IsNullOrWhiteSpace(input))
                return input ?? "";

            string s = input.Trim();

            // Apply formatting steps in order
            s = RewriteExp(s);                 // exp(x) -> e^x
            s = RewriteInsideBrackets(s);      // Recursively format inside brackets
            s = RewriteProductsAtTopLevel(s);  // Remove * and reorder factors
            s = ReplacePi(s);                  // Replace decimal with π

            return s;
        }

        private static string ReplacePi(string input)
        {
            // Replace hardcoded decimal approximation of pi with symbol
            return input.Replace("3.14159265358979", "π");
        }

        private static string RewriteProductsAtTopLevel(string input)
        {
            // Split expression into top-level terms (separated by +)
            var terms = SplitTopLevelByPlus(input);

            // Format each term individually
            for (int i = 0; i < terms.Count; i++)
                terms[i] = RewriteSingleTerm(terms[i].Trim());

            // Join back together
            return string.Join(" + ", terms.Where(t => !string.IsNullOrWhiteSpace(t)));
        }

        private static string RewriteSingleTerm(string term)
        {
            // Split term into factors using * (only at top level)
            var factors = SplitTopLevel(term, '*')
                .Select(f => f.Trim())
                .Where(f => f.Length > 0)
                .ToList();

            // If no multiplication, return as-is
            if (factors.Count <= 1)
                return term.Trim();

            var numericFactors = new List<string>();
            var otherFactors = new List<string>();

            // Separate numbers from other expressions
            foreach (var f in factors)
            {
                if (decimal.TryParse(f, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
                    numericFactors.Add(f);
                else
                    otherFactors.Add(f);
            }

            decimal numericProduct = 1m;
            bool hasNumeric = false;

            // Multiply all numeric factors together
            foreach (var n in numericFactors)
            {
                if (decimal.TryParse(n, NumberStyles.Number, CultureInfo.InvariantCulture, out var val))
                {
                    numericProduct *= val;
                    hasNumeric = true;
                }
                else
                {
                    // Fallback (shouldn't usually happen)
                    otherFactors.Add(n);
                }
            }

            // Reorder factors so variables/polynomials come before functions
            otherFactors = otherFactors
                .OrderBy(FactorPriority)
                .ThenBy(f => f)
                .ToList();

            var sb = new StringBuilder();

            // Handle numeric part at the front
            if (hasNumeric)
            {
                if (numericProduct == -1m && otherFactors.Count > 0)
                {
                    // -1x -> -x (cleaner)
                    sb.Append("-");
                }
                else if (numericProduct != 1m || otherFactors.Count == 0)
                {
                    sb.Append(numericProduct.ToString(CultureInfo.InvariantCulture));
                }
            }

            // Append remaining factors (no * symbol)
            foreach (var factor in otherFactors)
            {
                string cleaned = factor.Trim();

                // Add brackets if needed for correct readability
                if (NeedsBrackets(cleaned))
                    cleaned = "(" + cleaned + ")";

                sb.Append(cleaned);
            }

            string result = sb.ToString();

            // If nothing remains, return 1
            if (string.IsNullOrWhiteSpace(result))
                return "1";

            return result;
        }

        private static int FactorPriority(string factor)
        {
            factor = factor.Trim();

            // Functions should appear last for better readability
            if (factor.StartsWith("sin(") ||
                factor.StartsWith("cos(") ||
                factor.StartsWith("ln(") ||
                factor.StartsWith("exp(") ||
                factor.StartsWith("e^"))
            {
                return 1;
            }

            // Variables/powers come first
            return 0;
        }

        private static bool NeedsBrackets(string s)
        {
            int depth = 0;

            // If a + appears at top level, brackets are needed
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

            // Split only when not inside brackets
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

            // Same logic as above but for +
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

            // Convert exp(x) -> e^x
            while (i < input.Length)
            {
                if (StartsWithExp(input, i))
                {
                    int openIndex = i + 3;
                    int closeIndex = FindMatchingCloseBracket(input, openIndex);

                    if (closeIndex != -1)
                    {
                        string inside = input.Substring(openIndex + 1, closeIndex - openIndex - 1);

                        // Recursively format inside
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

            // Recursively format contents of brackets
            while (i < input.Length)
            {
                if (input[i] == '(')
                {
                    int start = i;
                    int end = FindMatchingCloseBracket(input, i);

                    if (end != -1)
                    {
                        string inside = input.Substring(start + 1, end - start - 1);
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
            // Checks for "exp(" at current position
            return index + 4 <= s.Length &&
                   s[index] == 'e' &&
                   s[index + 1] == 'x' &&
                   s[index + 2] == 'p' &&
                   s[index + 3] == '(';
        }

        private static int FindMatchingCloseBracket(string s, int openIndex)
        {
            int depth = 0;

            // Finds corresponding closing bracket
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

            // If top-level + or space exists, brackets are required
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