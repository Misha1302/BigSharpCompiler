using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;


// use in compile project
[Serializable]
public readonly struct BigFloat : IComparable, IComparable<BigFloat>, IEquatable<BigFloat>, IConvertible
{
    public readonly BigInteger Numerator;
    public readonly BigInteger Denominator;

    public static BigFloat One => new(BigInteger.One);

    public static BigFloat Zero => new(BigInteger.Zero);

    public static BigFloat MinusOne => new(BigInteger.MinusOne);

    public static BigFloat OneHalf => new(BigInteger.One, 2);

    public int Sign
    {
        get
        {
            var bigInteger = Numerator;
            var sign1 = bigInteger.Sign;
            bigInteger = Denominator;
            var sign2 = bigInteger.Sign;
            switch (sign1 + sign2)
            {
                case -2:
                case 2:
                    return 1;
                case 0:
                    return -1;
                default:
                    return 0;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private BigFloat(string value)
    {
        var bigFloat = Parse(value);
        Numerator = bigFloat.Numerator;
        Denominator = bigFloat.Denominator;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public BigFloat(BigInteger numerator, BigInteger denominator)
    {
        Numerator = numerator;
        Denominator = !(denominator == 0L) ? denominator : throw new ArgumentException("denominator equals 0");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public BigFloat(BigInteger value)
    {
        Numerator = value;
        Denominator = BigInteger.One;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public BigFloat(BigFloat value)
    {
        if (object.Equals(value, null))
        {
            Numerator = BigInteger.Zero;
            Denominator = BigInteger.One;
        }
        else
        {
            Numerator = value.Numerator;
            Denominator = value.Denominator;
        }
    }

    public BigFloat(ulong value)
        : this(new BigInteger(value))
    {
    }

    public BigFloat(long value)
        : this(new BigInteger(value))
    {
    }

    public BigFloat(uint value)
        : this(new BigInteger(value))
    {
    }

    public BigFloat(int value)
        : this(new BigInteger(value))
    {
    }

    public BigFloat(float value)
        : this(value.ToString("N99"))
    {
    }

    public BigFloat(double value)
        : this(value.ToString("N99"))
    {
    }

    public BigFloat(decimal value)
        : this(value.ToString("N99"))
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat Add(BigFloat value, BigFloat other)
    {
        if (object.Equals(other, null))
            throw new ArgumentNullException(nameof(other));
        return new BigFloat(value.Numerator * other.Denominator + other.Numerator * value.Denominator,
            value.Denominator * other.Denominator);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat Subtract(BigFloat value, BigFloat other)
    {
        if (object.Equals(other, null))
            throw new ArgumentNullException(nameof(other));
        return new BigFloat(value.Numerator * other.Denominator - other.Numerator * value.Denominator,
            value.Denominator * other.Denominator);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat Multiply(BigFloat value, BigFloat other)
    {
        if (object.Equals(other, null))
            throw new ArgumentNullException(nameof(other));
        var result = new BigFloat(value.Numerator * other.Numerator, value.Denominator * other.Denominator);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat Divide(BigFloat value, BigFloat other)
    {
        if (object.Equals(other, null))
            throw new ArgumentNullException(nameof(other));
        if (other.Numerator == 0L)
            throw new DivideByZeroException(nameof(other));
        return new BigFloat(value.Numerator * other.Denominator, value.Denominator * other.Numerator);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat Remainder(BigFloat value, BigFloat other)
    {
        if (object.Equals(other, null))
            throw new ArgumentNullException(nameof(other));
        return value - Floor(value / other) * other;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat DivideRemainder(
        BigFloat value,
        BigFloat other,
        out BigFloat remainder)
    {
        value = Divide(value, other);
        remainder = Remainder(value, other);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat Pow(BigFloat value, int exponent)
    {
        if (value.Numerator.IsZero)
            return value;
        if (exponent >= 0)
            return new BigFloat(BigInteger.Pow(value.Numerator, exponent), BigInteger.Pow(value.Denominator, exponent));
        var numerator = value.Numerator;
        return new BigFloat(BigInteger.Pow(value.Denominator, -exponent), BigInteger.Pow(numerator, -exponent));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat Pow(BigFloat value, BigFloat exponent)
    {
        if (value.Numerator.IsZero)
            return value;
        if (exponent >= 0)
            return new BigFloat(BigInteger.Pow(value.Numerator, Convert.ToInt32(exponent.ToString())),
                BigInteger.Pow(value.Denominator, Convert.ToInt32(exponent.ToString())));
        var numerator = value.Numerator;
        return new BigFloat(BigInteger.Pow(value.Denominator, Convert.ToInt32((-exponent).ToString())),
            BigInteger.Pow(numerator, Convert.ToInt32((-exponent).ToString())));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat Abs(BigFloat value)
    {
        return new BigFloat(BigInteger.Abs(value.Numerator), value.Denominator);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat Negate(BigFloat value)
    {
        return new BigFloat(BigInteger.Negate(value.Numerator), value.Denominator);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat Inverse(BigFloat value)
    {
        return new BigFloat(value.Denominator, value.Numerator);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat Increment(BigFloat value)
    {
        return new BigFloat(value.Numerator + value.Denominator, value.Denominator);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat Decrement(BigFloat value)
    {
        return new BigFloat(value.Numerator - value.Denominator, value.Denominator);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat Ceil(BigFloat value)
    {
        var numerator = value.Numerator;
        return Factor(new BigFloat(
            !(numerator < 0L)
                ? numerator + (value.Denominator - BigInteger.Remainder(numerator, value.Denominator))
                : numerator - BigInteger.Remainder(numerator, value.Denominator), value.Denominator));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat Floor(BigFloat value)
    {
        var numerator = value.Numerator;
        return Factor(new BigFloat(
            !(numerator < 0L)
                ? numerator - BigInteger.Remainder(numerator, value.Denominator)
                : numerator + (value.Denominator - BigInteger.Remainder(numerator, value.Denominator)),
            value.Denominator));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat Round(BigFloat value)
    {
        var valueStr = value.ToString();
        var firstDigitInFractionalPart =
            valueStr.Length > 1 ? Convert.ToInt32(valueStr.Split(',')[1][0].ToString()) : 0;
        if (firstDigitInFractionalPart < 5) return Truncate(value);
        return value < 0 ? Truncate(value) - 1 : Truncate(value) + 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat Round(BigFloat value, int radix)
    {
        if (radix > -1)
        {
            var multiplier = ParseBigFloat('1' + new string('0', radix));
            return Round(value * multiplier) / multiplier;
        }

        var divider = ParseBigFloat('1' + new string('0', -radix));
        return Round(value / divider) * divider;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat Truncate(BigFloat value)
    {
        var numerator = value.Numerator;
        return Factor(new BigFloat(numerator - BigInteger.Remainder(numerator, value.Denominator), value.Denominator));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat Decimals(BigFloat value)
    {
        return new BigFloat(BigInteger.Remainder(value.Numerator, value.Denominator), value.Denominator);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat ShiftDecimalLeft(BigFloat value, int shift)
    {
        return shift < 0
            ? ShiftDecimalRight(value, -shift)
            : new BigFloat(value.Numerator * BigInteger.Pow(10, shift), value.Denominator);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat ShiftDecimalRight(BigFloat value, int shift)
    {
        if (shift < 0)
            return ShiftDecimalLeft(value, -shift);
        var denominator = value.Denominator * BigInteger.Pow(10, shift);
        return new BigFloat(value.Numerator, denominator);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat Sqrt(BigFloat value)
    {
        return Divide(Math.Pow(10.0, BigInteger.Log10(value.Numerator) / 2.0),
            Math.Pow(10.0, BigInteger.Log10(value.Denominator) / 2.0));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static double Log10(BigFloat value)
    {
        return BigInteger.Log10(value.Numerator) - BigInteger.Log10(value.Denominator);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static double Log(BigFloat value, double baseValue)
    {
        return BigInteger.Log(value.Numerator, baseValue) - BigInteger.Log(value.Numerator, baseValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat Factor(BigFloat value)
    {
        if (value.Denominator == 1L)
            return value;
        var bigInteger = BigInteger.GreatestCommonDivisor(value.Numerator, value.Denominator);
        return new BigFloat(value.Numerator / bigInteger, value.Denominator / bigInteger);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public new static bool Equals(object left, object right)
    {
        if (left == null && right == null)
            return true;
        return left != null && right != null && !(left.GetType() != right.GetType()) &&
               ((BigInteger)left).Equals((BigInteger)right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static string ToString(BigFloat value)
    {
        return value.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat Parse(string value)
    {
        value = value != null ? value.Trim() : throw new ArgumentNullException(nameof(value));
        var numberFormat = Thread.CurrentThread.CurrentCulture.NumberFormat;
        value = value.Replace(numberFormat.NumberGroupSeparator, "");
        var num = value.IndexOf(numberFormat.NumberDecimalSeparator);
        value = value.Replace(numberFormat.NumberDecimalSeparator, "");
        return num < 0
            ? Factor(BigInteger.Parse(value))
            : Factor(new BigFloat(BigInteger.Parse(value), BigInteger.Pow(10, value.Length - num)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat ParseBigFloat(string number)
    {
        number = number.Replace('.', ',');
        number = number.TrimEnd(',');
        return Parse(number);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public BigFloat WriteLine()
    {
        Console.WriteLine(this);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool TryParse(string value, out BigFloat result)
    {
        try
        {
            result = Parse(value);
            return true;
        }
        catch (ArgumentNullException ex)
        {
            result = new BigFloat();
            return false;
        }
        catch (FormatException ex)
        {
            result = new BigFloat();
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static int Compare(BigFloat left, BigFloat right)
    {
        if (object.Equals(left, null))
            throw new ArgumentNullException(nameof(left));
        return !Equals(right, null)
            ? new BigFloat(left).CompareTo(right)
            : throw new ArgumentNullException(nameof(right));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public override string ToString()
    {
        var resultStr = ToString(100);
        if (resultStr.Contains('-')) resultStr = '-' + resultStr.Replace("-", "");

        return resultStr;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public string ToString(int precision, bool trailingZeros = false)
    {
        var bigFloat = Factor(this);
        var numberFormat = Thread.CurrentThread.CurrentCulture.NumberFormat;
        BigInteger remainder;
        var bigInteger1 = BigInteger.DivRem(bigFloat.Numerator, bigFloat.Denominator, out remainder);
        if ((remainder == 0L) & trailingZeros)
            return bigInteger1 + numberFormat.NumberDecimalSeparator + "0";
        if (remainder == 0L)
            return bigInteger1.ToString();
        var bigInteger2 = bigFloat.Numerator * BigInteger.Pow(10, precision) / bigFloat.Denominator;
        if ((bigInteger2 == 0L) & trailingZeros)
            return bigInteger1 + numberFormat.NumberDecimalSeparator + "0";
        if (bigInteger2 == 0L)
            return bigInteger1.ToString();
        var stringBuilder = new StringBuilder();
        while (precision-- > 0)
        {
            stringBuilder.Append(bigInteger2 % 10);
            bigInteger2 /= 10;
        }

        var str = bigInteger1 + numberFormat.NumberDecimalSeparator +
                  new string(stringBuilder.ToString().Reverse().ToArray());
        if (trailingZeros)
            return str;
        return str.TrimEnd('0');
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public string ToMixString()
    {
        var bigFloat = Factor(this);
        BigInteger remainder;
        var bigInteger = BigInteger.DivRem(bigFloat.Numerator, bigFloat.Denominator, out remainder);
        if (remainder == 0L)
            return bigInteger.ToString();
        return bigInteger + ", " + remainder + "/" + bigFloat.Denominator;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public string ToRationalString()
    {
        var bigFloat = Factor(this);
        var bigInteger = bigFloat.Numerator;
        var str1 = bigInteger.ToString();
        bigInteger = bigFloat.Denominator;
        var str2 = bigInteger.ToString();
        return str1 + " / " + str2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public int CompareTo(BigFloat other)
    {
        if (object.Equals(other, null))
            throw new ArgumentNullException(nameof(other));
        var numerator1 = Numerator;
        var numerator2 = other.Numerator;
        var denominator = other.Denominator;
        return BigInteger.Compare(numerator1 * denominator, numerator2 * Denominator);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public int CompareTo(object obj)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));
        if (!(obj is BigFloat other))
            throw new ArgumentException("obj is not a BigFloat");
        return CompareTo(other);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public override bool Equals(object obj)
    {
        return obj != null && !(GetType() != obj.GetType()) && Numerator == ((BigFloat)obj).Numerator &&
               Denominator == ((BigFloat)obj).Denominator;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool Equals(BigFloat other)
    {
        return other.Numerator * Denominator == Numerator * other.Denominator;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public override int GetHashCode()
    {
        return (Numerator, Denominator).GetHashCode();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat operator -(BigFloat value)
    {
        return Negate(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat operator -(BigFloat left, BigFloat right)
    {
        return Subtract(left, right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat operator --(BigFloat value)
    {
        return Decrement(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat operator +(BigFloat left, BigFloat right)
    {
        return Add(left, right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat operator +(BigFloat value)
    {
        return Abs(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat operator ++(BigFloat value)
    {
        return Increment(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat operator %(BigFloat left, BigFloat right)
    {
        return Remainder(left, right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat operator *(BigFloat left, BigFloat right)
    {
        return Multiply(left, right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat operator /(BigFloat left, BigFloat right)
    {
        return Divide(left, right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat operator >> (BigFloat value, int shift)
    {
        return ShiftDecimalRight(value, shift);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat operator <<(BigFloat value, int shift)
    {
        return ShiftDecimalLeft(value, shift);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat operator ^(BigFloat left, int right)
    {
        return Pow(left, right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat operator ^(BigFloat left, BigFloat right)
    {
        return Pow(left, right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static BigFloat operator ~(BigFloat value)
    {
        return Inverse(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool operator !=(BigFloat left, BigFloat right)
    {
        return Compare(left, right) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool operator ==(BigFloat left, BigFloat right)
    {
        return Compare(left, right) == 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool operator <(BigFloat left, BigFloat right)
    {
        return Compare(left, right) < 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool operator <=(BigFloat left, BigFloat right)
    {
        return Compare(left, right) <= 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool operator >(BigFloat left, BigFloat right)
    {
        return Compare(left, right) > 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool operator >=(BigFloat left, BigFloat right)
    {
        return Compare(left, right) >= 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool operator true(BigFloat value)
    {
        return value != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool operator false(BigFloat value)
    {
        return value == 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static explicit operator decimal(BigFloat value)
    {
        if (decimal.MinValue > value)
            throw new OverflowException("value is less than decimal.MinValue.");
        if (decimal.MaxValue < value)
            throw new OverflowException("value is greater than decimal.MaxValue.");
        return (decimal)value.Numerator / (decimal)value.Denominator;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static explicit operator double(BigFloat value)
    {
        if (double.MinValue > value)
            throw new OverflowException("value is less than double.MinValue.");
        if (double.MaxValue < value)
            throw new OverflowException("value is greater than double.MaxValue.");
        return (double)value.Numerator / (double)value.Denominator;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static explicit operator float(BigFloat value)
    {
        if (float.MinValue > value)
            throw new OverflowException("value is less than float.MinValue.");
        if (float.MaxValue < value)
            throw new OverflowException("value is greater than float.MaxValue.");
        return (float)value.Numerator / (float)value.Denominator;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static implicit operator BigFloat(byte value)
    {
        return new BigFloat((uint)value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static implicit operator BigFloat(sbyte value)
    {
        return new BigFloat(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static implicit operator BigFloat(short value)
    {
        return new BigFloat(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static implicit operator BigFloat(ushort value)
    {
        return new BigFloat((uint)value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static implicit operator BigFloat(int value)
    {
        return new BigFloat(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static implicit operator BigFloat(long value)
    {
        return new BigFloat(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static implicit operator BigFloat(uint value)
    {
        return new BigFloat(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static implicit operator BigFloat(ulong value)
    {
        return new BigFloat(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static implicit operator BigFloat(decimal value)
    {
        return new BigFloat(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static implicit operator BigFloat(double value)
    {
        return new BigFloat(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static implicit operator BigFloat(float value)
    {
        return new BigFloat(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static implicit operator BigFloat(BigInteger value)
    {
        return new BigFloat(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static explicit operator BigFloat(string value)
    {
        return new BigFloat(value);
    }

    # region IConvertible

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public TypeCode GetTypeCode()
    {
        return TypeCode.Int64;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool ToBoolean(IFormatProvider? provider)
    {
        return Convert.ToBoolean(ToString());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public byte ToByte(IFormatProvider? provider)
    {
        return Convert.ToByte(ToString());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public char ToChar(IFormatProvider? provider)
    {
        return Convert.ToChar(ToString());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public DateTime ToDateTime(IFormatProvider? provider)
    {
        return Convert.ToDateTime(ToString());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public decimal ToDecimal(IFormatProvider? provider)
    {
        return Convert.ToDecimal(ToString());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public double ToDouble(IFormatProvider? provider)
    {
        return Convert.ToDouble(ToString());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public short ToInt16(IFormatProvider? provider)
    {
        return Convert.ToInt16(ToString());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public int ToInt32(IFormatProvider? provider)
    {
        return Convert.ToInt32(ToString());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public long ToInt64(IFormatProvider? provider)
    {
        return Convert.ToInt64(ToString());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public sbyte ToSByte(IFormatProvider? provider)
    {
        return Convert.ToSByte(ToString());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public float ToSingle(IFormatProvider? provider)
    {
        return Convert.ToSingle(ToString());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public string ToString(IFormatProvider? provider)
    {
        return ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public object ToType(Type conversionType, IFormatProvider? provider)
    {
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public ushort ToUInt16(IFormatProvider? provider)
    {
        return Convert.ToUInt16(ToString());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public uint ToUInt32(IFormatProvider? provider)
    {
        return Convert.ToUInt32(ToString());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public ulong ToUInt64(IFormatProvider? provider)
    {
        return Convert.ToUInt64(ToString());
    }

    # endregion
}