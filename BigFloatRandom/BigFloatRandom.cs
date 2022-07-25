namespace BigFloatRandom;

// use in compile project
public static class BigFloatRandom
{
    private static readonly Random Random = new();

    public static BigFloat GetRandomFloat(BigFloat a, BigFloat b)
    {
        var x = Random.NextDouble();
        var result = x * a + (1 - x) * b;
        return result;
    }

    public static BigFloat GetRandomInt(BigFloat a, BigFloat b)
    {
        return BigFloat.Round(GetRandomFloat(a, b));
    }
}