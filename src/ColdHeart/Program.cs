using System.Numerics;
using libColdHeart;

namespace ColdHeart;

internal class Program
{
    private const int UPPER_LIMIT = 1000000;

    static void Main(System.String[] args)
    {
        var generator = new SequenceGenerator();
        for (BigInteger i = 1; i < UPPER_LIMIT; i++)
        {
            generator.Add(i);
        }
    }
}
