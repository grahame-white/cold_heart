using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace libColdHeart;

public class AngularTreeLayoutCalculator
{
    private const Single DefaultNodeWidth = 60.0f;
    private const Single DefaultNodeHeight = 30.0f;

    public LayoutNode CalculateLayout(TreeNode root, AngularVisualizationConfig? config = null)
    {
        config ??= new AngularVisualizationConfig();

        // First, calculate tree metrics needed for scaling
        var metrics = CalculateTreeMetrics(root);

        // Convert to layout tree with angular positioning
        var layoutRoot = ConvertToAngularLayoutTree(root, metrics);

        // Position nodes starting from bottom-left corner
        PositionNodesAngularly(layoutRoot, metrics, config);

        return layoutRoot;
    }

    public TreeMetrics CalculateTreeMetrics(TreeNode root)
    {
        var pathLengths = new ConcurrentDictionary<BigInteger, Int32>();
        var traversalCounts = new ConcurrentDictionary<BigInteger, Int32>();
        var allNodes = new ConcurrentBag<BigInteger>();

        // Calculate both metrics in parallel with optimized algorithms
        var pathTask = Task.Run(() => CalculatePathLengthsOptimized(root, pathLengths, allNodes));
        var traversalTask = Task.Run(() => CalculateTraversalCountsOptimized(root, traversalCounts));

        // Wait for both calculations to complete
        Task.WaitAll(pathTask, traversalTask);

        var furthestDistance = pathLengths.Values.Max();
        var longestPath = pathLengths.Values.Max();

        return new TreeMetrics(furthestDistance, longestPath, pathLengths.ToDictionary(), traversalCounts.ToDictionary());
    }

    private void CalculatePathLengths(TreeNode? node, Int32 currentDepth,
        ConcurrentDictionary<BigInteger, Int32> pathLengths,
        ConcurrentBag<BigInteger> allNodes)
    {
        if (node == null) return;

        // Update path length (distance from root) using thread-safe operations
        pathLengths.AddOrUpdate(node.Value, currentDepth, (key, existingDepth) => Math.Min(existingDepth, currentDepth));

        allNodes.Add(node.Value);

        // Recursively process children
        CalculatePathLengths(node.LeftChild, currentDepth + 1, pathLengths, allNodes);
        CalculatePathLengths(node.RightChild, currentDepth + 1, pathLengths, allNodes);
    }

    private void CalculatePathLengthsOptimized(TreeNode root,
        ConcurrentDictionary<BigInteger, Int32> pathLengths,
        ConcurrentBag<BigInteger> allNodes)
    {
        // Use iterative approach with concurrent processing for better performance
        var processedNodes = new ConcurrentDictionary<BigInteger, bool>();
        var workStack = new ConcurrentStack<(TreeNode Node, Int32 Depth)>();
        workStack.Push((root, 0));

        // Process nodes using parallel tasks
        var tasks = Enumerable.Range(0, Environment.ProcessorCount).Select(_ => Task.Run(() =>
        {
            while (workStack.TryPop(out var item))
            {
                var (node, depth) = item;

                // Skip if already processed (can happen due to concurrent access)
                if (!processedNodes.TryAdd(node.Value, true))
                    continue;

                // Process current node
                pathLengths.AddOrUpdate(node.Value, depth, (key, existingDepth) => Math.Min(existingDepth, depth));
                allNodes.Add(node.Value);

                // Add children to work stack
                if (node.LeftChild != null)
                    workStack.Push((node.LeftChild, depth + 1));
                if (node.RightChild != null)
                    workStack.Push((node.RightChild, depth + 1));
            }
        })).ToArray();

        Task.WaitAll(tasks);
    }

    private void CalculateTraversalCounts(TreeNode? node, ConcurrentDictionary<BigInteger, Int32> traversalCounts)
    {
        if (node == null) return;

        // Count how many leaf nodes are in the subtree rooted at this node
        var leafCount = CountLeavesInSubtree(node);
        traversalCounts[node.Value] = leafCount;

        // Recursively calculate for children
        CalculateTraversalCounts(node.LeftChild, traversalCounts);
        CalculateTraversalCounts(node.RightChild, traversalCounts);
    }

    private void CalculateTraversalCountsOptimized(TreeNode root, ConcurrentDictionary<BigInteger, Int32> traversalCounts)
    {
        // Use bottom-up approach with memoization for better performance
        var leafCountMemo = new ConcurrentDictionary<BigInteger, Int32>();

        // First pass: collect all nodes in post-order for bottom-up processing
        var allNodes = new List<TreeNode>();
        CollectNodesPostOrder(root, allNodes);

        // Process nodes in parallel, with leaf counts computed bottom-up
        Parallel.ForEach(allNodes, node =>
        {
            var leafCount = CountLeavesInSubtreeOptimized(node, leafCountMemo);
            traversalCounts[node.Value] = leafCount;
        });
    }

    private void CollectNodesPostOrder(TreeNode? node, List<TreeNode> nodes)
    {
        if (node == null) return;

        CollectNodesPostOrder(node.LeftChild, nodes);
        CollectNodesPostOrder(node.RightChild, nodes);
        nodes.Add(node);
    }

