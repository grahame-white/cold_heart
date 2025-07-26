using System.Numerics;
using System.Text.Json.Serialization;

namespace libColdHeart;

public class TreeNode
{
    [JsonPropertyName("value")]
    public BigInteger Value { get; init; }

    [JsonPropertyName("leftChild")]
    public TreeNode? LeftChild { get; set; }

    [JsonPropertyName("rightChild")]
    public TreeNode? RightChild { get; set; }

    [JsonConstructor]
    public TreeNode(BigInteger value)
    {
        Value = value;
    }

    public TreeNode()
    {
        Value = BigInteger.Zero;
    }
}
