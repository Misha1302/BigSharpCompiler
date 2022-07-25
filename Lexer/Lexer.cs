using System.Text;
using System.Text.RegularExpressions;

namespace Lexer;

public static class Lexer
{
    private static List<Token> _tokens = null!;
    private static string _codeText = null!;
    private static int _position;

    private static readonly IReadOnlyList<string> MathOperands = new[]
    {
        "**",
        "-",
        "+",
        "/",
        "*",
        "\\",
        "%",
        "!"
    };

    private static readonly IReadOnlyDictionary<string, Kind> Commands = new Dictionary<string, Kind>
    {
        { "WriteLine", Kind.WriteLine },
        { "Write", Kind.Write },
        { "CSharp", Kind.CSharp },
        { "Contains", Kind.Contains }
    };
    
    private static readonly IReadOnlyDictionary<string, Kind> Operators = new Dictionary<string, Kind>
    {
        { "in", Kind.In },
        { "is", Kind.Is },
        { "?", Kind.Question }
    };

    private static readonly IReadOnlyDictionary<string, Kind> Words = new Dictionary<string, Kind>
    {
        { "goto", Kind.Goto },
        { "break", Kind.Break },
        { "continue", Kind.Continue },
        { "new", Kind.New },
        { "const", Kind.Const },
        { "if", Kind.If },
        { "elif", Kind.Elif },
        { "else", Kind.Else },
        { "return", Kind.Return },
        { "enum", Kind.Enum }
    };

    private static readonly IReadOnlyDictionary<string, Kind> DataTypes = new Dictionary<string, Kind>
    {
        { "void", Kind.Void },
        { "dynamic", Kind.Dynamic },
        { "freeze", Kind.Freeze },
        { "Dictionary", Kind.Dictionary }
    };

    private static readonly IReadOnlyDictionary<string, Kind> Attributes = new Dictionary<string, Kind>
    {
        { "[Optimized]", Kind.Optimized }
    };

    private static readonly IReadOnlyDictionary<string, Kind> Classes = new Dictionary<string, Kind>
    {
        { "Console", Kind.Console }
    };

    private static readonly IReadOnlyDictionary<string, Kind> Prefixes = new Dictionary<string, Kind>
    {
        { "$", Kind.StringInterpolation }
    };

    public static List<Token> GetTokens(string codeText)
    {
        if (IsNullOrEmptyOrWhiteSpace(codeText)) return new List<Token>();

        SetDefaultVariables(codeText);

        // Get all tokens
        var token = GetNextToken();
        do
        {
            _tokens.Add(token);
            token = GetNextToken();
        } while (token.Kind != Kind.End);

        FixTokens(_tokens);

        return _tokens;
    }

    private static void FixTokens(IList<Token> tokens)
    {
        var openParenthesesExpressionCount = 0;
        var openParenthesesCount = 0;

        switch (tokens[0].Kind)
        {
            case Kind.OpenParentheses:
                openParenthesesCount++;
                break;
            case Kind.CloseParentheses:
                throw new Exception("[Lexer] The first parenthesis cannot be a closing parenthesis");
        }

        for (var i = 1; i < tokens.Count - 1; i++)
        {
            var token = tokens[i];
            if (tokens[i - 1].Kind != Kind.Var && token.Kind == Kind.Whitespace &&
                tokens[i + 1].Kind == Kind.OpenParentheses)
            {
                tokens.RemoveAt(i);
            }
            else if (tokens[i - 1].Own == Own.Expression &&
                     token.Kind is Kind.OpenParentheses or Kind.CloseParentheses &&
                     tokens[i + 1].Own == Own.Expression)
            {
                token.Own = Own.Expression;
                tokens[i] = token;
                if (token.Kind == Kind.OpenParentheses) openParenthesesExpressionCount++;
                else openParenthesesExpressionCount--;
                if (openParenthesesExpressionCount < 0)
                    throw new Exception("[Lexer] Any parenthesis is not closed");
            }

            if (openParenthesesExpressionCount == 1 && token.Kind == Kind.CloseParentheses)
            {
                openParenthesesExpressionCount--;
                token.Own = Own.Expression;
                tokens[i] = token;
            }

            switch (tokens[i].Kind)
            {
                case Kind.OpenParentheses:
                    openParenthesesCount++;
                    break;
                case Kind.CloseParentheses:
                    openParenthesesCount--;
                    break;
            }

            if (openParenthesesCount < 0) throw new Exception("[Lexer] Some parenthesis is not open but closed");
        }

        if (tokens[^1].Kind == Kind.OpenParentheses) openParenthesesCount++;
        if (tokens[^1].Kind == Kind.CloseParentheses) openParenthesesCount--;

        switch (openParenthesesCount)
        {
            case < 0: throw new Exception("[Lexer] Some parenthesis is not open");
            case > 0: throw new Exception("[Lexer] Some parenthesis is not closed");
        }
    }

