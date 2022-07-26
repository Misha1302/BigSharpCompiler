﻿using System.Globalization;
using System.Numerics;


// use in compile project
public struct BigFloat
{
    /// <summary>
    ///     The maximum Radix value for division operations.
    /// </summary>
    public static int DivMax
    {
        get => _divMax;
        set
        {
            _divMax = value;
            _sqrtMax = _divMax - 1;
            _expMax = _divMax - 1;
        }
    }

    private static int _divMax = 75;

    private static int _sqrtMax = _divMax - 1;
    private static int _expMax = _divMax - 1;

    private const int LOG_MAX = 10;
    private const int POS_INF = -1;
    private const int NEG_INF = -2;
    private const int NAN = -3;

    private readonly BigInteger _value;
    private int _radix;

    private BigFloat(BigInteger value, int radix)
    {
        _value = value;
        _radix = radix;
        while (_radix > 0)
        {
            if (_value % 10 != 0)
                break;
            _value /= 10;
            _radix--;
        }
    }

    public BigFloat(string s)
    {
        this = ParseBigFloat(s);
    }

    public BigFloat(params byte[] data)
    {
        _radix = BitConverter.ToInt32(data, 0);
        var d2 = new byte[data.Length - sizeof(int)];
        Array.Copy(data, sizeof(int), d2, 0, d2.Length);
        _value = new BigInteger(d2);
    }

    public BigFloat(double value)
    {
        _value = BigInteger.Zero;
        if (double.IsNaN(value))
        {
            _radix = NAN;
        }
        else if (double.IsNegativeInfinity(value))
        {
            _radix = NEG_INF;
        }
        else if (double.IsPositiveInfinity(value))
        {
            _radix = POS_INF;
        }
        else
        {
            var str = ToLongString(value);
            if (!str.Contains('.'))
            {
                _value = BigInteger.Parse(str);
                _radix = 0;
            }
            else
            {
                var split = str.Split('.');
                _value = BigInteger.Parse(split[0] + split[1]);
                _radix = split[1].Length;
            }
        }
    }

    public BigFloat(float value)
    {
        if (float.IsNaN(value))
        {
            _value = BigInteger.Zero;
            _radix = NAN;
        }
        else if (float.IsNegativeInfinity(value))
        {
            _value = BigInteger.Zero;
            _radix = NEG_INF;
        }
        else if (float.IsPositiveInfinity(value))
        {
            _value = BigInteger.Zero;
            _radix = POS_INF;
        }
        else
        {
            var str = ToLongString(value);
            if (!str.Contains('.'))
            {
                _value = BigInteger.Parse(str);
                _radix = 0;
            }
            else
            {
                var split = str.Split('.');
                _value = BigInteger.Parse(split[0] + split[1]);
                _radix = split[1].Length;
            }
        }
    }

    public BigFloat(decimal value)
    {
        _value = (BigInteger)value;
        _radix = 0;
        value -= decimal.Truncate(value);
        while (value != 0)
        {
            value *= 10;
            _value *= 10;
            _radix++;
            _value += (BigInteger)value;
            value -= decimal.Truncate(value);
        }
    }

    public BigFloat(BigInteger value) : this(value, 0)
    {
    }

    public BigFloat(int value) : this(value, 0)
    {
    }

    public BigFloat(uint value) : this(value, 0)
    {
    }

    public BigFloat(long value) : this(value, 0)
    {
    }

    public BigFloat(ulong value) : this(value, 0)
    {
    }

    public bool IsZero => _radix >= 0 && _value.IsZero;

    public bool IsPositiveInfinity => _radix == POS_INF;

    public bool IsNegativeInfinity => _radix == NEG_INF;

    public bool IsInfinity => IsPositiveInfinity || IsNegativeInfinity;

    public bool IsNaN => _radix == NAN;

