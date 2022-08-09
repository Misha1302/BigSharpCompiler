using System.Text.RegularExpressions;

namespace Preprocessor;

public static class Preprocessor
{
    public static string PreProcess(string code)
    {
        if (IsNullOrEmptyOrWhiteSpace(code)) return code;

        code = string.Join('\n', code.Split('\n').Where(x => !string.IsNullOrWhiteSpace(x) && !x.StartsWith("//")));

        code = code.Replace("\\\"", "＂");
        var splitCode = code.Split('"').ToList();
        for (var i = 0; i < splitCode.Count; i++)
            if (i % 2 == 0)
            {
                splitCode[i] = CleanCode(splitCode[i]);
            }
            else
            {
                splitCode[i] = Regex.Replace(splitCode[i], "\r\n", "\\n");

                if (!splitCode[i - 1].EndsWith('$')) continue;
                
                splitCode[i - 1] = splitCode[i - 1][..^1];

                var replaceOpenBracket = "{".GetHashCode() + Random.Shared.Next(100).ToString();
                var replaceCloseBracket = "}".GetHashCode() + Random.Shared.Next(100).ToString();

                splitCode[i] = Regex.Replace(splitCode[i], "\\{\\s{0,}\\}", "");
                splitCode[i] = Regex.Replace(splitCode[i], "\\\\{", replaceOpenBracket);
                splitCode[i] = Regex.Replace(splitCode[i], "\\\\}", replaceCloseBracket);


                var firstIndex = splitCode[i].IndexOf('{');
                var secondIndex = splitCode[i].IndexOf('}');
                while (secondIndex - firstIndex > 0)
                {
                    var e = CleanCode(splitCode[i].Substring(firstIndex + 1, secondIndex - firstIndex - 1));
                    if (!IsNullOrEmptyOrWhiteSpace(e))
                        splitCode[i] = splitCode[i][..(firstIndex + 1)][..^1] +
                                       $"\" + ({e}) + \"" +
                                       splitCode[i][secondIndex..][1..];

                    var position = firstIndex + e.Length + 8;

                    firstIndex = splitCode[i][position..].IndexOf('{') + position;
                    secondIndex = splitCode[i][position..].IndexOf('}') + position;
                }

                splitCode[i] = Regex.Replace(splitCode[i], replaceOpenBracket, "{");
                splitCode[i] = Regex.Replace(splitCode[i], replaceCloseBracket, "}");
            }

        code = string.Join('"', splitCode).Replace("＂", "\\\"") + ";";


        return code;
    }

    private static string CleanCode(string s)
    {
        var replaceNewString = ("\n".GetHashCode() + Random.Shared.Next(100)).ToString();

        s = Regex.Replace(s, "\\n", replaceNewString);
        s = Regex.Replace(s, "\\s+", " ");
        s = Regex.Replace(s, replaceNewString, "\r\n");

        s = Regex.Replace(s,
            "(?<!(((enum(.| |\r\n|\n)*\n\\{(.|\\n|\\r)*(?!(\\}))))|((if|elif|else)(| )\\((.)*\\))(\t| |)|\\,|\\{|\\(( |\\t)*))\r\n(?!(( |\\t)*(\\{|else|elif)))",
            ";\n", RegexOptions.Multiline);

        s = Regex.Replace(s, "(method|func)", "dynamic");
        s = Regex.Replace(s, "var(?!( [a-zA-Z0-9](| )=(| )const(| )\\())", "dynamic");
        s = Regex.Replace(s, "freeze(?!( [a-zA-Z0-9](| )=(| )const(| )\\())", "var");
        s = Regex.Replace(s, "throw Error", "throw new Exception");
        s = Regex.Replace(s, "=(| )list(| ){", " = new List<dynamic> {");
        s = Regex.Replace(s, "=(| )dict(| ){", " = new Dictionary<dynamic, dynamic> {");

        s = Regex.Replace(s, "\\*\\*", "^");

        s = Regex.Replace(s, "f(?!(.){1,})", "$");

        return s;
    }

    private static bool IsNullOrEmptyOrWhiteSpace(string? str)
    {
        return string.IsNullOrWhiteSpace(str) || str == "";
    }
}