    private static Token GetNextToken()
    {
        if (_position >= _codeText.Length) return new Token('\0', Kind.End);

        var currentChar = _codeText[_position];

        if (char.IsWhiteSpace(currentChar))
        {
            _position++;
            return new Token(currentChar, Kind.Whitespace);
        }

        switch (currentChar)
        {
            // ( - Parentheses
            // { - Bracket
            // [ - SquareBracket
            // < - AngleBracket
            
            case '(':
                _position++;
                return new Token(currentChar, Kind.OpenParentheses);
            case ')':
                _position++;
                return new Token(currentChar, Kind.CloseParentheses);
            case '{':
                _position++;
                return new Token(currentChar, Kind.OpenBracket);
            case '}':
                _position++;
                return new Token(currentChar, Kind.CloseBracket);
            case ',':
                _position++;
                return new Token(currentChar, Kind.Comma);
            case ';':
                _position++;
                return new Token(currentChar, Kind.Semicolon);
            case '=':
                _position++;
                return new Token(currentChar, Kind.SignEquals);
            case '<':
                _position++;
                return new Token(currentChar, Kind.OpenAngleBracket);
            case '>':
                _position++;
                return new Token(currentChar, Kind.CloseAngleBracket);
            case '.':
                _position++;
                return new Token(currentChar, Kind.Dot);
            case ':':
                _position++;
                return new Token(currentChar, Kind.Colon);
            case '\'':
                _position++;
                return new Token($"\'{_codeText[_position++]}\'", Kind.Char, value: _codeText[_position++]);
            case '"':
                _position++;
                var stringEnd = Regex.Match(_codeText[_position..], "(?<!(\\)))\"");
                if (!stringEnd.Success) throw new Exception("[Lexer] String has not end");
                var stringEndIndex = stringEnd.Index + 2;
                var text = _codeText.Substring(_position - 1, stringEndIndex);
                var value = _codeText.Substring(_position, stringEndIndex - 2);
                _position += text.Length - 1;
                return new Token(text, Kind.String, value: value);
        }


        if (IsDigit(currentChar))
        {
            var numberOfMinuses = 0;
            var numberOfDots = 0;
            var numberStringBuilder = new StringBuilder
            {
                Capacity = 16
            };

            string numberString;
            while (true)
            {
                if (_position >= _codeText.Length) break;
                currentChar = _codeText[_position];
                
                switch (currentChar)
                {
                    case '_':
                        _position++;
                        continue;
                    case '-' when numberStringBuilder.Length == 0:
                        numberOfMinuses++;
                        break;
                    case '-':
                        numberString = numberStringBuilder.ToString();
                        return new Token(numberString, Kind.Number, Own.Expression,
                            BigFloat.ParseBigFloat(numberString));
                }

                if (currentChar == '.')
                {
                    if (!IsDigit(_codeText[_position + 1])) break;
                    numberOfDots++;
                    if (numberOfDots > 1)
                    {
                        numberString = numberStringBuilder.ToString();
                        return new Token(numberString, Kind.Number, Own.Expression,
                            BigFloat.ParseBigFloat(numberString));
                    }

                    numberStringBuilder.Append(currentChar);
                    _position++;
                    continue;
                }

                if (numberOfMinuses > 1)
                {
                    if (!IsDigit(_codeText[_position + 1]))
                        throw new Exception("[Lexer] Number cannot have more than one sign");

                    _position++;
                    numberString = numberStringBuilder.ToString();
                    return new Token(numberString, Kind.Number, Own.Expression, BigFloat.ParseBigFloat(numberString));
                }

                if (!IsDigit(currentChar) && numberOfMinuses != 1) break;

                numberStringBuilder.Append(currentChar);
                _position++;
            }

            numberString = numberStringBuilder.ToString();
            return new Token(numberString, Kind.Number, Own.Expression, BigFloat.ParseBigFloat(numberString));
        }

        var textAfterPosition = _codeText[_position..];
        foreach (var mathOperand in MathOperands)
        {
            if (!textAfterPosition.StartsWith(mathOperand)) continue;

            _position += mathOperand.Length;
            return new Token(mathOperand, Kind.MathOperand, Own.Expression);
        }

        foreach (var commandPair in Commands)
        {
            if (!textAfterPosition.StartsWith(commandPair.Key)) continue;
            if (Regex.IsMatch(textAfterPosition[commandPair.Key.Length].ToString(), "[a-zA-Z]")) continue;

            _position += commandPair.Key.Length;
            return new Token(commandPair.Key, commandPair.Value, Own.Command);
        }

        foreach (var dataType in DataTypes)
        {
            if (!textAfterPosition.StartsWith(dataType.Key)) continue;
            if (Regex.IsMatch(textAfterPosition[dataType.Key.Length].ToString(), "[a-zA-Z]")) continue;

            _position += dataType.Key.Length;
            return new Token(dataType.Key, dataType.Value, Own.Command);
        }

        foreach (var attribute in Attributes)
        {
            if (!textAfterPosition.StartsWith(attribute.Key)) continue;
            if (Regex.IsMatch(textAfterPosition[attribute.Key.Length].ToString(), "[a-zA-Z]")) continue;

            _position += attribute.Key.Length;
            return new Token(attribute.Key, attribute.Value, Own.Command);
        }

        foreach (var wordPair in Words)
        {
            if (!textAfterPosition.StartsWith(wordPair.Key)) continue;
            if (Regex.IsMatch(textAfterPosition[wordPair.Key.Length].ToString(), "[a-zA-Z]")) continue;

            _position += wordPair.Key.Length;
            return new Token(wordPair.Key, wordPair.Value, Own.Word);
        }

        foreach (var @class in Classes)
        {
            if (!textAfterPosition.StartsWith(@class.Key)) continue;
            if (Regex.IsMatch(textAfterPosition[@class.Key.Length].ToString(), "[a-zA-Z]")) continue;

            _position += @class.Key.Length;
            return new Token(@class.Key, @class.Value, Own.Word);
        }

        foreach (var prefix in Prefixes)
        {
            if (!textAfterPosition.StartsWith(prefix.Key)) continue;
            if (Regex.IsMatch(textAfterPosition[prefix.Key.Length].ToString(), "[a-zA-Z]")) continue;

            _position += prefix.Key.Length;
            return new Token(prefix.Key, prefix.Value, Own.Word);
        }

        foreach (var @operator in Operators)
        {
            if (!textAfterPosition.StartsWith(@operator.Key)) continue;
            if (Regex.IsMatch(textAfterPosition[@operator.Key.Length].ToString(), "[a-zA-Z]")) continue;

            _position += @operator.Key.Length;
            return new Token(@operator.Key, @operator.Value, Own.Word);
        }

        switch (currentChar)
        {
            case '[':
                _position++;
                return new Token(currentChar, Kind.OpenSquareBracket);
            case ']':
                _position++;
                return new Token(currentChar, Kind.CloseSquareBracket);
            default:
                _position++;
                return new Token(currentChar, Kind.Unknown);
        }
    }

    private static bool IsNullOrEmptyOrWhiteSpace(string? str)
    {
        return string.IsNullOrWhiteSpace(str) || str == "";
    }

    private static bool IsDigit(char character)
    {
        return char.IsDigit(character);
    }

    private static void SetDefaultVariables(string codeText)
    {
        _position = 0;
        _tokens = new List<Token>(codeText.Length / 2);
        _codeText = Preprocessor.Preprocessor.PreProcess(codeText);
    }

    public static bool IsNumber(string number)
    {
        number = number.Replace(" ", "");
        var currentChar = number[0];
        var position = 0;
        if (IsDigit(currentChar) || currentChar == '-')
        {
            var numberOfDots = 0;

            while (true)
            {
                if (position >= number.Length) return true;
                currentChar = number[position];

                if (currentChar == '-')
                    if (position != 0)
                        return false;

                if (currentChar == '.')
                {
                    if (!IsDigit(number[position + 1])) return false;
                    numberOfDots++;
                    if (numberOfDots > 1) return false;

                    position++;
                    continue;
                }

                if (currentChar == '_')
                {
                    position++;
                    continue;
                }

                if (!IsDigit(currentChar)) return false;
                position++;
            }
        }

        return false;
    }
}