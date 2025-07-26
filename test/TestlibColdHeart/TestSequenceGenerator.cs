using System.Collections;
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
    }

    [Test]
    public void Add_CreatesNodeWithCorrectValue_WhenInputIsNot1()
    {
        _gen.Add(2);
        Assert.That(_gen.Root.LeftChild!.Value, Is.EqualTo(new BigInteger(2)));
    }

    [TestCaseSource(nameof(PathBuildingTestCases))]
    public void Add_CreatesExpectedNode_ForPathBuilding(BigInteger input, BigInteger expectedFirstChild)
    {
        _gen.Add(input);
        Assert.That(_gen.Root.LeftChild!.Value, Is.EqualTo(expectedFirstChild));
    }

    [Test]
    public void Add_BuildsPathFromFourTo1_CreatesNodeTwo()
    {
        _gen.Add(4);
        Assert.That(_gen.Root.LeftChild, Is.Not.Null);
    }

    [Test]
    public void Add_BuildsPathFromFourTo1_NodeTwoHasCorrectValue()
    {
        _gen.Add(4);
        Assert.That(_gen.Root.LeftChild!.Value, Is.EqualTo(new BigInteger(2)));
    }

    [Test]
    public void Add_BuildsPathFromFourTo1_NodeTwoHasChild()
    {
        _gen.Add(4);
        Assert.That(_gen.Root.LeftChild!.LeftChild, Is.Not.Null);
    }

    [Test]
    public void Add_BuildsPathFromFourTo1_NodeFourHasCorrectValue()
    {
        _gen.Add(4);
        Assert.That(_gen.Root.LeftChild!.LeftChild!.Value, Is.EqualTo(new BigInteger(4)));
    }

    [Test]
    public void Add_HandlesComplexSequence_CreatesFirstChild()
    {
        _gen.Add(3);
        Assert.That(_gen.Root.LeftChild, Is.Not.Null);
    }

    [Test]
    public void Add_HandlesComplexSequence_FirstChildHasCorrectValue()
    {
        _gen.Add(3);
        Assert.That(_gen.Root.LeftChild!.Value, Is.EqualTo(new BigInteger(2)));
    }

    [Test]
    public void Add_DoesNotDuplicateExistingNodes_NodeTwoExists()
    {
        _gen.Add(2);
        _gen.Add(4);
        Assert.That(_gen.Root.LeftChild, Is.Not.Null);
    }

    [Test]
    public void Add_DoesNotDuplicateExistingNodes_NodeTwoHasCorrectValue()
    {
        _gen.Add(2);
        _gen.Add(4);
        Assert.That(_gen.Root.LeftChild!.Value, Is.EqualTo(new BigInteger(2)));
    }

    [Test]
    public void Add_DoesNotDuplicateExistingNodes_NodeFourExists()
    {
        _gen.Add(2);
        _gen.Add(4);
        Assert.That(_gen.Root.LeftChild!.LeftChild, Is.Not.Null);
    }

    [Test]
    public void Add_DoesNotDuplicateExistingNodes_NodeFourHasCorrectValue()
    {
        _gen.Add(2);
        _gen.Add(4);
        Assert.That(_gen.Root.LeftChild!.LeftChild!.Value, Is.EqualTo(new BigInteger(4)));
    }

    [Test]
    public async Task SaveToFileAsync_CreatesFile()
    {
        _gen.Add(2);
        _gen.Add(4);

        var tempFile = Path.GetTempFileName();
        try
        {
            await _gen.SaveToFileAsync(tempFile);
            Assert.That(File.Exists(tempFile), Is.True);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Test]
    public async Task SaveToFileAsync_CreatesNonEmptyContent()
    {
        _gen.Add(2);
        _gen.Add(4);

        var tempFile = Path.GetTempFileName();
        try
        {
            await _gen.SaveToFileAsync(tempFile);
            var content = await File.ReadAllTextAsync(tempFile);
            Assert.That(content, Is.Not.Empty);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Test]
    public async Task SaveToFileAsync_ContainsExpectedRootValue()
    {
        _gen.Add(2);
        _gen.Add(4);

        var tempFile = Path.GetTempFileName();
        try
        {
            await _gen.SaveToFileAsync(tempFile);
            var content = await File.ReadAllTextAsync(tempFile);
            Assert.That(content, Does.Contain("\"value\": \"1\""));
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Test]
    public async Task LoadFromFileAsync_ReconstructsRootValue()
    {
        var tempFile = await CreateTestSequenceFile();
        try
        {
            var loadedGen = await SequenceGenerator.LoadFromFileAsync(tempFile);
            Assert.That(loadedGen.Root.Value, Is.EqualTo(new BigInteger(1)));
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Test]
    public async Task LoadFromFileAsync_ReconstructsFirstChild()
    {
        var tempFile = await CreateTestSequenceFile();
        try
        {
            var loadedGen = await SequenceGenerator.LoadFromFileAsync(tempFile);
            Assert.That(loadedGen.Root.LeftChild, Is.Not.Null);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Test]
    public async Task LoadFromFileAsync_ReconstructsFirstChildValue()
    {
        var tempFile = await CreateTestSequenceFile();
        try
        {
            var loadedGen = await SequenceGenerator.LoadFromFileAsync(tempFile);
            Assert.That(loadedGen.Root.LeftChild!.Value, Is.EqualTo(new BigInteger(2)));
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Test]
    public async Task LoadFromFileAsync_ReconstructsSecondChild()
    {
        var tempFile = await CreateTestSequenceFile();
        try
        {
            var loadedGen = await SequenceGenerator.LoadFromFileAsync(tempFile);
            Assert.That(loadedGen.Root.LeftChild!.LeftChild, Is.Not.Null);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Test]
    public async Task LoadFromFileAsync_ReconstructsSecondChildValue()
    {
        var tempFile = await CreateTestSequenceFile();
        try
        {
            var loadedGen = await SequenceGenerator.LoadFromFileAsync(tempFile);
            Assert.That(loadedGen.Root.LeftChild!.LeftChild!.Value, Is.EqualTo(new BigInteger(4)));
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    private async Task<string> CreateTestSequenceFile()
    {
        _gen.Add(2);
        _gen.Add(4);
        var tempFile = Path.GetTempFileName();
        await _gen.SaveToFileAsync(tempFile);
        return tempFile;
    }

    private static IEnumerable PathBuildingTestCases()
    {
        yield return new TestCaseData(new BigInteger(2), new BigInteger(2));
        yield return new TestCaseData(new BigInteger(4), new BigInteger(2));
        yield return new TestCaseData(new BigInteger(3), new BigInteger(2));
    }
}
