using System.Numerics;
using System.Buffers.Binary;

public static class HashaMasha {
    public static ulong[] buildLeaves(byte[] data){
        int paddedLength = ((data.Length + 8) / 8) * 8;
        byte[] byteBuffer = new byte[paddedLength];
        data.CopyTo(byteBuffer, 0);
        byteBuffer[data.Length] = 0x80;
        int leafCount = paddedLength / 8;
        int total = (int)BitOperations.RoundUpToPowerOf2((uint)leafCount);
        ulong[] leaves = new ulong[total];
        for (int i = 0; i < leafCount; i++) {
            leaves[i] = BinaryPrimitives.ReadUInt64BigEndian(byteBuffer.AsSpan(i * 8, 8));
        } 
        return leaves;
    }
}