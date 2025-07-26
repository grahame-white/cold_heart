using System.Collections.Generic;
using System.Numerics;

namespace libColdHeart;

public class LayoutNode
{
    public BigInteger Value { get; init; }
    public System.Single X { get; set; }
    public System.Single Y { get; set; }
    public System.Single Width { get; set; } = 60.0f;
    public System.Single Height { get; set; } = 30.0f;
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