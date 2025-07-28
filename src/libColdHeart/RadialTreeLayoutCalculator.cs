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
        // First, organize nodes by generation (distance from root)
        var nodesByGeneration = OrganizeNodesByGeneration(root);

        // Convert to layout tree structure
        var layoutRoot = ConvertToLayoutTree(root);

        // Position nodes radially
        PositionNodesRadially(layoutRoot, nodesByGeneration);

        return layoutRoot;
    }

    private Dictionary<Int32, List<TreeNode>> OrganizeNodesByGeneration(TreeNode root)
    {
        var nodesByGeneration = new Dictionary<Int32, List<TreeNode>>();
        var queue = new Queue<(TreeNode Node, Int32 Generation)>();

        queue.Enqueue((root, 0));

        while (queue.Count > 0)
        {
            var (node, generation) = queue.Dequeue();

            if (!nodesByGeneration.ContainsKey(generation))
            {
                nodesByGeneration[generation] = new List<TreeNode>();
            }

            nodesByGeneration[generation].Add(node);

            // Add children to next generation
            if (node.LeftChild != null)
            {
                queue.Enqueue((node.LeftChild, generation + 1));
            }
            if (node.RightChild != null)
            {
                queue.Enqueue((node.RightChild, generation + 1));
            }
        }

        return nodesByGeneration;
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

    private void PositionNodesRadially(LayoutNode layoutRoot, Dictionary<Int32, List<TreeNode>> nodesByGeneration)
    {
        // Build a mapping from TreeNode values to LayoutNodes for quick lookup
        var valueToLayoutNode = BuildValueToLayoutNodeMapping(layoutRoot);

        // Position root at center
        layoutRoot.X = CENTER_X - layoutRoot.Width / 2.0f;
        layoutRoot.Y = CENTER_Y - layoutRoot.Height / 2.0f;

        // Start positioning from root with initial angle 0 (top)
        PositionNodeRecursively(layoutRoot, 0.0f, 0);
    }

    private void PositionNodeRecursively(LayoutNode node, Single angle, Int32 generation)
    {
        // Position current node (if not root)
        if (generation > 0)
        {
            PositionNodeAtAngle(node, angle, generation);
        }

        // Position children based on number of children and parent angle
        if (node.Children.Count == 1)
        {
            // Single child: continue in same direction
            PositionNodeRecursively(node.Children[0], angle, generation + 1);
        }
        else if (node.Children.Count == 2)
        {
            // Two children: spread based on parent position
            var sortedChildren = node.Children.OrderBy(child => child.Value).ToList();

            Single leftAngle, rightAngle;

            if (generation == 0) // Root
            {
                // From root: first child (smaller value) at 270°, second at 90°
                leftAngle = 270.0f;
                rightAngle = 90.0f;
            }
            else
            {
                // From non-root: position children to avoid crossings
                // This is a simplified strategy - may need refinement
                if (angle == 0.0f) // Parent at top
                {
                    leftAngle = 270.0f; // Left
                    rightAngle = 90.0f;  // Right
                }
                else if (angle == 90.0f) // Parent at right
                {
                    leftAngle = 45.0f;   // Upper right
                    rightAngle = 135.0f; // Lower right
                }
                else if (angle == 270.0f) // Parent at left
                {
                    leftAngle = 315.0f;  // Upper left
                    rightAngle = 225.0f; // Lower left
                }
                else
                {
                    // General case: symmetric spread
                    leftAngle = NormalizeAngle(angle - 45.0f);
                    rightAngle = NormalizeAngle(angle + 45.0f);
                }
            }

            PositionNodeRecursively(sortedChildren[0], leftAngle, generation + 1);
            PositionNodeRecursively(sortedChildren[1], rightAngle, generation + 1);
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



    private Dictionary<BigInteger, LayoutNode> BuildValueToLayoutNodeMapping(LayoutNode root)
    {
        var mapping = new Dictionary<BigInteger, LayoutNode>();
        var queue = new Queue<LayoutNode>();

        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            mapping[node.Value] = node;

            foreach (var child in node.Children)
            {
                queue.Enqueue(child);
            }
        }

        return mapping;
    }
}
