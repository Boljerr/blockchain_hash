using static System.Buffers.Binary.BinaryPrimitives;

namespace Hash.Hashing;

public class HashFunc
{
    public string ComputeHash(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        ulong a = 1;
        ulong b = 1;
        ulong c = 1;
        ulong d = 1;

        foreach (byte value in data)
        {
            a = unchecked(a * value);
            b = unchecked(b * a) ^ a;
            c = unchecked(c ^ a + b);
            d = unchecked(d * b) ^ c;
        }

        return $"{a:X16}{b:X16}{c:X16}{d:X16}";
    }
}