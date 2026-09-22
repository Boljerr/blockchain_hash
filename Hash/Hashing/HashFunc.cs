namespace Hash.Hashing;

public class HashFunc
{
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
            ulong input = unchecked(value + (ulong)i);
            
            a = unchecked(a ^ input + b) ^ d;
            b = unchecked((b + c) ^ a) ^ d;
            c = unchecked(c ^ b + d) ^ a;
            d = unchecked((d ^ c) + a) ^ b;
        }

        return $"{a:X16}{b:X16}{c:X16}{d:X16}";
    }
}