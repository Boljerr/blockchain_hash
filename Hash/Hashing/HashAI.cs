using System.Numerics;
using System.Buffers.Binary;

namespace Hash.Hashing;

public class HashAI
{
    private const int BlockSize = 32;
    private const ulong MultiplierA = 14844827832870488419UL;
    private const ulong MultiplierB = 4702101796626366811UL;
    private const ulong MultiplierC = 12760894674763063281UL;
    private const ulong MultiplierD = 3200907817819897205UL;

    public string ComputeHash(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        unchecked
        {
            ulong a = 15036463542186079559UL;
            ulong b = 12483322617323990657UL;
            ulong c = 2658435062210208447UL;
            ulong d = 1587461109643488129UL;

            int offset = 0;
            for (; offset <= data.Length - BlockSize; offset += BlockSize)
            {
                MixBlock(data.AsSpan(offset, BlockSize), ref a, ref b, ref c, ref d);
            }

            if (offset < data.Length)
            {
                Span<byte> tail = stackalloc byte[BlockSize];
                tail.Clear();
                data.AsSpan(offset).CopyTo(tail);
                tail[data.Length - offset] = 0x80;
                MixBlock(tail, ref a, ref b, ref c, ref d);
            }

            ulong length = (ulong)data.Length;
            a ^= length * MultiplierA;
            b ^= BitOperations.RotateLeft(length, 17) * MultiplierB;
            c ^= ~length * MultiplierC;
            d ^= BitOperations.RotateLeft(length, 41) * MultiplierD;

            for (int round = 0; round < 4; round++)
            {
                a = BitOperations.RotateLeft((a ^ d) * MultiplierA + b, 23);
                b = BitOperations.RotateLeft((b ^ a) * MultiplierB + c, 29);
                c = BitOperations.RotateLeft((c ^ b) * MultiplierC + d, 41);
                d = BitOperations.RotateLeft((d ^ c) * MultiplierD + a, 13);
            }

            return $"{a:X16}{b:X16}{c:X16}{d:X16}";
        }
    }

    private static void MixBlock(ReadOnlySpan<byte> block, ref ulong a, ref ulong b, ref ulong c, ref ulong d)
    {
        unchecked
        {
            ulong x0 = BinaryPrimitives.ReadUInt64LittleEndian(block);
            ulong x1 = BinaryPrimitives.ReadUInt64LittleEndian(block[8..]);
            ulong x2 = BinaryPrimitives.ReadUInt64LittleEndian(block[16..]);
            ulong x3 = BinaryPrimitives.ReadUInt64LittleEndian(block[24..]);

            a = BitOperations.RotateLeft((a ^ (x0 + d)) * MultiplierA, 23);
            b = BitOperations.RotateLeft((b ^ (x1 + a)) * MultiplierB, 29);
            c = BitOperations.RotateLeft((c ^ (x2 + b)) * MultiplierC, 41);
            d = BitOperations.RotateLeft((d ^ (x3 + c)) * MultiplierD, 13);
        }
    }
}
