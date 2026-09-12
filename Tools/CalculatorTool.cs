using System.Globalization;
using System.Text.Json;
using ToolCallerLab.Models;

namespace ToolCallerLab.Tools;

public sealed class CalculatorTool : ITool
{
    public string Name => "CalculatorTool";
    public string Description => "Evaluates basic arithmetic expressions using numbers, +, -, *, /, and parentheses. Do not calculate the result yourself; call this tool.";
    public BinaryData ParametersSchema => BinaryData.FromString("""
    {
      "type": "object",
      "properties": { "expression": { "type": "string", "description": "Arithmetic expression, such as (10 + 5) * 3." } },
      "required": ["expression"],
      "additionalProperties": false
    }
    """);

    public ToolResult Execute(BinaryData arguments)
    {
        try
        {
            var request = JsonSerializer.Deserialize<Arguments>(arguments.ToString(), JsonOptions)
                ?? throw new FormatException("Arguments were empty.");
            var value = new Parser(request.Expression).Parse();
            return ToolResult.Success(value.ToString("G", CultureInfo.InvariantCulture));
        }
        catch (Exception ex) when (ex is FormatException or DivideByZeroException)
        {
            return ToolResult.Failure(ex.Message);
        }
    }

    private sealed record Arguments(string Expression);
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    // A deliberately small recursive-descent parser: expression -> term -> factor.
    private sealed class Parser(string text)
    {
        private int position;
        public double Parse()
        {
            var result = ParseExpression();
            SkipWhitespace();
            if (position != text.Length) throw new FormatException($"Unexpected character '{text[position]}'.");
            return result;
        }
        private double ParseExpression()
        {
            var value = ParseTerm();
            while (true)
            {
                SkipWhitespace();
                if (TryConsume('+')) value += ParseTerm();
                else if (TryConsume('-')) value -= ParseTerm();
                else return value;
            }
        }
        private double ParseTerm()
        {
            var value = ParseFactor();
            while (true)
            {
                SkipWhitespace();
                if (TryConsume('*')) value *= ParseFactor();
                else if (TryConsume('/'))
                {
                    var divisor = ParseFactor();
                    if (divisor == 0) throw new DivideByZeroException("Division by zero is not allowed.");
                    value /= divisor;
                }
                else return value;
            }
        }
        private double ParseFactor()
        {
            SkipWhitespace();
            if (TryConsume('('))
            {
                var value = ParseExpression();
                if (!TryConsume(')')) throw new FormatException("Missing closing parenthesis.");
                return value;
            }
            var start = position;
            if (position < text.Length && (text[position] == '+' || text[position] == '-')) position++;
            while (position < text.Length && (char.IsDigit(text[position]) || text[position] == '.')) position++;
            if (start == position || !double.TryParse(text[start..position], NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
                throw new FormatException("Expected a number.");
            return number;
        }
        private bool TryConsume(char character) { SkipWhitespace(); if (position < text.Length && text[position] == character) { position++; return true; } return false; }
        private void SkipWhitespace() { while (position < text.Length && char.IsWhiteSpace(text[position])) position++; }
    }
}
