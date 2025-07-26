using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text.Json;
using System.Threading.Tasks;

namespace libColdHeart;

public class SequenceGenerator
{
    public TreeNode Root { get; private set; }
    private readonly Dictionary<BigInteger, TreeNode> _nodeMap;

    public SequenceGenerator()
    {
        Root = new TreeNode(1);
        _nodeMap = new Dictionary<BigInteger, TreeNode> { { 1, Root } };
    }

    public SequenceGenerator(TreeNode root)
    {
        Root = root;
        _nodeMap = new Dictionary<BigInteger, TreeNode>();
        BuildNodeMap(Root);
    }

    private void BuildNodeMap(TreeNode? node)
    {
        if (node == null) return;
        
        _nodeMap[node.Value] = node;
        BuildNodeMap(node.LeftChild);
        BuildNodeMap(node.RightChild);
    }

    public void Add(BigInteger inputNumber)
    {
        if (inputNumber == 1)
        {
            return; // Root already exists
        }

        BuildPathToRoot(inputNumber);
    }

    public async Task SaveToFileAsync(string filePath)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            MaxDepth = 512,
            Converters = { new BigIntegerConverter() }
        };
        
        var json = JsonSerializer.Serialize(Root, options);
        await File.WriteAllTextAsync(filePath, json);
    }
    
    public static async Task<SequenceGenerator> LoadFromFileAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        var options = new JsonSerializerOptions
        {
            MaxDepth = 512,
            Converters = { new BigIntegerConverter() }
        };
        
        var root = JsonSerializer.Deserialize<TreeNode>(json, options);
        if (root == null)
        {
            throw new InvalidOperationException("Failed to deserialize sequence from file");
        }
        
        return new SequenceGenerator(root);
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
        else
        {
            throw new InvalidOperationException($"Cannot add child {child.Value} to parent {parent.Value}: both child positions are already occupied by {parent.LeftChild.Value} and {parent.RightChild.Value}. This indicates a logic error in the tree building algorithm.");
        }
    }

    private static BigInteger GetNext(BigInteger inputNumber)
    {
        return inputNumber % 2 == 0 ? inputNumber / 2 : 3 * inputNumber + 1;
    }
}
