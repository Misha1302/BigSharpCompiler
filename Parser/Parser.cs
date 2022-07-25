using System.Text;
using Lexer;

namespace Parser;

public static class Parser
{
    public static List<Token> SplitIntoTokens(string code, bool optimized = true, bool checks = true,
        bool supplement = true, bool replace = true, bool offsetArray = true)
    {
        var tokens = Lexer.Lexer.GetTokens(code);

        tokens = ConnectUnknownTokens(tokens);
        if (optimized) tokens = OptimizeMethods(tokens);
        if (checks) tokens = CheckTokens(tokens);
        if (replace) tokens = ReplaceTokens(tokens);
        if (supplement) tokens = SupplementTokens(tokens);
        if (offsetArray) tokens = OffsetArray(tokens);
        if (offsetArray) tokens = FixSemicolon(tokens);
        return tokens;
    }

    private static List<Token> FixSemicolon(List<Token> tokens)
    {
        for (var i = 0; i < tokens.Count; i++)
            if (tokens[i].Kind == Kind.Method)
            {
                while (tokens[i].Kind != Kind.OpenBracket)
                    i++;
                var openBrackets = 0;
                for (; i < tokens.Count; i++)
                {
                    switch (tokens[i].Kind)
                    {
                        case Kind.OpenBracket:
                            openBrackets++;
                            break;
                        case Kind.CloseBracket:
                            openBrackets--;
                            break;
                    }

                    if (openBrackets == 0) break;
                }

                i++;
                if (tokens[i].Kind == Kind.Semicolon) tokens.RemoveAt(i);
            }

        return tokens;
    }

    private static List<Token> OffsetArray(List<Token> tokens)
    {
        for (var i = 1; i < tokens.Count - 1; i++)
            if (tokens[i - 1].Kind == Kind.OpenSquareBracket && tokens[i].Kind == Kind.Number &&
                tokens[i + 1].Kind == Kind.CloseSquareBracket)
            {
                tokens.Insert(i, new Token("Convert", Kind.Convert));
                tokens.Insert(i + 1, new Token(".", Kind.Comma));
                tokens.Insert(i + 2, new Token("ToInt32", Kind.ToInt));
                tokens.Insert(i + 3, new Token("(", Kind.OpenParentheses));

                tokens.Insert(i + 5, new Token(")", Kind.CloseParentheses));
                tokens.Insert(i + 6, new Token("-", Kind.MathOperand));
                tokens.Insert(i + 7, new Token("1", Kind.Int, value: 1));
            }

        return tokens;
    }

    private static List<Token> ReplaceTokens(List<Token> tokens)
    {
        tokens = ReplaceBreakAndContinueToGoto(tokens);
        return tokens;
    }

    private static List<Token> ReplaceBreakAndContinueToGoto(List<Token> tokens)
    {
        for (var i = 1; i < tokens.Count; i++)
            if (tokens[i - 1].Kind == Kind.Break || tokens[i - 1].Kind == Kind.Continue)
            {
                var whiteSpaceWas = false;
                if (tokens[i].Kind == Kind.Whitespace)
                {
                    whiteSpaceWas = true;
                    i++;
                }

                if (tokens[i].Kind == Kind.Unknown && tokens[i + 1].Kind == Kind.Semicolon)
                {
                    if (whiteSpaceWas) tokens[i - 2] = new Token("goto", Kind.Goto);
                    else tokens[i - 1] = new Token("goto", Kind.Goto);
                }
            }

        return tokens;
    }

    private static bool ContainsAny(this string haystack, params string[] needles)
    {
        return needles.Any(haystack.Contains);
    }

    private static List<Token> SupplementTokens(List<Token> tokens)
    {
        tokens = SupplementIf(tokens);
        tokens = SupplementWriteLine(tokens);
        tokens = SupplementVariables(tokens);
        tokens = SupplementMethodsAndVoids(tokens);
        tokens = SupplementStrings(tokens);
        tokens = SupplementOperators(tokens);
        return tokens;
    }


