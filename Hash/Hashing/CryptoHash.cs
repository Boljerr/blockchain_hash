namespace Hash.Hashing;
using System.Buffers.Binary;
using System.Numerics;

public class HashaMasha {
    private const int BlockSizeBytes = 32;
    private const int WordSizeBytes = 8;
    private const int LengthSuffixSizeBytes = 8;
    private const byte PaddingMarker = 0x80;
    private const ulong InitialStateA = 0x243F6A8885A308D3UL;
    private const ulong InitialStateB = 0x13198A2E03707344UL;
    private const ulong InitialStateC = 0xA4093822299F31D0UL;
    private const ulong InitialStateD = 0x082EFA98EC4E6C89UL;

    public static string ComputeHash(ReadOnlySpan<byte> message) {
        ulong stateA = InitialStateA;
        ulong stateB = InitialStateB;
        ulong stateC = InitialStateC;
        ulong stateD = InitialStateD;
        int messageLength = message.Length;
        int zeroPaddingLength = (BlockSizeBytes - ((messageLength + 9) % BlockSizeBytes)) % BlockSizeBytes;
        int totalPaddedLength = messageLength + 1 + zeroPaddingLength + LengthSuffixSizeBytes;
        byte[] paddedMessage = new byte[totalPaddedLength];
        message.CopyTo(paddedMessage);
        paddedMessage[messageLength] = PaddingMarker;
        int lengthSuffixOffset = totalPaddedLength - LengthSuffixSizeBytes;
        BinaryPrimitives.WriteUInt64BigEndian(paddedMessage.AsSpan(lengthSuffixOffset), (ulong)messageLength);
        for (int blockOffset = 0; blockOffset < paddedMessage.Length; blockOffset += BlockSizeBytes){
            ReadOnlySpan<byte> currentBlock = paddedMessage.AsSpan(blockOffset, BlockSizeBytes);
            stateA ^= BinaryPrimitives.ReadUInt64BigEndian(currentBlock.Slice(0 * WordSizeBytes, WordSizeBytes));
            stateB ^= BinaryPrimitives.ReadUInt64BigEndian(currentBlock.Slice(1 * WordSizeBytes, WordSizeBytes));
            stateC ^= BinaryPrimitives.ReadUInt64BigEndian(currentBlock.Slice(2 * WordSizeBytes, WordSizeBytes));
            stateD ^= BinaryPrimitives.ReadUInt64BigEndian(currentBlock.Slice(3 * WordSizeBytes, WordSizeBytes));
            for (int round = 0; round < 32; round++){
                stateA += stateB;
                stateA = BitOperations.RotateLeft(stateA, 13) ^ stateC;
                stateB += stateC;
                stateB = BitOperations.RotateLeft(stateB, 13) ^ stateD;
                stateC += stateD;
                stateC = BitOperations.RotateLeft(stateC, 13) ^ stateA;
                stateD += stateA;
                stateD = BitOperations.RotateLeft(stateD, 13) ^ stateB;
            }
        }
        byte[] outputBytes = new byte[BlockSizeBytes];
        BinaryPrimitives.WriteUInt64BigEndian(outputBytes.AsSpan(0 * WordSizeBytes, WordSizeBytes), stateA);
        BinaryPrimitives.WriteUInt64BigEndian(outputBytes.AsSpan(1 * WordSizeBytes, WordSizeBytes), stateB);
        BinaryPrimitives.WriteUInt64BigEndian(outputBytes.AsSpan(2 * WordSizeBytes, WordSizeBytes), stateC);
        BinaryPrimitives.WriteUInt64BigEndian(outputBytes.AsSpan(3 * WordSizeBytes, WordSizeBytes), stateD);
        return Convert.ToHexString(outputBytes);
    }
}