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

        // Position nodes generation by generation
        foreach (var generation in nodesByGeneration.Keys.OrderBy(g => g))
        {
            if (generation == 0) continue; // Root already positioned

            var nodesInGeneration = nodesByGeneration[generation];
            PositionGenerationNodes(nodesInGeneration, valueToLayoutNode, generation);
        }
    }

    private void PositionGenerationNodes(List<TreeNode> nodesInGeneration, Dictionary<BigInteger, LayoutNode> valueToLayoutNode, Int32 generation)
    {
        if (nodesInGeneration.Count == 0) return;

        // Sort nodes by value for consistent ordering
        var sortedNodes = nodesInGeneration.OrderBy(n => n.Value).ToList();

        // Calculate angular spacing to distribute nodes evenly around the circle
        Single angleStep = 360.0f / Math.Max(sortedNodes.Count, 1);

        // Position each node at even intervals around the circle
        for (Int32 i = 0; i < sortedNodes.Count; i++)
        {
            var treeNode = sortedNodes[i];
            if (!valueToLayoutNode.TryGetValue(treeNode.Value, out var layoutNode))
                continue;

            // Calculate angle (starting at 0° = top, going clockwise)
            Single angle = i * angleStep;

            // Position the node
            PositionNodeAtAngle(layoutNode, angle, generation);
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
