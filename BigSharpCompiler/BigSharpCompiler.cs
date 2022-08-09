using System.Text;
using DotnetController;
using Lexer;

namespace BigSharpCompiler;

public static class BigSharpCompiler
{
    public static void Main(string[] args)
    {
        Thread.CurrentThread.Priority = ThreadPriority.Highest;
        Start(args.Length == 0 ? "Settings.xml" : args[0]);

        // try
        // {
        //     Start(args.Length == 0 ? "Settings.xml" : args[0]);
        // }
        // catch(Exception e)
        // {
        //     Console.WriteLine($"StackTrace: {e.StackTrace}\n\n");
        //
        //     var trace = new StackTrace(e, true);
        //
        //     foreach (var frame in trace.GetFrames())
        //     {
        //         var sb = new StringBuilder();
        //
        //         sb.AppendLine($"Файл: {frame.GetFileName()}");
        //         sb.AppendLine($"Строка: {frame.GetFileLineNumber()}");
        //         sb.AppendLine($"Столбец: {frame.GetFileColumnNumber()}");
        //         sb.AppendLine($"Метод: {frame.GetMethod()}");
        //
        //         Console.WriteLine(sb);
        //     }
        // }
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
        code.Append("Thread.CurrentThread.Priority = ThreadPriority.Highest;");


        for (var i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];
            if (token.Own == Own.Command)
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
            else
                switch (token.Kind)
                {
                    case Kind.Number:
                        code.Append($"ParseBigFloat(\"{token.Text}\")");
                        break;
                    default:
                        //if (token.Kind == Kind.Unknown) throw new Exception($"Unknown token: {token.Text}");
                        code.Append(token.Text);
                        break;
                }
        }

        code.Append(File.ReadAllText(@"Code\BottomCode.txt"));


        return code.ToString();
    }
}