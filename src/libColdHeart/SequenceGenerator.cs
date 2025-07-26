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
        BigInteger currentNumber = inputNumber;

        while (!Sequence.ContainsKey(currentNumber))
        {
            BigInteger nextValue = GetNext(currentNumber);
            Console.WriteLine($"{currentNumber} -> {nextValue}");
            Sequence[currentNumber] = nextValue;
            currentNumber = nextValue;
        }
    }

    private static BigInteger GetNext(BigInteger inputNumber)
    {
        return inputNumber % 2 == 0 ? inputNumber / 2 : 3 * inputNumber + 1;
    }
}
