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

        foreach (var generationEntry in nodesByGeneration.OrderBy(kvp => kvp.Key))
        {
            var generation = generationEntry.Key;
            var nodesInGeneration = generationEntry.Value;

            if (generation == 0)
            {
                // Position root at center
                layoutRoot.X = CENTER_X - layoutRoot.Width / 2.0f;
                layoutRoot.Y = CENTER_Y - layoutRoot.Height / 2.0f;
            }
            else
            {
                // Calculate radius for this generation
                Single radius = BASE_RADIUS + (generation - 1) * RADIUS_INCREMENT;

                // For proper radial tree layout with placeholder spacing:
                // Each generation can have at most 2^generation nodes in a binary tree
                Int32 maxNodesAtGeneration = (Int32)Math.Pow(2, generation);

                // Sort nodes to ensure consistent positioning
                var sortedNodes = nodesInGeneration.OrderBy(n => n.Value).ToList();

                // Calculate angular spacing between potential positions
                Single angleIncrement = 360.0f / maxNodesAtGeneration;

                // Assign each node to a specific slot to maintain placeholder spacing
                var nodeSlots = AssignNodesToSlots(sortedNodes, maxNodesAtGeneration);

                // Position each node at its assigned slot
                for (Int32 slotIndex = 0; slotIndex < maxNodesAtGeneration; slotIndex++)
                {
                    if (nodeSlots.ContainsKey(slotIndex))
                    {
                        var node = nodeSlots[slotIndex];
                        if (valueToLayoutNode.TryGetValue(node.Value, out var layoutNode))
                        {
                            // Calculate angle for this slot (start from top: -90 degrees)
                            Single angle = -90.0f + (slotIndex * angleIncrement);

                            // Convert to radians for trigonometric calculations
                            Single angleRadians = angle * (Single)Math.PI / 180.0f;

                            // Calculate position on the circle
                            Single x = CENTER_X + radius * (Single)Math.Cos(angleRadians) - layoutNode.Width / 2.0f;
                            Single y = CENTER_Y + radius * (Single)Math.Sin(angleRadians) - layoutNode.Height / 2.0f;

                            layoutNode.X = x;
                            layoutNode.Y = y;
                        }
                    }
                }
            }
        }
    }

    private Dictionary<Int32, TreeNode> AssignNodesToSlots(List<TreeNode> sortedNodes, Int32 maxSlots)
    {
        var nodeSlots = new Dictionary<Int32, TreeNode>();
        
        // Distribute nodes evenly across available slots
        // This ensures even spacing with placeholder gaps for missing nodes
        for (Int32 i = 0; i < sortedNodes.Count; i++)
        {
            // Calculate which slot this node should occupy
            // Use floating point division to get even distribution, then round to nearest slot
            Single exactSlot = (Single)i * maxSlots / sortedNodes.Count;
            Int32 slotIndex = (Int32)Math.Round(exactSlot) % maxSlots;
            
            // Handle collision by finding next available slot
            while (nodeSlots.ContainsKey(slotIndex))
            {
                slotIndex = (slotIndex + 1) % maxSlots;
            }
            
            nodeSlots[slotIndex] = sortedNodes[i];
        }

        return nodeSlots;
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