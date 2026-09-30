using System.Text.RegularExpressions;

namespace Hash.Experiments.Experiments;

public static class Helpers
{
    public static void AssertValidHash(string hash, int hexLength)
    {
        if (hash.Length != hexLength)
            throw new Exception($"Expected 64 digits, got {hash.Length} :(");
        //from stackoverflow
        if (!Regex.IsMatch(hash, "^[0-9A-F]+$"))
            throw new Exception($"Hash has non hex characters: {hash}");
    }

    public const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public const int Seed = 676767;

    public static byte[] Generate(int length, Random random)
    {
        byte[] bytes = new byte[length];

        for (int i = 0; i < length; i++)
        {
            int randomIndex = random.Next(Alphabet.Length);
            char character = Alphabet[randomIndex];
            byte asciiByte = (byte)character;

            bytes[i] = asciiByte;
        }
        return bytes;
    }

}