    public int Sign
    {
        get
        {
            return _radix switch
            {
                NAN => 0,
                NEG_INF => -1,
                POS_INF => 1,
                _ => _value.Sign
            };
        }
    }

    public BigFloat Reciprocal => One / this;

    public static BigFloat Round(BigFloat value)
    {
        var valueStr = value.ToString(false).Replace('.', ',');
        var firstDigitInFractionalPart =
            valueStr.IndexOf(',') != -1 ? Convert.ToInt32(valueStr.Split(',')[1][0].ToString()) : 0;
        if (firstDigitInFractionalPart < 5) return Truncate(value);
        return value < 0 ? Truncate(value) - 1 : Truncate(value) + 1;
    }

    public override string? ToString()
    {
        return ToString();
    }

    public string ToString(bool round = true)
    {
        switch (_radix)
        {
            case NAN:
                return "NaN";
            case POS_INF:
                return "Infinity";
            case NEG_INF:
                return "-Infinity";
            case 0:
                return _value.ToString("R");
            default:
                var m = this < Zero;
                var str = BigInteger.Abs(_value).ToString("R").PadLeft(_radix + 1, '0');
                str = str.Insert(str.Length - _radix, ".");
                if (round) str = Round(ParseBigFloat(str), _divMax - 15).ToString(false);
                return m ? "-" + str : str;
        }
    }


    public static BigFloat Round(BigFloat value, int radix)
    {
        if (radix > -1)
        {
            var multiplier = ParseBigFloat('1' + new string('0', radix - 1));
            return Round(value * multiplier) / multiplier;
        }

        var divider = ParseBigFloat('1' + new string('0', -radix));
        return Round(value / divider) * divider;
    }


    public static BigFloat Truncate(BigFloat val)
    {
        if (val._radix <= 0)
            return val;
        return (BigInteger)val;
    }

    public static BigFloat Truncate(BigFloat val, int radix)
    {
        if (val._radix <= 0)
            return val;
        if (val._radix >= radix)
        {
            val._radix -= radix;
            return new BigFloat((BigInteger)val, radix);
        }

        return val;
    }

    public static BigFloat Floor(BigFloat val)
    {
        if (val._radix <= 0)
            return val;
        var s = val._value / BigInteger.Pow(10, val._radix - 1);
        if (s < 0 && s % 10 < 0)
            return s / 10 - 1;
        return s / 10;
    }

    public static BigFloat Floor(BigFloat val, int radix)
    {
        if (val._radix <= 0)
            return val;
        if (val._radix > radix)
        {
            var s = val._value / BigInteger.Pow(10, val._radix - radix - 1);
            if (s < 0 && s % 10 < 0)
                return new BigFloat(s / 10 - 1, radix);
            return new BigFloat(s / 10, radix);
        }

        return val;
    }

    public static BigFloat Ceiling(BigFloat val)
    {
        if (val._radix <= 0)
            return val;
        var s = val._value / BigInteger.Pow(10, val._radix - 1);
        if (s > 0 && s % 10 > 0)
            return s / 10 + 1;
        return s / 10;
    }

    public static BigFloat Ceiling(BigFloat val, int radix)
    {
        if (val._radix <= 0)
            return val;
        if (val._radix > radix)
        {
            var s = val._value / BigInteger.Pow(10, val._radix - radix - 1);
            if (s > 0 && s % 10 > 0)
                return new BigFloat(s / 10 + 1, radix);
            return new BigFloat(s / 10, radix);
        }

        return val;
    }

    public static BigFloat Exp(BigFloat val)
    {
        if (val.IsNegativeInfinity) return Zero;
        if (val._radix < 0) return val;

        BigFloat last = 0, iter = 1;
        BigInteger n = 1, fact = 1;
        var sq = val;
        while (Round(iter, _expMax) != Round(last, _expMax))
        {
            last = iter;
            iter += sq / (fact *= n);
            sq *= val;
            n++;
        }

        return Round(iter, _expMax);
    }

