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
    public void Root_Initially_HasValue1()
    {
        Assert.That(_gen.Root.Value, Is.EqualTo(new BigInteger(1)));
    }

    [Test]
    public void Root_Initially_HasNoChildren()
    {
        Assert.That(_gen.Root.LeftChild, Is.Null);
        Assert.That(_gen.Root.RightChild, Is.Null);
    }

    [Test]
    public void Add_DoesNothing_WhenInputIs1()
    {
        _gen.Add(1);
        Assert.That(_gen.Root.LeftChild, Is.Null);
        Assert.That(_gen.Root.RightChild, Is.Null);
    }

    [Test]
    public void Add_AddsNodeToTree_WhenInputIsNot1()
    {
        _gen.Add(2);
        Assert.That(_gen.Root.LeftChild, Is.Not.Null);
        Assert.That(_gen.Root.LeftChild!.Value, Is.EqualTo(new BigInteger(2)));
    }

    [Test]
    public void Add_BuildsPathFromInputTo1()
    {
        _gen.Add(4);
        // Path: 4 -> 2 -> 1
        // Tree: 1 has child 2, 2 has child 4
        Assert.That(_gen.Root.LeftChild, Is.Not.Null);
        Assert.That(_gen.Root.LeftChild!.Value, Is.EqualTo(new BigInteger(2)));
        Assert.That(_gen.Root.LeftChild!.LeftChild, Is.Not.Null);
        Assert.That(_gen.Root.LeftChild!.LeftChild!.Value, Is.EqualTo(new BigInteger(4)));
    }

    [Test]
    public void Add_HandlesComplexSequence()
    {
        _gen.Add(3);
        // Path: 3 -> 10 -> 5 -> 16 -> 8 -> 4 -> 2 -> 1
        // Should build tree with proper parent-child relationships
        Assert.That(_gen.Root.LeftChild, Is.Not.Null);
        Assert.That(_gen.Root.LeftChild!.Value, Is.EqualTo(new BigInteger(2)));
    }

    [Test]
    public void Add_DoesNotDuplicateExistingNodes()
    {
        _gen.Add(2);
        _gen.Add(4);

        // Both 2 and 4 should be in tree, but no duplicates
        // 4 -> 2 -> 1, so 2 should already exist when adding 4
        Assert.That(_gen.Root.LeftChild, Is.Not.Null);
        Assert.That(_gen.Root.LeftChild!.Value, Is.EqualTo(new BigInteger(2)));
        Assert.That(_gen.Root.LeftChild!.LeftChild, Is.Not.Null);
        Assert.That(_gen.Root.LeftChild!.LeftChild!.Value, Is.EqualTo(new BigInteger(4)));
    }
}