    private static List<Token> SupplementMethodsAndVoids(List<Token> tokens)
    {
        List<string> methods = new();
        for (var i = 2; i < tokens.Count; i++)
            if (tokens[i - 2].Kind is Kind.Dynamic or Kind.Void && tokens[i].Kind is Kind.Unknown or Kind.Variable &&
                tokens[i + 1].Kind == Kind.OpenParentheses)
            {
                tokens[i] = new Token(tokens[i].Text, Kind.Method);
                methods.Add(tokens[i].Text);
            }

        for (var i = 0; i < tokens.Count; i++)
            if (tokens[i].Kind == Kind.Unknown && tokens[i + 1].Kind == Kind.OpenParentheses)
                if (methods.Contains(tokens[i].Text))
                    tokens[i] = new Token(tokens[i].Text, Kind.Method);


        return tokens;
    }

    private static List<Token> SupplementOperators(List<Token> tokens)
    {
        for (var i = 2; i < tokens.Count; i++)
            if (tokens[i].Kind == Kind.In)
            {
                tokens.RemoveAt(i);
                var containsTokens = new List<Token>();

                var openParenthesesCount = 0;
                i -= 2;
                do
                {
                    containsTokens.Add(tokens[i]);
                    if (tokens[i].Kind == Kind.OpenParentheses) openParenthesesCount++;
                    if (tokens[i].Kind == Kind.CloseParentheses) openParenthesesCount--;
                    tokens.RemoveAt(i);
                    i--;
                } while (openParenthesesCount != 0);

                i += 3;

                var arrayTokensList = new List<Token>();
                var openBracketsCount = 0;
                var lastToken = new Token("", Kind.Whitespace);


                do
                {
                    arrayTokensList.Add(tokens[i]);
                    if (tokens[i].Kind == Kind.OpenBracket) openBracketsCount++;
                    if (tokens[i].Kind == Kind.CloseBracket) openBracketsCount--;
                    tokens.RemoveAt(i);
                } while (openBracketsCount != 0);

                lastToken = tokens[i];
                tokens.RemoveAt(i);


                i--;
                arrayTokensList.Reverse();
                
                foreach (var arrayToken in arrayTokensList) tokens.Insert(i, arrayToken);
                i += arrayTokensList.Count - 1;
                tokens.Insert(i+1, new Token(" ", Kind.Whitespace));
                tokens.Insert(i+2, new Token("is", Kind.Is));
                tokens.Insert(i+3, new Token(" ", Kind.Whitespace));
                tokens.Insert(i+4, new Token("Dictionary", Kind.Dictionary));
                tokens.Insert(i+5, new Token("<", Kind.OpenAngleBracket));
                tokens.Insert(i+6, new Token("dynamic", Kind.Dynamic));
                tokens.Insert(i+7, new Token(",", Kind.Comma));
                tokens.Insert(i+8, new Token("dynamic", Kind.Dynamic));
                tokens.Insert(i+9, new Token(">", Kind.OpenAngleBracket));
                tokens.Insert(i+10, new Token(" ", Kind.Whitespace));
                tokens.Insert(i+11, new Token("?", Kind.Question));
                tokens.Insert(i+12, new Token(" ", Kind.Whitespace));
                
                foreach (var arrayToken in arrayTokensList) tokens.Insert(i+13, arrayToken);
                i += arrayTokensList.Count - 1;
                
                tokens.Insert(i+14, new Token(".", Kind.Comma));
                tokens.Insert(i+15, new Token("ContainsKey", Kind.ContainsKey));
                tokens.Insert(i+16, new Token("(", Kind.OpenParentheses));
                
                foreach (var containsToken in containsTokens) tokens.Insert(i + 17, containsToken);
                i += containsTokens.Count - 1;
                
                tokens.Insert(i+18, new Token(")", Kind.CloseParentheses));
                tokens.Insert(i+19, new Token(" ", Kind.Whitespace));
                tokens.Insert(i+20, new Token(":", Kind.Colon));
                tokens.Insert(i+21, new Token(" ", Kind.Whitespace));

                i += 21;
                
                if (arrayTokensList.Count != 1)
                {
                    tokens.Insert(i, new Token("Enumerable", Kind.Enumerable));
                    tokens.Insert(i+1, new Token(".", Kind.Dot));
                    tokens.Insert(i+2, new Token("Contains", Kind.Contains));
                    tokens.Insert(i+3, new Token("(", Kind.OpenParentheses));

                    i += 3;

                    tokens.Insert(i + 1, new Token("new", Kind.New));
                    tokens.Insert(i + 2, new Token(" ", Kind.Whitespace));
                    tokens.Insert(i + 3, new Token("dynamic", Kind.Dynamic));
                    tokens.Insert(i + 4, new Token("[", Kind.OpenSquareBracket));
                    tokens.Insert(i + 5, new Token("]", Kind.CloseSquareBracket));
                    
                    foreach (var arrayToken in arrayTokensList) tokens.Insert(i + 6, arrayToken);
                    i += arrayTokensList.Count;
                    
                    tokens.Insert(i + 7, new Token(",", Kind.Comma));
                    tokens.Insert(i + 8, new Token("(", Kind.OpenParentheses));

                    foreach (var containsToken in containsTokens) tokens.Insert(i + 9, containsToken);
                    i += containsTokens.Count - 1;
                    
                    tokens.Insert(i + 10, new Token(")", Kind.CloseParentheses));
                    tokens.Insert(i + 11, new Token(")", Kind.CloseParentheses));
                    i += 12;
                }
                else
                {
                    tokens.Insert(i, new Token("Enumerable", Kind.Enumerable));
                    tokens.Insert(i + 1, new Token(".", Kind.Dot));
                    tokens.Insert(i + 2, new Token("Contains", Kind.Contains));
                    tokens.Insert(i + 3, new Token("(", Kind.OpenParentheses));

                    foreach (var arrayToken in arrayTokensList) tokens.Insert(i + 4, arrayToken);
                    i += arrayTokensList.Count - 1;

                    tokens.Insert(i + 5, new Token(",", Kind.Comma));

                    foreach (var containsToken in containsTokens) tokens.Insert(i + 6, containsToken);
                    i += containsTokens.Count - 1;

                    tokens.Insert(i + 7, new Token(")", Kind.CloseParentheses));
                    i += 8;
                }


                tokens.Insert(i, lastToken);
            }

        return tokens;
    }