    private static BigFloat IntPow(BigFloat number, BigFloat degree)
    {
        var result = number;
        for (var i = 1; i < Abs(degree); i++) result *= number;
        if (degree < 0) result = 1 / result;
        return result;
    }

    public static BigFloat Pow(BigFloat number, BigFloat degree, bool round = true)
    {
        if (number.IsNaN || degree.IsNaN) return NaN;
        if (degree == 0) return 1;

        BigFloat result;
        if (degree % 1 == 0)
        {
            result = IntPow(number, degree);
        }
        else
        {
            if (number < 0) throw new Exception("You can't raise a negative number to a fractional power");
            degree = Round(degree, 2);
            var (numerator, denominator) = GetTheSmallestFraction(degree);
            result = GetRoot(number ^ numerator, denominator);
        }

        var returnNumber = degree < 0 && number < 0 ? -Abs(result) : result;
        return returnNumber;

        /*
        if (x.IsNaN || y.IsNaN) return NaN;
        if (y == 0) return 1;

        if (x._radix < 0 || y._radix < 0) return Math.Pow((double)x, (double)y);

        if (y._radix != 0) return round ? Round(Exp(y * StrangeLog(x)), _divMax - 3) : Exp(y * StrangeLog(x));

        if (x._radix == 0 && y >= 0 && y <= int.MaxValue)
            return BigInteger.Pow(x._value, (int)y._value);
        return round ? Round(PowBySquaring(x, y._value), _divMax - 3) : PowBySquaring(x, y._value);*/
    }

    public static (BigFloat numerator, BigFloat denominator) ReduceFraction(BigFloat numerator, BigFloat denominator)
    {
        if (numerator % 1 != 0) throw new Exception("non-integer values cannot be used in fractions (numerator)");
        if (denominator % 1 != 0) throw new Exception("non-integer values cannot be used in fractions (denominator)");

        var greatestCommonDivisor = GetGreatestCommonDivisor(numerator, denominator);
        while (greatestCommonDivisor != 1)
        {
            numerator /= greatestCommonDivisor;
            denominator /= greatestCommonDivisor;
            greatestCommonDivisor = GetGreatestCommonDivisor(numerator, denominator);
        }

        return (numerator, denominator);
    }

    public static (BigFloat numerator, BigFloat denominator) GetTheSmallestFraction(BigFloat number)
    {
        //TODO: Исправить!!!
        //number = Round(number, _divMax - 3);
        var zeroCount = new BigFloat('1' + new string('0', number.ToString().Split('.')[1].Length));
        return ReduceFraction(number * zeroCount, zeroCount);
    }

    public static BigFloat GetGreatestCommonDivisor(BigFloat a, BigFloat b)
    {
        while (b != 0)
        {
            var temp = b;
            b = a % b;
            a = temp;
        }

        return a;
    }


    // TODO: sin, cos, tan, sinh, cosh, tanh, asin, acos, atan, atan2, log

    public static BigFloat Sqrt(BigFloat val)
    {
        if (val.IsZero || val.IsPositiveInfinity || val.IsNaN)
            return val;
        if (val.Sign == -1) return NaN;

        var root = val / 2;
        var oRoot = val / root;
        while (Round(root, _sqrtMax) != Round(oRoot, _sqrtMax))
        {
            root = (root + oRoot) / 2;
            oRoot = val / root;
        }

        return root;
    }

    public static BigFloat Abs(BigFloat val)
    {
        switch (val._radix)
        {
            case NAN:
                return NaN;
            case NEG_INF:
            case POS_INF:
                return PositiveInfinity;
            default:
                return new BigFloat(BigInteger.Abs(val._value), val._radix);
        }
    }

