using System.Collections.Generic;
using System.Numerics;

namespace libColdHeart;

public class LayoutNode
{
    public BigInteger Value { get; init; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; } = 60.0f;
    public float Height { get; set; } = 30.0f;
    public List<LayoutNode> Children { get; init; } = new();

    public LayoutNode(BigInteger value)
    {
        Value = value;
    }

    public LayoutNode(TreeNode treeNode)
    {
        Value = treeNode.Value;
    }
}