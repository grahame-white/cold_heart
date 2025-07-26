using System;
using System.Collections.Generic;
using System.Numerics;

namespace libColdHeart;

public class SequenceGenerator
{
    // Key, next value
    public Dictionary<BigInteger, BigInteger> Sequence { get; }

    public SequenceGenerator()
    {
        Sequence = [];
    }

    public void Add(BigInteger inputNumber)
    {
        if (Sequence.ContainsKey(inputNumber)) return;

        BigInteger nextValue = GetNext(inputNumber);
        Console.WriteLine($"{inputNumber} -> {nextValue}");
        Sequence[inputNumber] = nextValue;

        // Recursively add the next value
        Add(nextValue); 
    }

    private static BigInteger GetNext(BigInteger inputNumber)
    {
        if (inputNumber % 2 == 0)
        {
            return inputNumber / 2;
        }
        else
        {
            return 3 * inputNumber + 1;
        }
    }
}
