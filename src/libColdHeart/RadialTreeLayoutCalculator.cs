using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace libColdHeart;

public class RadialTreeLayoutCalculator
{
    private const Single DEFAULT_NODE_WIDTH = 60.0f;
    private const Single DEFAULT_NODE_HEIGHT = 30.0f;
    private const Single BASE_RADIUS = 100.0f;
    private const Single RADIUS_INCREMENT = 100.0f;
    private const Single CENTER_X = 300.0f;
    private const Single CENTER_Y = 300.0f;

    public LayoutNode CalculateLayout(TreeNode root)
    {
        // Convert to layout tree structure
        var layoutRoot = ConvertToLayoutTree(root);

        // Position root at center
        layoutRoot.X = CENTER_X - layoutRoot.Width / 2.0f;
        layoutRoot.Y = CENTER_Y - layoutRoot.Height / 2.0f;

        // Position nodes using sector-based allocation starting with full 360° sector
        PositionNodesUsingSectors(layoutRoot, 0.0f, 360.0f, 0);

        return layoutRoot;
    }

    private LayoutNode ConvertToLayoutTree(TreeNode node)
    {
        var layoutNode = new LayoutNode(node)
        {
            Width = DEFAULT_NODE_WIDTH,
            Height = DEFAULT_NODE_HEIGHT
        };

        // Add children
        if (node.LeftChild != null)
        {
            layoutNode.Children.Add(ConvertToLayoutTree(node.LeftChild));
        }
        if (node.RightChild != null)
        {
            layoutNode.Children.Add(ConvertToLayoutTree(node.RightChild));
        }

        return layoutNode;
    }

    private void PositionNodesUsingSectors(LayoutNode node, Single sectorStartAngle, Single sectorEndAngle, Int32 generation)
    {
        // Position current node (if not root)
        if (generation > 0)
        {
            // Position node at the center of its allocated sector
            Single sectorCenterAngle = (sectorStartAngle + sectorEndAngle) / 2.0f;
            PositionNodeAtAngle(node, sectorCenterAngle, generation);
        }

        // Allocate sectors to children
        if (node.Children.Count > 0)
        {
            // Sort children by value for consistent positioning
            var sortedChildren = node.Children.OrderBy(child => child.Value).ToList();

            Single sectorSize = sectorEndAngle - sectorStartAngle;
            Single childSectorSize = sectorSize / sortedChildren.Count;

            for (Int32 i = 0; i < sortedChildren.Count; i++)
            {
                Single childSectorStart = sectorStartAngle + i * childSectorSize;
                Single childSectorEnd = childSectorStart + childSectorSize;

                PositionNodesUsingSectors(sortedChildren[i], childSectorStart, childSectorEnd, generation + 1);
            }
        }
    }

    private void PositionNodeAtAngle(LayoutNode node, Single angle, Int32 generation)
    {
        // Calculate radius for this generation
        Single radius = BASE_RADIUS + (generation - 1) * RADIUS_INCREMENT;

        // Convert angle to radians (0 degrees = top, clockwise)
        Single angleRadians = angle * (Single)Math.PI / 180.0f;

        // Calculate position on the circle
        Single x = CENTER_X + radius * (Single)Math.Sin(angleRadians) - node.Width / 2.0f;
        Single y = CENTER_Y - radius * (Single)Math.Cos(angleRadians) - node.Height / 2.0f;

        node.X = x;
        node.Y = y;
    }

    private Single NormalizeAngle(Single angle)
    {
        while (angle < 0) angle += 360.0f;
        while (angle >= 360.0f) angle -= 360.0f;
        return angle;
    }
}
