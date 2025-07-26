using System.Collections.Generic;
using System.Numerics;

namespace libColdHeart;

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
        TreeNode? existingNode;
        while (!_nodeMap.TryGetValue(currentNumber, out existingNode))
        {
            path.Add(currentNumber);
            currentNumber = GetNext(currentNumber);
        }

        // Now add nodes to tree in reverse order (from known node back to input)
        TreeNode? knownNode = existingNode;

        for (int i = path.Count - 1; i >= 0; i--)
        {
            BigInteger nodeValue = path[i];

            if (!_nodeMap.TryGetValue(nodeValue, out TreeNode? existingNodeValue))
            {
                TreeNode newNode = new TreeNode(nodeValue);
                _nodeMap[nodeValue] = newNode;

                // Add as child to the previous node in the reverse path
                AddChildToParent(knownNode, newNode);
                knownNode = newNode;
            }
            else
            {
                knownNode = existingNodeValue;
            }
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