    private Int32 CountLeavesInSubtreeOptimized(TreeNode? node, ConcurrentDictionary<BigInteger, Int32> memo)
    {
        if (node == null) return 0;

        // Check memo first
        if (memo.TryGetValue(node.Value, out var cachedCount))
            return cachedCount;

        Int32 result;
        if (node.LeftChild == null && node.RightChild == null)
        {
            // Leaf node
            result = 1;
        }
        else
        {
            // Internal node - sum children (will be available due to post-order processing)
            var leftCount = node.LeftChild != null ? memo.GetValueOrDefault(node.LeftChild.Value, CountLeavesInSubtreeOptimized(node.LeftChild, memo)) : 0;
            var rightCount = node.RightChild != null ? memo.GetValueOrDefault(node.RightChild.Value, CountLeavesInSubtreeOptimized(node.RightChild, memo)) : 0;
            result = leftCount + rightCount;
        }

        memo[node.Value] = result;
        return result;
    }

    private Int32 CountLeavesInSubtree(TreeNode? node)
    {
        if (node == null) return 0;

        // If this is a leaf node, count it as 1
        if (node.LeftChild == null && node.RightChild == null)
        {
            return 1;
        }

        // Otherwise, sum the leaves in both subtrees
        return CountLeavesInSubtree(node.LeftChild) + CountLeavesInSubtree(node.RightChild);
    }

    private LayoutNode ConvertToAngularLayoutTree(TreeNode node, TreeMetrics metrics)
    {
        var layoutNode = new LayoutNode(node)
        {
            Width = DefaultNodeWidth,
            Height = DefaultNodeHeight
        };

        // Add children
        if (node.LeftChild != null)
        {
            layoutNode.Children.Add(ConvertToAngularLayoutTree(node.LeftChild, metrics));
        }
        if (node.RightChild != null)
        {
            layoutNode.Children.Add(ConvertToAngularLayoutTree(node.RightChild, metrics));
        }

        return layoutNode;
    }

    private void PositionNodesAngularly(LayoutNode rootLayout, TreeMetrics metrics, AngularVisualizationConfig config)
    {
        // Position root at bottom-left (we'll adjust coordinates later to ensure proper positioning)
        rootLayout.X = 0.0f;
        rootLayout.Y = 0.0f;

        // Start positioning children with initial angle of 90 degrees (pointing up)
        Single initialAngle = 90.0f;
        PositionChildrenAngularly(rootLayout, initialAngle, metrics, config);

        // Adjust all coordinates to ensure root is at bottom-left
        AdjustToBottomLeft(rootLayout);
    }

    private void PositionChildrenAngularly(LayoutNode parent, Single currentAngle, TreeMetrics metrics, AngularVisualizationConfig config)
    {
        if (parent.Children.Count == 0) return;

        foreach (var child in parent.Children)
        {
            // Calculate angle change based on whether the child value is even or odd
            Single angleChange = (child.Value % 2 == 0) ? config.LeftTurnAngle : config.RightTurnAngle;
            Single newAngle = currentAngle + angleChange;

            // Calculate edge length based on distance from root
            Single edgeLength = CalculateEdgeLength(child.Value, metrics);

            // Convert angle to radians for trigonometric calculations
            Single angleRadians = newAngle * (Single)Math.PI / 180.0f;

            // Calculate child position relative to parent
            Single deltaX = edgeLength * (Single)Math.Cos(angleRadians);
            Single deltaY = edgeLength * (Single)Math.Sin(angleRadians);

            child.X = parent.X + deltaX;
            child.Y = parent.Y + deltaY;

            // Recursively position children of this child
            PositionChildrenAngularly(child, newAngle, metrics, config);
        }
    }

    private Single CalculateEdgeLength(BigInteger nodeValue, TreeMetrics metrics)
    {
        // Edge length scales as 1 / log(furthest node from root)
        var furthestDistance = metrics.FurthestDistance;

        if (furthestDistance <= 1)
        {
            return 80.0f; // Default edge length for single node or very small trees
        }

        // Use natural logarithm and scale appropriately
        Single baseLength = 200.0f; // Base scaling factor
        Single scaleFactor = 1.0f / (Single)Math.Log(furthestDistance + 1);

        return baseLength * scaleFactor;
    }

    private void AdjustToBottomLeft(LayoutNode rootLayout)
    {
        // Find the bounds of all nodes
        var bounds = CalculateBounds(rootLayout);

        // Calculate offset to move root to bottom-left
        Single offsetX = -bounds.MinX + 50.0f; // Small margin from left edge
        Single offsetY = -bounds.MinY + 50.0f; // Small margin from bottom edge

        // Apply offset to all nodes
        ApplyOffset(rootLayout, offsetX, offsetY);
    }

    private (Single MinX, Single MinY, Single MaxX, Single MaxY) CalculateBounds(LayoutNode rootLayout)
    {
        var allNodes = GetAllNodes(rootLayout);

        if (!allNodes.Any())
            return (0, 0, 0, 0);

        var minX = allNodes.Min(n => n.X);
        var maxX = allNodes.Max(n => n.X + n.Width);
        var minY = allNodes.Min(n => n.Y);
        var maxY = allNodes.Max(n => n.Y + n.Height);

        return (minX, minY, maxX, maxY);
    }

    private void ApplyOffset(LayoutNode node, Single offsetX, Single offsetY)
    {
        node.X += offsetX;
        node.Y += offsetY;

        foreach (var child in node.Children)
        {
            ApplyOffset(child, offsetX, offsetY);
        }
    }

    private List<LayoutNode> GetAllNodes(LayoutNode node)
    {
        var nodes = new List<LayoutNode> { node };

        foreach (var child in node.Children)
        {
            nodes.AddRange(GetAllNodes(child));
        }

        return nodes;
    }
}

public record TreeMetrics(
    Int32 FurthestDistance,
    Int32 LongestPath,
    Dictionary<BigInteger, Int32> PathLengths,
    Dictionary<BigInteger, Int32> TraversalCounts
);
