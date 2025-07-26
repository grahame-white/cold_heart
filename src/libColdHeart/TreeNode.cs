using System.Numerics;

namespace libColdHeart;

public class TreeNode
{
    public BigInteger Value { get; }
    public TreeNode? LeftChild { get; set; }
    public TreeNode? RightChild { get; set; }

    public TreeNode(BigInteger value)
    {
        Value = value;
    }
}
