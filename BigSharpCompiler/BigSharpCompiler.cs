using System.Text;
using DotnetController;
using Lexer;

namespace BigSharpCompiler;

public static class BigSharpCompiler
{
    public static void Main(string[] args)
    {
        Start(args.Length == 0 ? "Settings.xml" : args[0]);
    }

    private static void Start(string settingsPath)
    {
        var settingsDictionary = XmlReaderDictionary.XmlReaderDictionary.GetXmlElements(settingsPath);

        var bigSharpCode = File.ReadAllText(settingsDictionary["bigSharpCodePath"]);
        var saveCompiledCodePath = settingsDictionary["saveCompiledCodePath"];
        var autoStart = Convert.ToBoolean(settingsDictionary["autoStart"].ToLower());

        var tokens = Parser.Parser.SplitIntoTokens(bigSharpCode);

        var code = JoinToCode(tokens);

        if (autoStart)
        {
            DotnetBat.CreateWriteAndRunDotnet(saveCompiledCodePath, code);
        }
        else
        {
            DotnetBat.DotnetNewConsole(saveCompiledCodePath);
            DotnetBat.WriteCodeToDotnetProject(saveCompiledCodePath, code);
        }
    }

    private static string JoinToCode(IReadOnlyList<Token> tokens)
    {
        var code = new StringBuilder(65536);

        code.Append(File.ReadAllText(@"Code\UsingsCode.txt"));
        code.Append(File.ReadAllText(@"Code\TopCode.txt"));


        for (var i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];
            if (token.Own == Own.Command)
            {
                switch (token.Kind)
                {
                    case Kind.CSharp:
                        var openParentheses = 1;
                        i += 2;
                        do
                        {
                            code.Append(tokens[i].Text);
                            i++;
                            if (tokens[i].Kind == Kind.OpenParentheses) openParentheses++;
                            if (tokens[i].Kind == Kind.CloseParentheses) openParentheses--;
                        } while (openParentheses != 0);

                        break;
                    default:
                        if (token.Kind == Kind.Unknown) throw new Exception($"Unknown token: {token.Text}");
                        code.Append(token.Text);
                        break;
                }
            }
            else if (token.Kind == Kind.Number)
            {
                code.Append($"ParseBigFloat(\"{token.Text}\")");
            }
            else
            {
                if (token.Kind == Kind.Unknown) throw new Exception($"Unknown token: {token.Text}");
                code.Append(token.Text);
            }
        }

        code.Append(File.ReadAllText(@"Code\BottomCode.txt"));


        return code.ToString();
    }
}