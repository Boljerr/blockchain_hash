using System.Text.RegularExpressions;

namespace Hash.Experiments.Experiments;

public static class HashChecks
{
    public static void AssertValidHash(string hash)
    {
        if (hash.Length != 64)
            throw new Exception($"Expected 64 digits, got {hash.Length} :(");
        //from stackoverflow
        if (!Regex.IsMatch(hash, "^[0-9A-F]+$"))
            throw new Exception($"Hash has non hex characters: {hash}");
    }
}