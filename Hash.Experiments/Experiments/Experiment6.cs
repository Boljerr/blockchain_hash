using System.Collections;

namespace Hash.Experiments.Experiments;

public static class Experiment6
{
    public static void Run(string name, Func<byte[], string> hash,  Random random)
    {
        int[] lengths = [10, 100, 500, 1000];
        const int pairsPerLength = 25000;

        var rows = new List<string>
        {
            "function,length,pair,changed_index,bits_different,hex_digits_different,bit_percent,hex_percent"
        };

        foreach (var length in lengths)
        {
            for (int pair = 1; pair <= pairsPerLength; pair++)
            {
                byte[] original = Helpers.Generate(length, random);
                byte[] changed = (byte[])original.Clone();
                
                int changedIndex = random.Next(changed.Length);

                do
                {
                    int alphabetIndex = random.Next(Helpers.Alphabet.Length);
                    changed[changedIndex] = (byte)Helpers.Alphabet[alphabetIndex];
                }
                while (changed[changedIndex] == original[changedIndex]);
                
                string firstHex = hash(original);
                string secondHex = hash(changed);
                
                byte[] firstBytes = Convert.FromHexString(firstHex);
                byte[] secondBytes = Convert.FromHexString(secondHex);
                
                var firstBits = new BitArray(firstBytes);
                var secondBits = new BitArray(secondBytes);

                int differentBits = 0;
                for (int i = 0; i < firstBits.Length; i++)
                {
                    if (firstBits[i] != secondBits[i])
                        differentBits++;
                }

                int differentHexDigits = 0;
                for (int i = 0; i < firstHex.Length; i++)
                {
                    if (firstHex[i] != secondHex[i])
                        differentHexDigits++;
                }
                
                double bitPercent = 100.0 * differentBits / firstBits.Length;
                double hexPercent = 100.0 * differentHexDigits / firstHex.Length;
                rows.Add($"{name},{length},{pair},{changedIndex},{differentBits},{differentHexDigits},{bitPercent.ToString("F6")},{hexPercent.ToString("F6")}");
            }
            
        }
        string outputPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..",
            "Results",
            $"Experiment6_{name}.csv"));

        File.WriteAllLines(outputPath, rows);
        
        Console.WriteLine($"Saved: {outputPath}");
    }
}