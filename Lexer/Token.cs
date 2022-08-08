using Lexer;

public class Token
{
    public string Text;
    public Kind Kind;
    public object? Value;
    public Dependence? Dependence;
    public Own Own;

    public Token(string text, Kind kind, Own own = Own.Default, dynamic? value = null, Dependence? dependence = null)
    {
        Text = text;
        Kind = kind;
        Dependence = dependence;
        Own = own;
        Value = value;
    }

    public Token(char character, Kind kind, Own own = Own.Default, object? value = null, Dependence? dependence = null)
    {
        Text = character.ToString();
        Kind = kind;
        Dependence = dependence;
        Own = own;
        Value = value;
    }
}