    private static BigFloat StrangeLog(BigFloat val)
    {
        if (val.Sign != 1) return NaN;
        if (val._radix < 0) return val;

        if (val == 10) return Ln10;

        bool neg;
        if (val < 1)
        {
            // log(1-x)
            neg = true;
            val = 1 - val;
        }
        else if (val < 2)
        {
            // log(1+x)
            neg = false;
            val -= 1;
        }
        else if (val < 4)
        {
            return StrangeLog(Sqrt(val)) * 2;
        }
        else if (val < 10)
        {
            return StrangeLog(Sqrt(Sqrt(val))) * 4;
        }
        else
        {
            var deltaRadix = 0;
            while (val > 10)
            {
                val._radix++;
                deltaRadix++;
            }

            return StrangeLog(val) + deltaRadix * Ln10;
        }

        BigFloat last = 1, iter = 0;
        BigInteger n = 1;
        var sq = val;
        while (Round(iter, LOG_MAX) != Round(last, LOG_MAX))
        {
            last = iter;
            if (n.IsEven || neg)
                iter -= sq / n;
            else
                iter += sq / n;
            sq *= val;
            n++;
        }

        return Round(iter, LOG_MAX);
    }

    public static BigFloat Min(BigFloat a, BigFloat b)
    {
        return a > b ? b : a;
    }

    public static BigFloat Max(BigFloat a, BigFloat b)
    {
        return a > b ? a : b;
    }

    public static BigFloat Log10(BigFloat val)
    {
        return StrangeLog(val) / Ln10;
    }

    public static BigFloat operator +(BigFloat val)
    {
        return val;
    }

    public static BigFloat operator -(BigFloat val)
    {
        return val._radix switch
        {
            NAN => NaN,
            POS_INF => NegativeInfinity,
            NEG_INF => PositiveInfinity,
            _ => new BigFloat(-val._value, val._radix)
        };
    }

    public static BigFloat operator ~(BigFloat val)
    {
        return val._radix < 0 ? val : new BigFloat(~val._value, val._radix);
    }

    public static BigFloat operator ++(BigFloat val)
    {
        return val + 1;
    }

    public static BigFloat operator --(BigFloat val)
    {
        return val - 1;
    }

    public static BigFloat operator +(BigFloat a, BigFloat b)
    {
        if (a.IsNaN || b.IsNaN)
            return NaN;
        if ((a.IsPositiveInfinity && b.IsNegativeInfinity) || (a.IsNegativeInfinity && b.IsPositiveInfinity))
            return NaN;
        if (a.IsPositiveInfinity || b.IsPositiveInfinity)
            return PositiveInfinity;
        if (a.IsNegativeInfinity || b.IsNegativeInfinity)
            return NegativeInfinity;
        var valA = a._value;
        var valB = b._value;
        int radix;
        if (a._radix == b._radix)
        {
            radix = a._radix;
        }
        else if (a._radix > b._radix)
        {
            radix = a._radix;
            valB *= BigInteger.Pow(10, a._radix - b._radix);
        }
        else
        {
            radix = b._radix;
            valA *= BigInteger.Pow(10, b._radix - a._radix);
        }

        var returnValue = new BigFloat(valA + valB, radix);
        return Round(returnValue, _divMax - 3);
    }

    public static BigFloat operator -(BigFloat a, BigFloat b)
    {
        var returnValue = a + -b;
        return Round(returnValue, _divMax - 3);
    }

    public static BigFloat operator *(BigFloat a, BigFloat b)
    {
        if (a.IsNaN || b.IsNaN)
            return NaN;
        if (a.IsInfinity || b.IsInfinity)
            switch (a.Sign * b.Sign)
            {
                case 1:
                    return PositiveInfinity;
                case -1:
                    return NegativeInfinity;
                case 0:
                    return NaN;
            }

        var returnValue = new BigFloat(a._value * b._value, a._radix + b._radix);
        return returnValue;
    }