    private static List<Token> SupplementStrings(List<Token> tokens)
    {
        for (var i = 0; i < tokens.Count; i++)
            if (tokens[i].Kind == Kind.String)
            {
                if (tokens[i - 1].Kind != Kind.StringInterpolation)
                {
                    tokens.Insert(i, new Token("FastString", Kind.FastString));
                    tokens.Insert(i, new Token(" ", Kind.Whitespace));
                    tokens.Insert(i, new Token("new", Kind.New));
                    tokens.Insert(i + 3, new Token('(', Kind.OpenParentheses));
                    tokens.Insert(i + 5, new Token(')', Kind.CloseParentheses));
                    i += 5;
                }
                else
                {
                    tokens.Insert(i - 1, new Token("FastString", Kind.FastString));
                    tokens.Insert(i - 1, new Token(" ", Kind.Whitespace));
                    tokens.Insert(i - 1, new Token("new", Kind.New));
                    tokens.Insert(i + 2, new Token('(', Kind.OpenParentheses));
                    tokens.Insert(i + 5, new Token(')', Kind.CloseParentheses));
                    i += 5;
                }
            }

        return tokens;
    }

    private static List<Token> SupplementVariables(List<Token> tokens)
    {
        List<string> variables = new();
        for (var i = 2; i < tokens.Count; i++)
            if (tokens[i - 2].Kind is Kind.Dynamic or Kind.Var &&
                tokens[i - 1].Kind == Kind.Whitespace && tokens[i].Kind == Kind.Unknown)
            {
                tokens[i] = new Token(tokens[i].Text, Kind.Variable);
                variables.Add(tokens[i].Text);
            }

        for (var i = 0; i < tokens.Count; i++)
            if (tokens[i].Kind == Kind.Unknown)
                if (variables.Contains(tokens[i].Text))
                    tokens[i] = new Token(tokens[i].Text, Kind.Variable);


        return tokens;
    }

