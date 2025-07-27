using System;
using System.Numerics;
using System.Text.Json;
using libColdHeart;

namespace TestlibColdHeart;

public class BigIntegerConverterTests
{
    private BigIntegerConverter _converter;
    private JsonSerializerOptions _options;

    [SetUp]
    public void Setup()
    {
        _converter = new BigIntegerConverter();
        _options = new JsonSerializerOptions();
        _options.Converters.Add(_converter);
    }

    [Test]
    public void Write_WithSmallBigInteger_WritesStringValue()
    {
        var value = new BigInteger(123);

        var json = JsonSerializer.Serialize(value, _options);

        Assert.That(json, Is.EqualTo("\"123\""));
    }

    [Test]
    public void Write_WithLargeBigInteger_WritesStringValue()
    {
        var value = new BigInteger(long.MaxValue) * 2;

        var json = JsonSerializer.Serialize(value, _options);

        Assert.That(json, Is.EqualTo($"\"{value}\""));
    }

    [Test]
    public void Write_WithZero_WritesStringValue()
    {
        var value = BigInteger.Zero;

        var json = JsonSerializer.Serialize(value, _options);

        Assert.That(json, Is.EqualTo("\"0\""));
    }

    [Test]
    public void Write_WithNegativeBigInteger_WritesStringValue()
    {
        var value = new BigInteger(-456);

        var json = JsonSerializer.Serialize(value, _options);

        Assert.That(json, Is.EqualTo("\"-456\""));
    }

    [Test]
    public void Read_WithValidStringNumber_ReturnsCorrectBigInteger()
    {
        var json = "\"789\"";

        var value = JsonSerializer.Deserialize<BigInteger>(json, _options);

        Assert.That(value, Is.EqualTo(new BigInteger(789)));
    }

    [Test]
    public void Read_WithValidLargeStringNumber_ReturnsCorrectBigInteger()
    {
        var largeNumber = "123456789012345678901234567890";
        var json = $"\"{largeNumber}\"";

        var value = JsonSerializer.Deserialize<BigInteger>(json, _options);

        Assert.That(value, Is.EqualTo(BigInteger.Parse(largeNumber)));
    }

    [Test]
    public void Read_WithValidNegativeStringNumber_ReturnsCorrectBigInteger()
    {
        var json = "\"-999\"";

        var value = JsonSerializer.Deserialize<BigInteger>(json, _options);

        Assert.That(value, Is.EqualTo(new BigInteger(-999)));
    }

    [Test]
    public void Read_WithValidNumberToken_ReturnsCorrectBigInteger()
    {
        var json = "42";

        var value = JsonSerializer.Deserialize<BigInteger>(json, _options);

        Assert.That(value, Is.EqualTo(new BigInteger(42)));
    }

    [Test]
    public void Read_WithValidLongValue_ReturnsCorrectBigInteger()
    {
        var json = $"{long.MaxValue}";

        var value = JsonSerializer.Deserialize<BigInteger>(json, _options);

        Assert.That(value, Is.EqualTo(new BigInteger(long.MaxValue)));
    }

    [Test]
    public void Read_WithInvalidStringFormat_ThrowsJsonException()
    {
        var json = "\"not-a-number\"";

        var ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<BigInteger>(json, _options));
        Assert.That(ex.Message, Does.Contain("Cannot convert String to BigInteger"));
    }

    [Test]
    public void Read_WithNullString_ThrowsJsonException()
    {
        var json = "null";

        var ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<BigInteger>(json, _options));
        Assert.That(ex.Message, Does.Contain("Cannot convert Null to BigInteger"));
    }

    [Test]
    public void Read_WithBooleanToken_ThrowsJsonException()
    {
        var json = "true";

        var ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<BigInteger>(json, _options));
        Assert.That(ex.Message, Does.Contain("Cannot convert True to BigInteger"));
    }

    [Test]
    public void Read_WithArrayToken_ThrowsJsonException()
    {
        var json = "[1, 2, 3]";

        var ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<BigInteger>(json, _options));
        Assert.That(ex.Message, Does.Contain("Cannot convert StartArray to BigInteger"));
    }

    [Test]
    public void Read_WithObjectToken_ThrowsJsonException()
    {
        var json = "{\"value\": 123}";

        var ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<BigInteger>(json, _options));
        Assert.That(ex.Message, Does.Contain("Cannot convert StartObject to BigInteger"));
    }

    [TestCaseSource(nameof(ValidStringNumberTestCases))]
    public void Read_WithValidStringNumbers_ReturnsCorrectValues(String jsonString, BigInteger expectedValue)
    {
        var json = $"\"{jsonString}\"";

        var value = JsonSerializer.Deserialize<BigInteger>(json, _options);

        Assert.That(value, Is.EqualTo(expectedValue));
    }

    private static System.Collections.IEnumerable ValidStringNumberTestCases()
    {
        yield return new TestCaseData("0", BigInteger.Zero);
        yield return new TestCaseData("1", BigInteger.One);
        yield return new TestCaseData("1000000000000000000000", BigInteger.Parse("1000000000000000000000"));
        yield return new TestCaseData("-1", new BigInteger(-1));
        yield return new TestCaseData("9223372036854775807", new BigInteger(long.MaxValue));
        yield return new TestCaseData("-9223372036854775808", new BigInteger(long.MinValue));
    }

    [TestCaseSource(nameof(ValidNumberTokenTestCases))]
    public void Read_WithValidNumberTokens_ReturnsCorrectValues(Int64 numberValue, BigInteger expectedValue)
    {
        var json = numberValue.ToString();

        var value = JsonSerializer.Deserialize<BigInteger>(json, _options);

        Assert.That(value, Is.EqualTo(expectedValue));
    }

    private static System.Collections.IEnumerable ValidNumberTokenTestCases()
    {
        yield return new TestCaseData(0L, BigInteger.Zero);
        yield return new TestCaseData(1L, BigInteger.One);
        yield return new TestCaseData(-1L, new BigInteger(-1));
        yield return new TestCaseData(long.MaxValue, new BigInteger(long.MaxValue));
        yield return new TestCaseData(long.MinValue, new BigInteger(long.MinValue));
        yield return new TestCaseData(12345L, new BigInteger(12345));
    }

    [Test]
    public void RoundTrip_SerializeAndDeserialize_PreservesValue()
    {
        var originalValue = BigInteger.Parse("987654321098765432109876543210");

        var json = JsonSerializer.Serialize(originalValue, _options);
        var deserializedValue = JsonSerializer.Deserialize<BigInteger>(json, _options);

        Assert.That(deserializedValue, Is.EqualTo(originalValue));
    }
}