    public static BigFloat operator /(BigFloat a, BigFloat b)
    {
        if (a.IsNaN || b.IsNaN)
            return NaN;
        if (a.IsInfinity && b.IsInfinity)
            return NaN;
        if (b.IsZero)
            switch (a.Sign)
            {
                case 1:
                    return PositiveInfinity;
                case -1:
                    return NegativeInfinity;
                case 0:
                    return NaN;
            }

        if (a.IsInfinity)
            switch (a.Sign * b.Sign)
            {
                case 1:
                    return PositiveInfinity;
                case -1:
                    return NegativeInfinity;
            }

        if (b.IsInfinity)
            return Zero;
        var valA = a._value;
        var valB = b._value;
        if (a._radix > b._radix)
            valB *= BigInteger.Pow(10, a._radix - b._radix);
        else
            valA *= BigInteger.Pow(10, b._radix - a._radix);
        var result = BigInteger.Zero;
        var radix = 0;
        while (true)
        {
            result += BigInteger.DivRem(valA, valB, out valA);
            if (valA.IsZero)
                break;
            if (radix > _divMax)
                break;
            radix++;
            result *= 10;
            valA *= 10;
        }

        var returnValue = new BigFloat(result, radix);
        return returnValue;
    }

    public static BigFloat operator %(BigFloat a, BigFloat b)
    {
        if (a.IsNaN || b.IsNaN || a.IsInfinity || b.IsZero)
            return NaN;
        if (b.IsInfinity)
            return a;
        var valA = a._value;
        var valB = b._value;
        int radix;
        if (a._radix == b._radix)
        {
            radix = a._radix;
        }
        else if (a._radix > b._radix)
        {
            radix = a._radix;
            valB *= BigInteger.Pow(10, a._radix - b._radix);
        }
        else
        {
            radix = b._radix;
            valA *= BigInteger.Pow(10, b._radix - a._radix);
        }

        var returnValue = new BigFloat(valA % valB, radix);
        return returnValue;
    }

    public static BigFloat operator &(BigFloat a, BigFloat b)
    {
        if (a._radix < 0 || b._radix < 0)
            return NaN;
        var valA = a._value;
        var valB = b._value;
        int radix;
        if (a._radix == b._radix)
        {
            radix = a._radix;
        }
        else if (a._radix > b._radix)
        {
            radix = a._radix;
            valB *= BigInteger.Pow(10, a._radix - b._radix);
        }
        else
        {
            radix = b._radix;
            valA *= BigInteger.Pow(10, b._radix - a._radix);
        }

        return new BigFloat(valA & valB, radix);
    }

    public static BigFloat operator |(BigFloat a, BigFloat b)
    {
        if (a._radix < 0 || b._radix < 0)
            return NaN;
        var valA = a._value;
        var valB = b._value;
        int radix;
        if (a._radix == b._radix)
        {
            radix = a._radix;
        }
        else if (a._radix > b._radix)
        {
            radix = a._radix;
            valB *= BigInteger.Pow(10, a._radix - b._radix);
        }
        else
        {
            radix = b._radix;
            valA *= BigInteger.Pow(10, b._radix - a._radix);
        }

        return new BigFloat(valA | valB, radix);
    }

    public static BigFloat operator ^(BigFloat a, BigFloat b)
    {
        return Pow(a, b);
        /*if (a.Radix < 0 || b.Radix < 0)
            return NaN;
        BigInteger valA = a.Value;
        BigInteger valB = b.Value;
        int radix;
        if (a.Radix == b.Radix)
            radix = a.Radix;
        else if (a.Radix > b.Radix)
        {
            radix = a.Radix;
            valB *= BigInteger.Pow(10, a.Radix - b.Radix);
        }
        else
        {
            radix = b.Radix;
            valA *= BigInteger.Pow(10, b.Radix - a.Radix);
        }
        return new BigFloat(valA ^ valB, radix);*/
    }


