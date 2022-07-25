using System.Text.RegularExpressions;

namespace BigFloatCalculator;

// use in compile project
public static class Calculator
{
    private static readonly IReadOnlyList<string> Operands = new[] { "^", "+", "-", "*", "/", "\\", "%" };

    public static BigFloat Calculate(string str)
    {
        str = str.Replace("_", "").Replace("**", "^").Replace(":", "^");
        str = Regex.Replace(str, "(\\}|\\{|\\(|\\)|\\[|\\])(?<=\\1\\1)", "");
        return Compute(Preparation(str));
    }

    private static int Priority(char x)
    {
        switch (x)
        {
            case '(':
            case ')':
                return -1;

            case '+':
            case '-':
                return 0;

            case '*':
            case '/':
                return 1;

            // case '↓':
            // case '↑':
            default:
                return 2;
        }
    }

    private static bool IsDigit(char digit)
    {
        return char.IsNumber(digit);
        //return decimal.TryParse(digit.ToString(), out _);
    }


    private static List<string?> Preparation(string equation)
    {
        var res = new List<string?>();
        var stack = new List<char>();

        var x = 0;

        var digit = "";

        while (x < equation.Length)
        {
            digit = string.Empty;
            while (x < equation.Length)
                if (IsDigit(equation[x]) || equation[x] == '.')
                {
                    digit += equation[x++];
                }
                else if (equation[x] == '-' && digit == string.Empty && IsDigit(equation[x + 1]))
                {
                    digit += equation[x];
                    x += 1;
                }
                else
                {
                    if (digit != "")
                    {
                        res.Add(digit);
                        digit = "";
                    }


                    if (equation[x] == ')')
                    {
                        while (stack.Count - 1 > 0 && stack[^1] != '(')
                        {
                            res.Add(stack[^1].ToString());
                            stack.RemoveAt(stack.Count - 1);
                        }

                        if (stack.Count - 1 > 0) stack.RemoveAt(stack.Count - 1);
                    }

                    else if (equation[x] == '(')
                    {
                        stack.Add(equation[x]);
                    }
                    else
                    {
                        if (equation[x] == '?') break;

                        var temp = Priority(equation[x]);
                        while (stack.Count != 0 && temp <= Priority(stack[^1]))
                        {
                            res.Add(stack[^1].ToString());
                            stack.RemoveAt(stack.Count - 1);
                        }

                        stack.Add(equation[x]);
                    }

                    break;
                }

            x += 1;
        }

        if (digit != "") res.Add(digit);

        for (var i = stack.Count - 1; i > -1; i--) res.Add(stack[i].ToString());

        return res;
    }

    private static BigFloat Compute(IList<string> res)
    {
        // foreach (var variable in res) Console.Write(variable + " ");
        // Console.WriteLine();

        BigFloat firstBigFloat;

        for (var x = 1; x < res.Count; x++)
            if (Operands.Contains(res[x]))
            {
                BigFloat secondBigFloat;
                try
                {
                    firstBigFloat = BigFloat.Parse(res[x - 1]);
                    secondBigFloat = BigFloat.Parse(res[x - 2]);
                }
                catch
                {
                    continue;
                }

                object sum = res[x] switch
                {
                    "+" => secondBigFloat + firstBigFloat,
                    "-" => secondBigFloat - firstBigFloat,
                    "*" => secondBigFloat * firstBigFloat,
                    "/" => secondBigFloat / firstBigFloat,
                    ":" => secondBigFloat / firstBigFloat,
                    "%" => GetPercent(secondBigFloat, firstBigFloat),
                    "\\" => secondBigFloat % firstBigFloat,
                    "^" => BigFloat.Pow(secondBigFloat, firstBigFloat),
                    _ => res[x - 2]
                };

                res[x - 2] = sum.ToString()!.Replace(',', '.');
                res.RemoveAt(x);
                res.RemoveAt(x - 1);
                x -= 2;
            }

        return BigFloat.ParseBigFloat(res[0]);
    }

    public static decimal ToRadians(decimal degrees)
    {
        return (decimal)Math.PI / 180 * degrees;
    }

    public static decimal ToDegrees(decimal radians)
    {
        return 180 / (decimal)Math.PI * radians;
    }

    private static BigFloat Factorial(BigFloat number)
    {
        if (number % 1 > 0.00000001m) throw new Exception("An attempt to find the factorial of a non-integer number");
        var res = number;
        for (var i = number - 1; i > 1; i--)
            res *= i;
        return res;
    }

    private static BigFloat GetPercent(BigFloat number, BigFloat percents)
    {
        return number * percents / 100;
    }
}