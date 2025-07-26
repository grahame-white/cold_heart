using System.Numerics;
using libColdHeart;

namespace ColdHeart;

internal class Program
{
    static void Main(System.String[] args)
    {
        var generator = new SequenceGenerator();
        for (BigInteger i = 0; i < 1000000; i++)
        {
            generator.Add(i);
        }
    }
}