    /// <summary>
    ///     Very resource intensive operation. Looking for the root of a number with the N-th degree
    /// </summary>
    /// <param name="number">root extraction number</param>
    /// <param name="degree">degree</param>
    /// <returns></returns>
    public static BigFloat GetRoot(BigFloat number, BigFloat degree)
    {
        if (number < 0 && degree % 2 == 0) throw new Exception("There is no even root of a negative number");

        if (degree % 1 == 0)
        {
            var value = degree > 0 ? GetNaturalRoot(number, degree) : 1 / GetNaturalRoot(number, Abs(degree));
            return value;
        }


        var (numerator, denominator) = GetTheSmallestFraction(degree);

        var result = Pow(number, denominator / numerator, false);

        return result;
    }

    private static BigFloat GetNaturalRoot(BigFloat number, BigFloat degree)
    {
        BigFloat left, right, middle;

        if (number > 0)
        {
            right = number + 1;
            left = 0;
        }
        else
        {
            right = 0;
            left = number - 1;
        }

        var lastMiddle = (left + right) / 2 + 1;
        while (true)
        {
            middle = (left + right) / 2;
            var result = Pow(middle, degree, false);

            if (middle == lastMiddle) break;
            lastMiddle = middle;

            if (result > number) right = middle;
            else if (result < number) left = middle;
        }

        return middle;
    }

    public static bool operator ==(BigFloat a, BigFloat b)
    {
        if (a.IsNaN || b.IsNaN)
            return false;
        if (a._radix < 0 || b._radix < 0)
            return a._radix == b._radix;
        return a._radix == b._radix && a._value == b._value;
    }

    public static bool operator !=(BigFloat a, BigFloat b)
    {
        if (a.IsNaN || b.IsNaN)
            return true;
        if (a._radix < 0 || b._radix < 0)
            return a._radix != b._radix;
        return a._radix != b._radix || a._value != b._value;
    }

    public static bool operator <(BigFloat a, BigFloat b)
    {
        if (a.IsNaN || b.IsNaN)
            return false;
        if (a.IsPositiveInfinity)
            return !b.IsPositiveInfinity;
        if (a.IsNegativeInfinity)
            return false;
        var valA = a._value;
        var valB = b._value;
        if (a._radix > b._radix)
            valB *= BigInteger.Pow(10, a._radix - b._radix);
        else if (a._radix < b._radix)
            valA *= BigInteger.Pow(10, b._radix - a._radix);
        return valA < valB;
    }

    public static bool operator >(BigFloat a, BigFloat b)
    {
        if (a.IsNaN || b.IsNaN)
            return false;
        if (a.IsPositiveInfinity)
            return false;
        if (a.IsNegativeInfinity)
            return !b.IsNegativeInfinity;
        var valA = a._value;
        var valB = b._value;
        if (a._radix > b._radix)
            valB *= BigInteger.Pow(10, a._radix - b._radix);
        else if (a._radix < b._radix)
            valA *= BigInteger.Pow(10, b._radix - a._radix);
        return valA > valB;
    }

    public static bool operator <=(BigFloat a, BigFloat b)
    {
        return a == b || a < b;
    }

    public static bool operator >=(BigFloat a, BigFloat b)
    {
        return a == b || a > b;
    }

    public static implicit operator BigFloat(byte value)
    {
        return new BigFloat(value);
    }

    public static implicit operator BigFloat(sbyte value)
    {
        return new BigFloat(value);
    }

    public static implicit operator BigFloat(short value)
    {
        return new BigFloat(value);
    }

    public static implicit operator BigFloat(ushort value)
    {
        return new BigFloat(value);
    }

    public static implicit operator BigFloat(int value)
    {
        return new BigFloat(value);
    }

    public static implicit operator BigFloat(uint value)
    {
        return new BigFloat(value);
    }

    public static implicit operator BigFloat(long value)
    {
        return new BigFloat(value);
    }

    public static implicit operator BigFloat(ulong value)
    {
        return new BigFloat(value);
    }

    public static implicit operator BigFloat(float value)
    {
        return new BigFloat(value);
    }

    public static implicit operator BigFloat(double value)
    {
        return new BigFloat(value);
    }

