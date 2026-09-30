using System.Text;

namespace Hash.Experiments.Experiments;

public static class Experiment5
{
    private static readonly int[] Lengths = { 10, 100, 500, 1000 };
    private const int PairsPerLength = 100_000;

    public static void Run(string name, Func<byte[], string> hash, int seed)
    {
        Console.WriteLine($"=== Experiment 5 ({name}): collisions ===");
        Console.WriteLine($"Alphabet ({Helpers.Alphabet.Length} symbols, 1 ASCII byte each): {Helpers.Alphabet}");
        Console.WriteLine($"Seed: {seed}");

        var random = new Random(seed);
        var rows = new List<string>
        {
            "function,input_set,seed,pairs,pair_collisions,inputs,distinct_inputs,collision_groups"
        };
        var examples = new List<string>();

        foreach (int length in Lengths)
        {
            Helpers.AssertValidHash(hash(Helpers.Generate(length, new Random(seed))));

            // hash -> distinct inputs that produced it. Identical inputs always share a hash,
            // so inputs only need to be compared within one bucket.
            var buckets = new Dictionary<string, List<byte[]>>();
            int pairCollisions = 0;

            for (int i = 0; i < PairsPerLength; i++)
            {
                byte[] a = Helpers.Generate(length, random);
                byte[] b = Helpers.Generate(length, random);
                while (a.AsSpan().SequenceEqual(b)) // the two inputs of a pair must differ
                    b = Helpers.Generate(length, random);

                string hashA = hash(a);
                string hashB = hash(b);

                if (hashA == hashB)
                {
                    pairCollisions++;
                    examples.Add($"[pair, length {length}] hash {hashA}:");
                    examples.Add("    " + Describe(a));
                    examples.Add("    " + Describe(b));
                }

                Add(buckets, hashA, a);
                Add(buckets, hashB, b);
            }

            var (distinct, groups) = Summarize(buckets, $"whole set, length {length}", examples);

            Console.WriteLine($"Length {length}: pairs {pairCollisions}/{PairsPerLength} collisions; " +
                              $"whole set {2 * PairsPerLength} inputs, {distinct} distinct, {groups} collision groups");
            rows.Add($"{name},length {length},{seed},{PairsPerLength},{pairCollisions},{2 * PairsPerLength},{distinct},{groups}");
        }

        // Structured inputs: weaknesses random strings are unlikely to hit
        var structured = BuildStructuredInputs();
        var structuredBuckets = new Dictionary<string, List<byte[]>>();
        foreach (var input in structured)
            Add(structuredBuckets, hash(input), input);

        var (structuredDistinct, structuredGroups) = Summarize(structuredBuckets, "structured", examples);

        Console.WriteLine($"Structured: {structured.Count} inputs, {structuredDistinct} distinct, " +
                          $"{structuredGroups} collision groups");
        rows.Add($"{name},structured,-,0,0,{structured.Count},{structuredDistinct},{structuredGroups}");

        string resultsDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Results"));

        string outputPath = Path.Combine(resultsDir, $"Experiment5_{name}.csv");
        File.WriteAllLines(outputPath, rows);
        Console.WriteLine($"Saved: {outputPath}");

        if (examples.Count > 0)
        {
            string examplesPath = Path.Combine(resultsDir, $"Experiment5_{name}_collisions.txt");
            File.WriteAllLines(examplesPath, examples);
            Console.WriteLine($"Collisions found, examples saved: {examplesPath}");
        }
    }

    private static void Add(Dictionary<string, List<byte[]>> buckets, string hash, byte[] input)
    {
        if (!buckets.TryGetValue(hash, out var inputs))
            buckets[hash] = new List<byte[]> { input };
        else if (!inputs.Any(x => x.AsSpan().SequenceEqual(input)))
            inputs.Add(input);
    }

    /// <summary>Counts distinct inputs and hashes shared by 2+ distinct inputs; records the latter as examples.</summary>
    private static (int Distinct, int Groups) Summarize(
        Dictionary<string, List<byte[]>> buckets, string set, List<string> examples)
    {
        int distinct = 0;
        int groups = 0;

        foreach (var (hash, inputs) in buckets)
        {
            distinct += inputs.Count;
            if (inputs.Count < 2) continue;

            groups++;
            examples.Add($"[{set}] hash {hash} shared by {inputs.Count} inputs:");
            examples.AddRange(inputs.Select(x => "    " + Describe(x)));
        }
        return (distinct, groups);
    }

    private static List<byte[]> BuildStructuredInputs()
    {
        var inputs = new List<byte[]>();
        byte[] Ascii(string s) => Encoding.ASCII.GetBytes(s);

        // Permutations: same bytes in every order
        foreach (var p in Permutations("abcde".ToArray()))
            inputs.Add(Ascii(new string(p.ToArray())));
        foreach (var p in Permutations(new[] { "alpha", "beta", "gamma", "delta" }))
            inputs.Add(Ascii(string.Join(" ", p)));

        // Swapping each pair of neighbouring characters
        byte[] text = Ascii("The quick brown fox jumps over the lazy dog");
        for (int i = 0; i < text.Length - 1; i++)
        {
            byte[] swapped = (byte[])text.Clone();
            (swapped[i], swapped[i + 1]) = (swapped[i + 1], swapped[i]);
            inputs.Add(swapped);
        }

        // Repeating patterns of growing length
        foreach (string pattern in new[] { "a", "ab", "abc", "0" })
            for (int n = 1; n <= 64; n++)
                inputs.Add(Ascii(string.Concat(Enumerable.Repeat(pattern, n))));
        for (int n = 0; n <= 64; n++)
        {
            inputs.Add(new byte[n]);
            inputs.Add(Enumerable.Repeat((byte)0xFF, n).ToArray());
        }

        // Single bit flips in one 32-byte block of zeros
        for (int bit = 0; bit < 32 * 8; bit++)
        {
            byte[] block = new byte[32];
            block[bit / 8] ^= (byte)(1 << (bit % 8));
            inputs.Add(block);
        }

        // Trailing bytes that resemble padding
        inputs.Add(Ascii("abc"));
        inputs.Add(Ascii("abc\0"));
        inputs.Add(Ascii("abc\0\0"));
        inputs.Add(new byte[] { (byte)'a', (byte)'b', (byte)'c', 0x80 });
        inputs.Add(new byte[] { (byte)'a', (byte)'b', (byte)'c', 0x80, 0x00 });

        return inputs;
    }

    private static IEnumerable<List<T>> Permutations<T>(IReadOnlyList<T> items)
    {
        if (items.Count <= 1)
        {
            yield return items.ToList();
            yield break;
        }

        for (int i = 0; i < items.Count; i++)
        {
            var rest = items.Where((_, j) => j != i).ToList();
            foreach (var p in Permutations(rest))
            {
                p.Insert(0, items[i]);
                yield return p;
            }
        }
    }

    private static string Describe(byte[] input)
    {
        bool printable = input.All(b => b >= 32 && b < 127);
        return printable
            ? $"\"{Encoding.ASCII.GetString(input)}\" ({input.Length} B)"
            : $"hex {Convert.ToHexString(input)} ({input.Length} B)";
    }
}
