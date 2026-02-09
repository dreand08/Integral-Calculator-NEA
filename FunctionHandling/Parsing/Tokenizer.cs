using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Computer_Science_NEA.FunctionHandling.Parsing
{
    public static class Tokenizer
    {
        public static List<Token> Tokenize(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            var tokens = new List<Token>();
            int i = 0;

            while (i < input.Length)
            {
                char c = input[i];

                // Skip whitespace
                if (char.IsWhiteSpace(c))
                {
                    i++;
                    continue;
                }

                int start = i;

                // Number: 12, 12.34, .5, 5.
                if (char.IsDigit(c) || c == '.')
                {
                    bool seenDot = (c == '.');
                    i++;

                    while (i < input.Length)
                    {
                        char d = input[i];

                        if (char.IsDigit(d))
                        {
                            i++;
                            continue;
                        }

                        if (d == '.' && !seenDot)
                        {
                            seenDot = true;
                            i++;
                            continue;
                        }

                        break;
                    }

                    // Reject "." by itself
                    var text = input[start..i];
                    if (text == ".")
                        throw new ParseException("Invalid number '.'", start);

                    tokens.Add(new Token(TokenType.Number, text, start));
                    continue;
                }

                // Identifier: x, sin, cos, exp, ln, etc.
                if (char.IsLetter(c) || c == '_')
                {
                    i++;
                    while (i < input.Length && (char.IsLetterOrDigit(input[i]) || input[i] == '_'))
                        i++;

                    tokens.Add(new Token(TokenType.Identifier, input[start..i], start));
                    continue;
                }

                // Single-character tokens
                i++;
                tokens.Add(c switch
                {
                    '+' => new Token(TokenType.Plus, "+", start),
                    '-' => new Token(TokenType.Minus, "-", start),
                    '*' => new Token(TokenType.Star, "*", start),
                    '^' => new Token(TokenType.Caret, "^", start),
                    '(' => new Token(TokenType.LParen, "(", start),
                    ')' => new Token(TokenType.RParen, ")", start),
                    _ => throw new ParseException($"Invalid character '{c}'", start),
                });
            }

            // End sentinel
            tokens.Add(new Token(TokenType.End, "", input.Length));
            return tokens;
        }
    }
}