    public static implicit operator BigFloat(decimal value)
    {
        return new BigFloat(value);
    }

    public static implicit operator BigFloat(BigInteger value)
    {
        return new BigFloat(value);
    }

    public static explicit operator byte(BigFloat value)
    {
        return value._radix switch
        {
            NAN => 0,
            POS_INF => byte.MaxValue,
            NEG_INF => byte.MinValue,
            _ => (byte)(value._value / BigInteger.Pow(10, value._radix))
        };
    }

    public static explicit operator sbyte(BigFloat value)
    {
        return value._radix switch
        {
            NAN => 0,
            POS_INF => sbyte.MaxValue,
            NEG_INF => sbyte.MinValue,
            _ => (sbyte)(value._value / BigInteger.Pow(10, value._radix))
        };
    }

    public static explicit operator short(BigFloat value)
    {
        return value._radix switch
        {
            NAN => 0,
            POS_INF => short.MaxValue,
            NEG_INF => short.MinValue,
            _ => (short)(value._value / BigInteger.Pow(10, value._radix))
        };
    }

    public static explicit operator ushort(BigFloat value)
    {
        return value._radix switch
        {
            NAN => 0,
            POS_INF => ushort.MaxValue,
            NEG_INF => ushort.MinValue,
            _ => (ushort)(value._value / BigInteger.Pow(10, value._radix))
        };
    }

    public static explicit operator int(BigFloat value)
    {
        return value._radix switch
        {
            NAN => 0,
            POS_INF => int.MaxValue,
            NEG_INF => int.MinValue,
            _ => (int)(value._value / BigInteger.Pow(10, value._radix))
        };
    }

    public static explicit operator uint(BigFloat value)
    {
        return value._radix switch
        {
            NAN => 0,
            POS_INF => uint.MaxValue,
            NEG_INF => uint.MinValue,
            _ => (uint)(value._value / BigInteger.Pow(10, value._radix))
        };
    }

    public static explicit operator long(BigFloat value)
    {
        return value._radix switch
        {
            NAN => 0,
            POS_INF => long.MaxValue,
            NEG_INF => long.MinValue,
            _ => (long)(value._value / BigInteger.Pow(10, value._radix))
        };
    }

    public static explicit operator ulong(BigFloat value)
    {
        return value._radix switch
        {
            NAN => 0,
            POS_INF => ulong.MaxValue,
            NEG_INF => ulong.MinValue,
            _ => (ulong)(value._value / BigInteger.Pow(10, value._radix))
        };
    }

    public static explicit operator float(BigFloat value)
    {
        switch (value._radix)
        {
            case NAN:
                return float.NaN;
            case POS_INF:
                return float.PositiveInfinity;
            case NEG_INF:
                return float.NegativeInfinity;
            default:
                var res = BigInteger.DivRem(value._value, BigInteger.Pow(10, value._radix), out var rem);
                return (float)res + (float)rem / (float)Math.Pow(10, value._radix);
        }
    }

    public static explicit operator double(BigFloat value)
    {
        switch (value._radix)
        {
            case NAN:
                return double.NaN;
            case POS_INF:
                return double.PositiveInfinity;
            case NEG_INF:
                return double.NegativeInfinity;
            default:
                var res = BigInteger.DivRem(value._value, BigInteger.Pow(10, value._radix), out var rem);
                return (double)res + (double)rem / Math.Pow(10, value._radix);
        }
    }

    public static explicit operator decimal(BigFloat value)
    {
        switch (value._radix)
        {
            case NAN:
                return decimal.Zero;
            case POS_INF:
                return decimal.MaxValue;
            case NEG_INF:
                return decimal.MinValue;
            default:
                var res = BigInteger.DivRem(value._value, BigInteger.Pow(10, value._radix), out var rem);
                return (decimal)res + (decimal)rem / (decimal)Math.Pow(10, value._radix);
        }
    }

