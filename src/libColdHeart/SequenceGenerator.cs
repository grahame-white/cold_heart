using System;
using System.Collections.Generic;
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

public class SequenceGenerator
{
    public TreeNode Root { get; }
    private readonly Dictionary<BigInteger, TreeNode> _nodeMap;

    public SequenceGenerator()
    {
        Root = new TreeNode(1);
        _nodeMap = new Dictionary<BigInteger, TreeNode> { { 1, Root } };
    }

    public void Add(BigInteger inputNumber)
    {
        if (inputNumber == 1)
        {
            return; // Root already exists
        }

        BuildPathToRoot(inputNumber);
    }

    private void BuildPathToRoot(BigInteger number)
    {
        BigInteger currentNumber = number;
        var path = new List<BigInteger>();

        // Build forward path using traditional Collatz sequence
        while (!_nodeMap.ContainsKey(currentNumber))
        {
            path.Add(currentNumber);
            currentNumber = GetNext(currentNumber);
        }

        // Now add nodes to tree in reverse order (from known node back to input)
        TreeNode? knownNode = _nodeMap[currentNumber];

        for (int i = path.Count - 1; i >= 0; i--)
        {
            BigInteger nodeValue = path[i];

            if (!_nodeMap.ContainsKey(nodeValue))
            {
                TreeNode newNode = new TreeNode(nodeValue);
                _nodeMap[nodeValue] = newNode;

                // Add as child to the previous node in the reverse path
                AddChildToParent(knownNode, newNode);
                Console.WriteLine($"{knownNode.Value} -> {newNode.Value}");
            }

            knownNode = _nodeMap[nodeValue];
        }
    }

    private static void AddChildToParent(TreeNode parent, TreeNode child)
    {
        if (parent.LeftChild == null)
        {
            parent.LeftChild = child;
        }
        else if (parent.RightChild == null)
        {
            parent.RightChild = child;
        }
        // If both children exist, we don't add (should not happen in normal Collatz)
    }

    private static BigInteger GetNext(BigInteger inputNumber)
    {
        return inputNumber % 2 == 0 ? inputNumber / 2 : 3 * inputNumber + 1;
    }
}
