using System.IO;
using System.Numerics;
using System.Threading.Tasks;
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
    public void Root_Initially_HasNoLeftChild()
    {
        Assert.That(_gen.Root.LeftChild, Is.Null);
    }

    [Test]
    public void Root_Initially_HasNoRightChild()
    {
        Assert.That(_gen.Root.RightChild, Is.Null);
    }

    [Test]
    public void Add_DoesNotAddLeftChild_WhenInputIs1()
    {
        _gen.Add(1);
        Assert.That(_gen.Root.LeftChild, Is.Null);
    }

    [Test]
    public void Add_DoesNotAddRightChild_WhenInputIs1()
    {
        _gen.Add(1);
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
    public void Add_DoesNotDuplicateExistingNodes_CreatesCorrectTreeStructure()
    {
        _gen.Add(2);
        _gen.Add(4);

        // Verify complete tree structure: 1 -> 2 -> 4
        // When adding 4, it should create: 4 -> 2 -> 1
        // But since 2 already exists, no duplication should occur

        // Verify node 2 is properly connected as left child of root (1)
        Assert.That(_gen.Root.LeftChild, Is.Not.Null);
        Assert.That(_gen.Root.LeftChild.Value, Is.EqualTo(new BigInteger(2)));

        // Verify node 4 is properly connected as left child of node 2
        Assert.That(_gen.Root.LeftChild.LeftChild, Is.Not.Null);
        Assert.That(_gen.Root.LeftChild.LeftChild.Value, Is.EqualTo(new BigInteger(4)));
    }

    [Test]
    public async Task SaveToFileAsync_CreatesValidJsonFile()
    {
        _gen.Add(2);
        _gen.Add(4);

        var tempFile = Path.GetTempFileName();
        try
        {
            await _gen.SaveToFileAsync(tempFile);

            Assert.That(File.Exists(tempFile), Is.True);
            var content = await File.ReadAllTextAsync(tempFile);
            Assert.That(content, Is.Not.Empty);
            Assert.That(content, Does.Contain("\"value\": \"1\""));
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Test]
    public async Task LoadFromFileAsync_ReconstructsSequenceCorrectly()
    {
        // First create and save a sequence
        _gen.Add(2);
        _gen.Add(4);

        var tempFile = Path.GetTempFileName();
        try
        {
            await _gen.SaveToFileAsync(tempFile);

            // Load the sequence from file
            var loadedGen = await SequenceGenerator.LoadFromFileAsync(tempFile);

            // Verify the loaded sequence has the same structure
            Assert.That(loadedGen.Root.Value, Is.EqualTo(new BigInteger(1)));
            Assert.That(loadedGen.Root.LeftChild, Is.Not.Null);
            Assert.That(loadedGen.Root.LeftChild!.Value, Is.EqualTo(new BigInteger(2)));
            Assert.That(loadedGen.Root.LeftChild!.LeftChild, Is.Not.Null);
            Assert.That(loadedGen.Root.LeftChild!.LeftChild!.Value, Is.EqualTo(new BigInteger(4)));
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}