    public static explicit operator BigInteger(BigFloat value)
    {
        return value._radix switch
        {
            NAN => BigInteger.Zero,
            POS_INF => BigInteger.One,
            NEG_INF => BigInteger.MinusOne,
            _ => value._value / BigInteger.Pow(10, value._radix)
        };
    }

    public static readonly BigFloat Zero = new();

    public static readonly BigFloat One = new(1);

    public static readonly BigFloat PositiveInfinity = new(0, POS_INF);

    public static readonly BigFloat NegativeInfinity = new(0, NEG_INF);

    public static readonly BigFloat NaN = new(0, NAN);

    public static readonly BigFloat Ln10 = new(0x64, 0x00, 0x00, 0x00, 0x88, 0x51, 0x81, 0x0A, 0xA0, 0x03,
        0xDD, 0x3D, 0xF0, 0xAD, 0x42, 0x3C, 0x70, 0xDD, 0x55, 0xFC, 0x52, 0xFB, 0xEB, 0xA3, 0x3A, 0x04, 0x25, 0x34,
        0x2A, 0x19, 0x13, 0x65, 0x02, 0x2A, 0x8C, 0xA5, 0xD8, 0xA8, 0x00, 0xC7, 0xF1, 0x94, 0x4B, 0xF5, 0x1B, 0x2A);

    public static BigFloat Parse(string s)
    {
        if (s.Equals("nan", StringComparison.OrdinalIgnoreCase))
            return NaN;
        if (s.Equals("infinity", StringComparison.OrdinalIgnoreCase))
            return PositiveInfinity;
        if (s.Equals("-infinity", StringComparison.OrdinalIgnoreCase))
            return NegativeInfinity;
        s = ProcessScientificString(s);
        if (!s.Contains('.'))
            return BigInteger.Parse(s);
        var split = s.Split('.');
        return new BigFloat(BigInteger.Parse(split[0] + split[1]), split[1].Length);
    }

    public byte[] ToByteArray()
    {
        var d2 = _value.ToByteArray();
        var data = new byte[d2.Length + sizeof(int)];
        BitConverter.GetBytes(_radix).CopyTo(data, 0);
        d2.CopyTo(data, sizeof(int));
        return data;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not BigFloat) return false;
        return this == (BigFloat)obj;
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode() ^ _radix.GetHashCode();
    }

    private static string ProcessScientificString(string str)
    {
        if (!str.Contains('E') & !str.Contains('e'))
            return str;
        str = str.ToUpper();
        const char decSeparator = '.';
        var exponentParts = str.Split('E');
        var decimalParts = exponentParts[0].Split(decSeparator);
        if (decimalParts.Length == 1)
            decimalParts = new[]
            {
                exponentParts[0],
                "0"
            };
        var exponentValue = int.Parse(exponentParts[1]);
        var newNumber = decimalParts[0] + decimalParts[1];
        string? result;
        if (exponentValue > 0)
        {
            result = newNumber + GetZeros(exponentValue - decimalParts[1].Length);
        }
        else
        {
            result = string.Empty;
            if (newNumber.StartsWith("-"))
            {
                result = "-";
                newNumber = newNumber[1..];
            }

            result += "0" + decSeparator + GetZeros(exponentValue + decimalParts[0].Length) + newNumber;
            result = result.TrimEnd('0');
        }

        return result;
    }

    private static string ToLongString(double input)
    {
        return ProcessScientificString(input.ToString("R", NumberFormatInfo.InvariantInfo));
    }

    private static string ToLongString(float input)
    {
        return ProcessScientificString(input.ToString("R", NumberFormatInfo.InvariantInfo));
    }

    private static string GetZeros(int zeroCount)
    {
        return new string('0', Math.Abs(zeroCount));
    }

    public static BigFloat ParseBigFloat(string p0)
    {
        p0 = p0.Replace(',', '.');
        return Parse(p0);
    }
}