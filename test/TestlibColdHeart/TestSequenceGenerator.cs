using System.Numerics;
using libColdHeart;

namespace TestlibColdHeart;

public class Tests
{
    private SequenceGenerator _gen;

    [SetUp]
    public void Setup()
    {
        _gen = new SequenceGenerator();
    }

    [Test]
    public void Sequence_Initially_Empty()
    {
        Assert.That(_gen.Sequence.Count, Is.Zero);
    }

    [Test]
    public void Add_AddsKeyToSequence_WhenKeyNotInSequence()
    {
        _gen.Add(1);
        Assert.That(_gen.Sequence.ContainsKey(1), Is.True);
    }

    [Test]
    public void Add_SetsSequenceValueTo3nPlus1_WhenKeyIsOdd()
    {
        BigInteger oddKey = 1;
        _gen.Add(1);

        BigInteger next = 3 * oddKey + 1;
        Assert.That(_gen.Sequence[1], Is.EqualTo(next));
    }

    [Test]
    public void Add_SetsSequenceValueTonDividedBy2_WhenKeyIsEven()
    {
        BigInteger evenKey = 2;
        _gen.Add(2);

        BigInteger next = evenKey / 2;
        Assert.That(_gen.Sequence[2], Is.EqualTo(next));
    }

    [Test]
    public void Add_ContinuesToAddValues_UntilKeyIsInSequence()
    {
        BigInteger key = 1;
        _gen.Add(key);

        // expected sequence : 1, 4, 2, (1)
        Assert.That(_gen.Sequence.Count, Is.EqualTo(3));
    }
}
