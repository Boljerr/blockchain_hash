namespace Hash.Hashing;

public class HashFunc
{
    private const ulong MultiplierA = 14844827832870488419UL;
    private const ulong MultiplierB = 4702101796626366811UL;
    private const ulong MultiplierC = 12760894674763063281UL;
    private const ulong MultiplierD = 3200907817819897205UL;
    private const ulong MultiplierIndex = 11132683126343784489UL;
    public string ComputeHash(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        
        ulong a = 15036463542186079559UL;
        ulong b = 12483322617323990657UL;
        ulong c = 2658435062210208447UL;
        ulong d = 1587461109643488129UL;
        
        for (int i = 0; i < data.Length; i++)
        {   
            byte value = data[i];
            ulong input = unchecked((value + (ulong)i) * MultiplierIndex);
            
            a = unchecked(a ^ (input + b) * MultiplierA) ^ d;
            b = unchecked(((b + c) ^ a) * MultiplierB) ^ d;
            c = unchecked((c ^ b) * MultiplierC + d) ^ a;
            d = unchecked(((d ^ c) + a) * MultiplierD) ^ b;
        }
        ulong length = (ulong)data.Length;
        a = unchecked(a ^ b * length) ^ c;
        d = unchecked(((b + c) ^ a) * MultiplierC) ^ c;
        c = unchecked((c ^ d) * MultiplierD) ^ a;
        b = unchecked(((d + b) ^ a) * MultiplierA) ^ c;
        a = unchecked(((a + d) ^ b) * MultiplierB) ^ d;

        return $"{a:X16}{b:X16}{c:X16}{d:X16}";
    }
}