    private static List<Token> SupplementWriteLine(List<Token> tokens)
    {
        for (var i = 2; i < tokens.Count; i++)
            if (tokens[i - 2].Kind != Kind.Console && tokens[i - 1].Kind == Kind.Dot && tokens[i].Kind == Kind.WriteLine)
            {
                tokens.Insert(i - 1, new Token('.', Kind.Comma));
                tokens.Insert(i, new Token("ToString", Kind.ToString));
                tokens.Insert(i + 1, new Token('(', Kind.OpenParentheses));
                tokens.Insert(i + 2, new Token(')', Kind.CloseParentheses));
                i += 6;
            }

        for (var i = 2; i < tokens.Count; i++)
            if (tokens[i - 2].Kind != Kind.Console && tokens[i - 1].Kind == Kind.Dot && tokens[i].Kind == Kind.WriteLine)
            {
                tokens.Insert(i - 2, new Token(')', Kind.CloseParentheses));
                i -= 4;
                var startIndex = i;
                while (i > 0)
                {
                    i--;
                    if (tokens[i].Kind is Kind.OpenBracket or Kind.OpenParentheses or Kind.OpenAngleBracket
                        or Kind.Semicolon) break;
                }

                // ( ( string )
                // ) string ( (
                tokens.Insert(i, new Token(')', Kind.CloseParentheses));
                tokens.Insert(i, new Token("string", Kind.ToString));
                tokens.Insert(i, new Token('(', Kind.OpenParentheses));
                tokens.Insert(i, new Token('(', Kind.OpenParentheses));
                i = startIndex + 15;
            }

        return tokens;
    }

    private static List<Token> SupplementIf(List<Token> tokens)
    {
        /*
        if(a==3) 
            Write(3)
        
        if(a==3){
            Write(3)
        }
         */
        for (var i = 0; i < tokens.Count; i++)
            if (tokens[i].Kind is Kind.If or Kind.Elif or Kind.Else)
            {
                var openParentheses = 0;
                i++;
                if (tokens[i].Kind == Kind.Whitespace) i++;
                for (; i < tokens.Count; i++)
                {
                    switch (tokens[i].Kind)
                    {
                        case Kind.OpenParentheses:
                            openParentheses++;
                            break;
                        case Kind.CloseParentheses:
                            openParentheses--;
                            break;
                    }

                    if (openParentheses == 0) break;
                }

                tokens.Insert(i + 1, new Token('{', Kind.OpenBracket));
                i++;
                for (; i < tokens.Count; i++)
                    if (tokens[i].Kind == Kind.Semicolon)
                        break;
                tokens.Insert(i + 1, new Token('}', Kind.CloseBracket));
            }

        return tokens;
    }

    private static List<Token> CheckTokens(List<Token> tokens)
    {
        tokens = CheckConstants(tokens);
        tokens = CheckGoto(tokens);
        return tokens;
    }

    private static List<Token> CheckGoto(List<Token> tokens)
    {
        if (tokens.Any(token => token.Kind == Kind.Goto)) throw new Exception("[Parser] Goto - evil. Don't use it");
        return tokens;
    }

    private static List<Token> CheckConstants(List<Token> tokens)
    {
        List<Token> constants = new();
        for (var i = 0; i < tokens.Count; i++)
        {
            if (tokens[i].Kind == Kind.SignEquals)
            {
                var constToken = tokens[i - 1].Kind switch
                {
                    Kind.Whitespace => tokens[i - 2],
                    Kind.MathOperand => tokens[i - 2].Kind == Kind.Whitespace ? tokens[i - 3] : tokens[i - 2],
                    _ => tokens[i - 1]
                };
                if (constants.Contains(constToken))
                    throw new Exception($"[Parser] Constant <{constToken.Text}> cannot be changed");
            }

            if (tokens[i].Kind == Kind.SignEquals)
            {
                var constToken = tokens[i - 1].Kind switch
                {
                    Kind.Whitespace => tokens[i - 2],
                    Kind.MathOperand => tokens[i - 2].Kind == Kind.Whitespace ? tokens[i - 3] : tokens[i - 2],
                    _ => tokens[i - 1]
                };

                i++;
                if (tokens[i].Kind == Kind.Whitespace) i++;

                if (tokens[i].Kind == Kind.Const && tokens[i + 1].Kind == Kind.OpenParentheses)
                {
                    tokens.RemoveAt(i);
                    constants.Add(constToken);
                }
            }
        }

        return tokens;
    }

    private static List<Token> ConnectUnknownTokens(List<Token> tokens)
    {
        for (var i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];
            if (token.Kind != Kind.Unknown) continue;
            if (i + 1 == tokens.Count) break;

            while (tokens[i + 1].Kind == Kind.Unknown)
            {
                token.Text += tokens[i + 1].Text;
                tokens.RemoveAt(i + 1);
                if (i + 1 == tokens.Count) break;
            }

            tokens[i] = token;
        }

