using Computer_Science_NEA.FunctionHandling.SymbolicMath;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Computer_Science_NEA.FunctionHandling.Parsing
{
    public static class ExpressionParser
    {
        public static Expression Parse(string input)
        {
            var tokens = Tokenizer.Tokenize(input);
            var p = new Parser(tokens);
            var expr = p.ParseExpression();

            if (p.Current.Type != TokenType.End)
                throw new ParseException($"Unexpected token '{p.Current.Text}'", p.Current.Pos);

            return expr.Simplify();
        }

        private sealed class Parser
        {
            private readonly List<Token> _toks;
            private int _idx;

            public Parser(List<Token> toks)
            {
                _toks = toks ?? throw new ArgumentNullException(nameof(toks));
                _idx = 0;
            }

            public Token Current => _toks[_idx];

            private Token Consume(TokenType type, string? friendlyMessage = null)
            {
                if (Current.Type != type)
                    throw new ParseException(
                        friendlyMessage ?? $"Expected {type} but found '{Current.Text}'",
                        Current.Pos
                    );

                return _toks[_idx++];
            }

            private bool Match(TokenType type)
            {
                if (Current.Type == type)
                {
                    _idx++;
                    return true;
                }
                return false;
            }

            // Expression -> Term ((+|-) Term)*
            public Expression ParseExpression()
            {
                var left = ParseTerm();

                while (true)
                {
                    if (Match(TokenType.Plus))
                    {
                        left = AddExpression.Make(left, ParseTerm()).Simplify();
                    }
                    else if (Match(TokenType.Minus))
                    {
                        // a - b => a + (-1*b)
                        var b = ParseTerm();
                        left = AddExpression.Make(left,
                            MultiplyExpression.Make(new NumberExpression(-1m), b)
                        ).Simplify();
                    }
                    else break;
                }

                return left;
            }

            // Term -> Factor ( (* Factor) | (implicit-mul Factor) )*
            private Expression ParseTerm()
            {
                var left = ParseFactor();

                while (true)
                {
                    if (Match(TokenType.Star))
                    {
                        left = MultiplyExpression.Make(left, ParseFactor()).Simplify();
                        continue;
                    }

                    // Implicit multiplication:
                    // If the next token can start a factor/primary, then multiplication is implied.
                    if (IsImplicitMulStart(Current))
                    {
                        left = MultiplyExpression.Make(left, ParseFactor()).Simplify();
                        continue;
                    }

                    break;
                }

                return left;
            }

            // Factor -> Unary ( ^ Factor )?   (right-associative)
            private Expression ParseFactor()
            {
                var left = ParseUnary();

                if (Match(TokenType.Caret))
                {
                    var right = ParseFactor(); // right associative: a^(b^c)
                    // Rewrite e^u as exp(u) to avoid floating approximations of e
                    if (IsEConstant(left))
                        return ExpExpression.Make(right).Simplify();
                    return PowerExpression.Make(left, right).Simplify();
                }

                return left;
            }
            private static bool IsEConstant(Expression e)
            {
                if (e is VariableExpression v && v.Name.Equals("e", StringComparison.OrdinalIgnoreCase))
                    return true;

                // If you currently parse e as a number, catch that too.
                if (e is NumberExpression n)
                {
                    // Rough check: close to Math.E (decimal approximation)
                    var dv = (double)n.Value;
                    return Math.Abs(dv - Math.E) < 1e-12;
                }

                return false;
            }

            // Unary -> (- Unary) | Primary
            private Expression ParseUnary()
            {
                if (Match(TokenType.Minus))
                {
                    // -x => (-1) * x
                    return MultiplyExpression.Make(new NumberExpression(-1m), ParseUnary()).Simplify();
                }

                return ParsePrimary();
            }

            // Primary -> Number | Identifier ( '(' Expression ')' )? | '(' Expression ')'
            private Expression ParsePrimary()
            {
                // Parentheses
                if (Current.Type == TokenType.LParen)
                {
                    var lower = name.ToLowerInvariant();
                    bool isKnownFunc = lower is "sin" or "cos" or "exp" or "ln";

                    if (isKnownFunc)
                    {
                        Consume(TokenType.LParen);
                        var arg = ParseExpression();
                        Consume(TokenType.RParen, "Missing ')' after function argument");
                        return MakeFunction(name, arg, idTok.Pos);
                    }
                }


                // Number
                if (Current.Type == TokenType.Number)
                {
                    var tok = Consume(TokenType.Number);
                    if (!decimal.TryParse(tok.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out var v))
                        throw new ParseException($"Invalid number '{tok.Text}'", tok.Pos);

                    return new NumberExpression(v);
                }

                // Identifier: variable or function name
                if (Current.Type == TokenType.Identifier)
                {
                    var idTok = Consume(TokenType.Identifier);
                    var name = idTok.Text;

                    // Function call: name(expr)
                    if (Match(TokenType.LParen))
                    {
                        var arg = ParseExpression();
                        Consume(TokenType.RParen, "Missing ')' after function argument");
                        return MakeFunction(name, arg, idTok.Pos);
                    }

                    // Constants

                    if (string.Equals(name, "pi", StringComparison.OrdinalIgnoreCase))
                        return new NumberExpression((decimal)Math.PI);

                    // Otherwise it's a variable
                    return new VariableExpression(name);
                }

                throw new ParseException($"Unexpected token '{Current.Text}'", Current.Pos);
            }

            private static bool IsImplicitMulStart(Token t) =>
                t.Type is TokenType.Number or TokenType.Identifier or TokenType.LParen;

            private static Expression MakeFunction(string name, Expression arg, int pos)
            {
                name = name.ToLowerInvariant();

                return name switch
                {
                    "sin" => SinExpression.Make(arg),
                    "cos" => CosExpression.Make(arg),
                    "exp" => ExpExpression.Make(arg),
                    "ln" => LnExpression.Make(arg),
                    _ => throw new ParseException($"Unknown function '{name}'", pos),
                };
            }
        }
    }
}
