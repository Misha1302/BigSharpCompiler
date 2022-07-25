using Lexer;

public struct Token
{
    public string Text;
    public Kind Kind;
    public object? Value;
    public Own Own;

    public Token(string text, Kind kind, Own own = Own.Default, object? value = null)
    {
        Text = text;
        Kind = kind;
        Own = own;
        Value = value;
    }

    public Token(char character, Kind kind, Own own = Own.Default, object? value = null)
    {
        Text = character.ToString();
        Kind = kind;
        Own = own;
        Value = value;
    }
}