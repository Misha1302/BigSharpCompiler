using System.Text.RegularExpressions;

namespace Preprocessor;

public static class Preprocessor
{
    public static string PreProcess(string code)
    {
        if (IsNullOrEmptyOrWhiteSpace(code)) return code;

        code = string.Join('\n', code.Split('\n').Where(x => !string.IsNullOrWhiteSpace(x) && !x.StartsWith("//")));

        code = code.Replace("\\\"", "＂");
        var splitCode = code.Split('"');
        for (var i = 0; i < splitCode.Length; i++)
            if (i % 2 == 0)
            {
                var replaceNewString = "\n".GetHashCode().ToString();
                splitCode[i] = Regex.Replace(splitCode[i], "\\n", replaceNewString);
                splitCode[i] = Regex.Replace(splitCode[i], "\\s+", " ");
                splitCode[i] = Regex.Replace(splitCode[i], replaceNewString, "\r\n");

                splitCode[i] = Regex.Replace(splitCode[i],
                    "(?<!(((enum(.| |\r\n|\n)*\n\\{(.|\\n|\\r)*(?!(\\}))))|((if|elif|else)(| )\\((.)*\\))(\t| |)|\\,|\\{|\\(( |\\t)*))\r\n(?!(( |\\t)*(\\{|else|elif)))",
                    ";\n", RegexOptions.Multiline);

                splitCode[i] = Regex.Replace(splitCode[i], "(method|func)", "dynamic");
                splitCode[i] = Regex.Replace(splitCode[i], "var(?!( [a-zA-Z0-9](| )=(| )const(| )\\())", "dynamic");
                splitCode[i] = Regex.Replace(splitCode[i], "freeze(?!( [a-zA-Z0-9](| )=(| )const(| )\\())", "var");
                splitCode[i] = Regex.Replace(splitCode[i], "throw Error", "throw new Exception");
                splitCode[i] = Regex.Replace(splitCode[i], "=(| )list(| ){", " = new List<dynamic> {");
                splitCode[i] = Regex.Replace(splitCode[i], "=(| )dict(| ){", " = new Dictionary<dynamic, dynamic> {");

                splitCode[i] = Regex.Replace(splitCode[i], "\\*\\*", "^");
                
                splitCode[i] = Regex.Replace(splitCode[i], "f(?!(.){1,})", "$");
            }
            else
            {
                //splitCode[i] = Regex.Unescape(splitCode[i]);
                splitCode[i] = Regex.Replace(splitCode[i], "\r\n", "\\n");
            }

        code = string.Join('"', splitCode).Replace("＂", "\\\"") + ";";


        return code;
    }

    private static bool IsNullOrEmptyOrWhiteSpace(string? str)
    {
        return string.IsNullOrWhiteSpace(str) || str == "";
    }
}