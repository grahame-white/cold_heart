using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace libColdHeart;

public class TreeLayoutCalculator
{
    private const Single NODE_SPACING_Y = 80.0f;
    private const Single NODE_SPACING_X = 20.0f;
    private const Single DEFAULT_NODE_WIDTH = 60.0f;
    private const Single DEFAULT_NODE_HEIGHT = 30.0f;

    public LayoutNode CalculateLayout(TreeNode root)
    {
        var layoutRoot = ConvertToLayoutTree(root);
        CalculatePositions(layoutRoot, 0.0f, 0.0f);
        return layoutRoot;
    }

    private LayoutNode ConvertToLayoutTree(TreeNode node)
    {
        var layoutNode = new LayoutNode(node)
        {
            Width = DEFAULT_NODE_WIDTH,
            Height = DEFAULT_NODE_HEIGHT
        };

        var children = new List<TreeNode?> { node.LeftChild, node.RightChild }
            .Where(child => child != null)
            .Cast<TreeNode>()
            .ToList();

        foreach (var child in children)
        {
            layoutNode.Children.Add(ConvertToLayoutTree(child));
        }

        return layoutNode;
    }

    private void CalculatePositions(LayoutNode node, Single x, Single y)
    {
        node.X = x;
        node.Y = y;

        if (node.Children.Count == 0)
        {
            // Leaf node, no children to position
            return;
        }

        if (node.Children.Count == 1)
        {
            // Single child: place above parent in same X position
            var child = node.Children[0];
            CalculatePositions(child, x, y + NODE_SPACING_Y);
        }
        else
        {
            // Multiple children: sort by size (value), lowest on right, highest on left
            var sortedChildren = node.Children
                .OrderByDescending(child => child.Value)
                .ToList();

            // Calculate total width needed for all children
            Single totalChildrenWidth = CalculateSubtreeWidth(sortedChildren);
            Single startX = x - totalChildrenWidth / 2.0f;

            Single currentX = startX;
            foreach (var child in sortedChildren)
            {
                Single childSubtreeWidth = CalculateSubtreeWidth(new List<LayoutNode> { child });
                Single childCenterX = currentX + childSubtreeWidth / 2.0f;

                CalculatePositions(child, childCenterX, y + NODE_SPACING_Y);
                currentX += childSubtreeWidth + NODE_SPACING_X;
            }
        }
    }

    private Single CalculateSubtreeWidth(IList<LayoutNode> nodes)
    {
        if (!nodes.Any())
            return 0.0f;

        if (nodes.Count == 1)
        {
            var node = nodes[0];
            if (node.Children.Count == 0)
                return node.Width;

            // For nodes with children, the subtree width is the width of all child subtrees
            return Math.Max(node.Width, CalculateSubtreeWidth(node.Children));
        }

        // For multiple nodes, calculate total width including spacing
        Single totalWidth = 0.0f;
        for (Int32 i = 0; i < nodes.Count; i++)
        {
            totalWidth += CalculateSubtreeWidth(new List<LayoutNode> { nodes[i] });
            if (i < nodes.Count - 1)
                totalWidth += NODE_SPACING_X;
        }

        return totalWidth;
    }
}