        return tokens;
    }

    private static List<Token> OptimizeMethods(List<Token> tokens)
    {
        for (var i = 0; i < tokens.Count; i++)
            if (tokens[i].Kind == Kind.Optimized)
                if (tokens[i + 3].Kind == Kind.Dynamic)
                {
                    var variableName = $"MethodDict{DateTimeOffset.Now.ToUnixTimeMilliseconds()}";
                    tokens[i] = new Token($"var {variableName} = new Dictionary<string, dynamic>();",
                        Kind.ParserOptimized);
                    tokens.Insert(i + 2,
                        new Token(
                            "[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]",
                            Kind.ParserOptimized));
                    var openBrackets = 0;
                    if (i == 0) i = 1;

                    i++;
                    if (i >= tokens.Count) break;
                    var parameters = new List<string>();
                    while (tokens[i - 1].Kind != Kind.CloseBracket)
                    {
                        i++;
                        if (i == tokens.Count) break;
                        if (tokens[i].Kind == Kind.OpenParentheses)
                        {
                            if (tokens[i + 1].Kind != Kind.CloseParentheses)
                                while (tokens[i].Kind != Kind.CloseParentheses)
                                {
                                    i += 3;
                                    parameters.Add(tokens[i].Text);
                                    i++;
                                }

                            i++;
                            if (i == tokens.Count) break;
                        }

                        while (true)
                        {
                            switch (tokens[i].Kind)
                            {
                                case Kind.OpenBracket:
                                    openBrackets++;
                                    var a = parameters.Aggregate("", (current, parameter) => current + $" {parameter}");
                                    if (string.IsNullOrEmpty(a)) a = "\" \"";
                                    a = "$\"{" + a + "}\"";
                                    tokens.Insert(i + 1,
                                        new Token($"if({variableName}.ContainsKey({a})) return {variableName}[{a}];",
                                            Kind.Unknown));
                                    i++;
                                    break;
                                case Kind.CloseBracket:
                                    openBrackets--;
                                    break;
                            }

                            if (openBrackets <= 0) break;

                            if (tokens[i].Text == "return")
                            {
                                var b = new StringBuilder();
                                var j = i;
                                while (tokens[j + 1].Kind != Kind.Semicolon)
                                {
                                    b.Append(tokens[j + 1].Text);
                                    j++;
                                }

                                var a = parameters.Aggregate("", (current, parameter) => current + $" {parameter}");
                                if (string.IsNullOrEmpty(a)) a = "\" \"";
                                a = "$\"{" + a + "}\"";
                                if (Lexer.Lexer.IsNumber(b.ToString()))
                                    b = new StringBuilder($"BigFloat.ParseBigFloat(\"{b}\")");
                                tokens.Insert(i - 1,
                                    new Token(
                                        $"if(!{variableName}.ContainsKey({a})) {variableName}.Add({a}, {b.ToString()});",
                                        Kind.Unknown));
                                i++;
                            }

                            i++;
                        }
                    }
                }

        for (var i = 0; i < tokens.Count; i++)
            switch (tokens[i].Kind)
            {
                case Kind.Void:
                    tokens.Insert(i,
                        new Token(
                            "[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]",
                            Kind.ParserOptimized));
                    i += 4;
                    break;
                case Kind.Dynamic:
                    // ' '
                    // METHOD_NAME
                    if (tokens[i + 3].Kind == Kind.OpenParentheses)
                    {
                        tokens.Insert(i,
                            new Token(
                                "[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]",
                                Kind.ParserOptimized));
                        i += 4;
                    }

                    break;
            }

        return tokens;
    }

    public static void PrintAllTokens(List<Token> tokens)
    {
        foreach (var token in tokens)
        {
            const int totalWidth = 20;

            var text = ('[' + token.Text + ']').PadRight(totalWidth, ' ');
            var kind = ('[' + token.Kind.ToString() + ']').PadRight(totalWidth, ' ');
            var own = ('[' + token.Own.ToString() + ']').PadRight(totalWidth, ' ');

            var value = '[' + token.Value?.ToString();
            value += "]".PadRight(totalWidth, ' ');

            Console.WriteLine($"Text: {text} Kind: {kind} Own: {own} Value: {value}");
        }
    }
}