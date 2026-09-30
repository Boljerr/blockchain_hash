namespace Hash.Hashing;
using System.Buffers.Binary;
using System.Numerics;

public class HashaMasha {
    private const int BlockSizeBytes = 32;
    private const int WordSizeBytes = 8;
    private const int LengthSuffixSizeBytes = 8;
    private const byte PaddingMarker = 0x80;
    private const int RotationA = 13;
    private const int RotationB = 29;
    private const int RotationC = 41;
    private const int RotationD = 7;
    private const ulong InitialStateA = 0x243F6A8885A308D3UL;
    private const ulong InitialStateB = 0x13198A2E03707344UL;
    private const ulong InitialStateC = 0xA4093822299F31D0UL;
    private const ulong InitialStateD = 0x082EFA98EC4E6C89UL;

    // One constant per round (32 rounds), so no two rounds are the same and an all-zero state does not stay zero.
    // The first 32 SHA-512 round constants: fractional parts of the cube roots of the first 32 primes.
    private static readonly ulong[] RoundConstants = {
        0x428A2F98D728AE22UL, 0x7137449123EF65CDUL, 0xB5C0FBCFEC4D3B2FUL, 0xE9B5DBA58189DBBCUL,
        0x3956C25BF348B538UL, 0x59F111F1B605D019UL, 0x923F82A4AF194F9BUL, 0xAB1C5ED5DA6D8118UL,
        0xD807AA98A3030242UL, 0x12835B0145706FBEUL, 0x243185BE4EE4B28CUL, 0x550C7DC3D5FFB4E2UL,
        0x72BE5D74F27B896FUL, 0x80DEB1FE3B1696B1UL, 0x9BDC06A725C71235UL, 0xC19BF174CF692694UL,
        0xE49B69C19EF14AD2UL, 0xEFBE4786384F25E3UL, 0x0FC19DC68B8CD5B5UL, 0x240CA1CC77AC9C65UL,
        0x2DE92C6F592B0275UL, 0x4A7484AA6EA6E483UL, 0x5CB0A9DCBD41FBD4UL, 0x76F988DA831153B5UL,
        0x983E5152EE66DFABUL, 0xA831C66D2DB43210UL, 0xB00327C898FB213FUL, 0xBF597FC7BEEF0EE4UL,
        0xC6E00BF33DA88FC2UL, 0xD5A79147930AA725UL, 0x06CA6351E003826FUL, 0x142929670A0E6E70UL,
    };

    public string ComputeHash(ReadOnlySpan<byte> message) {
        ulong stateA = InitialStateA;
        ulong stateB = InitialStateB;
        ulong stateC = InitialStateC;
        ulong stateD = InitialStateD;
        int messageLength = message.Length;

        // Full blocks are read straight from the message instead of copying it
        int fullBlocksLength = messageLength - messageLength % BlockSizeBytes;
        for (int blockOffset = 0; blockOffset < fullBlocksLength; blockOffset += BlockSizeBytes){
            ProcessBlock(message.Slice(blockOffset, BlockSizeBytes), ref stateA, ref stateB, ref stateC, ref stateD);
        }

        // Only the leftover bytes, the padding marker and the length are built in a small buffer (1 or 2 blocks)
        ReadOnlySpan<byte> leftover = message.Slice(fullBlocksLength);
        int tailLength = leftover.Length + 1 + LengthSuffixSizeBytes <= BlockSizeBytes ? BlockSizeBytes : 2 * BlockSizeBytes;
        Span<byte> tail = stackalloc byte[2 * BlockSizeBytes];
        tail = tail.Slice(0, tailLength);
        tail.Clear();
        leftover.CopyTo(tail);
        tail[leftover.Length] = PaddingMarker;
        BinaryPrimitives.WriteUInt64BigEndian(tail.Slice(tailLength - LengthSuffixSizeBytes), (ulong)messageLength);
        for (int blockOffset = 0; blockOffset < tailLength; blockOffset += BlockSizeBytes){
            ProcessBlock(tail.Slice(blockOffset, BlockSizeBytes), ref stateA, ref stateB, ref stateC, ref stateD);
        }

        Span<byte> outputBytes = stackalloc byte[BlockSizeBytes];
        BinaryPrimitives.WriteUInt64BigEndian(outputBytes.Slice(0 * WordSizeBytes, WordSizeBytes), stateA);
        BinaryPrimitives.WriteUInt64BigEndian(outputBytes.Slice(1 * WordSizeBytes, WordSizeBytes), stateB);
        BinaryPrimitives.WriteUInt64BigEndian(outputBytes.Slice(2 * WordSizeBytes, WordSizeBytes), stateC);
        BinaryPrimitives.WriteUInt64BigEndian(outputBytes.Slice(3 * WordSizeBytes, WordSizeBytes), stateD);
        return Convert.ToHexString(outputBytes);
    }

    private static void ProcessBlock(ReadOnlySpan<byte> block, ref ulong stateA, ref ulong stateB, ref ulong stateC,
        ref ulong stateD) {
        ulong a = stateA ^ BinaryPrimitives.ReadUInt64BigEndian(block.Slice(0 * WordSizeBytes, WordSizeBytes));
        ulong b = stateB ^ BinaryPrimitives.ReadUInt64BigEndian(block.Slice(1 * WordSizeBytes, WordSizeBytes));
        ulong c = stateC ^ BinaryPrimitives.ReadUInt64BigEndian(block.Slice(2 * WordSizeBytes, WordSizeBytes));
        ulong d = stateD ^ BinaryPrimitives.ReadUInt64BigEndian(block.Slice(3 * WordSizeBytes, WordSizeBytes));
        for (int round = 0; round < RoundConstants.Length; round++){
            a ^= RoundConstants[round];
            a += b;
            a = BitOperations.RotateLeft(a, RotationA) ^ c;
            b += c;
            b = BitOperations.RotateLeft(b, RotationB) ^ d;
            c += d;
            c = BitOperations.RotateLeft(c, RotationC) ^ a;
            d += a;
            d = BitOperations.RotateLeft(d, RotationD) ^ b;
        }
        stateA = a;
        stateB = b;
        stateC = c;
        stateD = d;
    }
}
