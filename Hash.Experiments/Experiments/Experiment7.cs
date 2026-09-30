using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Hash.Experiments.Experiments;

public static class Experiment7
{
    private const int SaltLength = 16;   // bytes, public
    private const int SecretLength = 16; // bytes, secret r
    private const int ReuseTargets = 5;

    private record Match(byte[] Input, byte[] Suffix);

    private record AttackResult(long Attempts, long FirstMatch, List<Match> Matches, double ElapsedMs);

    public static void Run(string name, Func<byte[], string> hash, int seed)
    {
        Console.WriteLine($"=== Experiment 7 ({name}): guessing, public salt and secret randomness ===");

        var random = new Random(seed);
        var candidates = Enumerable.Range(0, 10_000)
            .Select(i => Encoding.ASCII.GetBytes(i.ToString("D4", CultureInfo.InvariantCulture)))
            .ToList();
        int targetIndex = random.Next(candidates.Count);
        byte[] target = candidates[targetIndex];

        Console.WriteLine($"Candidates: {candidates.Count} strings \"0000\"..\"9999\", 4 ASCII bytes each");
        Console.WriteLine($"Seed: {seed}; chosen target (never passed to the attack): {Text(target)}");

        Helpers.AssertValidHash(hash(target));
        foreach (var candidate in candidates) // warm-up, so JIT compilation is not timed
            hash(candidate);

        var rows = new List<string> { "function,scenario,targets,attempts,elapsed_ms,cracked,found" };
        byte[] noSuffix = Array.Empty<byte>();

        // 1. No salt
        Console.WriteLine();
        Console.WriteLine("1. No salt: H(input)");
        string plainHash = hash(target);
        Console.WriteLine($"Hash given to the attack: {plainHash}");
        var plain = BruteForce(hash, plainHash, candidates, new[] { noSuffix });
        Report(plain, target);
        rows.Add(Row(name, "no_salt", 1, plain.Attempts, plain.ElapsedMs, IsTarget(plain, target) ? 1 : 0, Found(plain)));

        var timer = Stopwatch.StartNew();
        var plainTable = BuildTable(hash, candidates, noSuffix);
        timer.Stop();
        double plainTableMs = timer.Elapsed.TotalMilliseconds;
        Console.WriteLine($"Distinct hashes among all candidates: {plainTable.Count}/{candidates.Count}");

        // 2. Public salt
        Console.WriteLine();
        Console.WriteLine("2. Public salt: H(input || salt)");
        byte[] salt = RandomBytes(SaltLength, random);
        Console.WriteLine($"Salt: {SaltLength} random bytes appended as raw bytes to the 4 input bytes " +
                          $"({4 + SaltLength}-byte message), written as hex: {Convert.ToHexString(salt)}");
        string saltedHash = hash(Concat(target, salt));
        Console.WriteLine($"Hash given to the attack: {saltedHash}");
        var salted = BruteForce(hash, saltedHash, candidates, new[] { salt });
        Report(salted, target);
        rows.Add(Row(name, "public_salt", 1, salted.Attempts, salted.ElapsedMs, IsTarget(salted, target) ? 1 : 0, Found(salted)));

        Console.WriteLine();
        Console.WriteLine($"Reusing precomputed hashes for {ReuseTargets} more targets, each with its own salt:");
        var targets = Enumerable.Range(0, ReuseTargets).Select(_ => candidates[random.Next(candidates.Count)]).ToList();
        var salts = targets.Select(_ => RandomBytes(SaltLength, random)).ToList();
        var plainHashes = targets.Select(hash).ToList();
        var saltedHashes = targets.Select((t, i) => hash(Concat(t, salts[i]))).ToList();

        // Without salt the table from part 1 answers every target with a lookup
        var plainFound = plainHashes.Select(h => plainTable.GetValueOrDefault(h)).ToList();
        int plainCracked = CountCracked(plainFound, targets);
        Console.WriteLine($"No salt: the table from part 1 ({candidates.Count} hashes, {plainTableMs:F3} ms) " +
                          $"cracks {plainCracked}/{ReuseTargets} targets with lookups only");
        rows.Add(Row(name, "reuse_no_salt_table", ReuseTargets, candidates.Count, plainTableMs, plainCracked, Found(plainFound)));

        // A table built for one salt is useless for every other salt
        timer.Restart();
        var firstSaltTable = BuildTable(hash, candidates, salts[0]);
        var firstSaltFound = saltedHashes.Select(h => firstSaltTable.GetValueOrDefault(h)).ToList();
        timer.Stop();
        int firstSaltCracked = CountCracked(firstSaltFound, targets);
        Console.WriteLine($"Salted: a table for the first target's salt ({candidates.Count} hashes, " +
                          $"{timer.Elapsed.TotalMilliseconds:F3} ms) cracks {firstSaltCracked}/{ReuseTargets} targets");
        rows.Add(Row(name, "reuse_first_salt_table", ReuseTargets, candidates.Count, timer.Elapsed.TotalMilliseconds,
            firstSaltCracked, Found(firstSaltFound)));

        // So every salt needs its own search
        timer.Restart();
        var perSalt = targets.Select((_, i) => BruteForce(hash, saltedHashes[i], candidates, new[] { salts[i] })).ToList();
        timer.Stop();
        var perSaltFound = perSalt.Select(r => r.Matches.Count == 1 ? r.Matches[0].Input : null).ToList();
        int perSaltCracked = CountCracked(perSaltFound, targets);
        long perSaltAttempts = perSalt.Sum(r => r.Attempts);
        Console.WriteLine($"Salted: a separate search for every salt ({perSaltAttempts} hashes, " +
                          $"{timer.Elapsed.TotalMilliseconds:F3} ms) cracks {perSaltCracked}/{ReuseTargets} targets");
        rows.Add(Row(name, "reuse_search_per_salt", ReuseTargets, perSaltAttempts, timer.Elapsed.TotalMilliseconds,
            perSaltCracked, Found(perSaltFound)));

        // 3. Secret randomness
        Console.WriteLine();
        Console.WriteLine("3. Secret randomness: H(input || r)");
        byte[] secret = RandomNumberGenerator.GetBytes(SecretLength);
        string commitment = hash(Concat(target, secret));
        Console.WriteLine($"r: {SecretLength} bytes from RandomNumberGenerator, appended like the salt but kept secret; " +
                          $"published hash: {commitment}");

        // With a 1-byte r every (candidate, r) pair can still be tried
        byte[] toySecret = RandomNumberGenerator.GetBytes(1);
        var everyToySecret = Enumerable.Range(0, 256).Select(b => new[] { (byte)b }).ToList();
        Console.WriteLine("Toy r of 1 byte, every (candidate, r) pair tried:");
        var toy = BruteForce(hash, hash(Concat(target, toySecret)), candidates, everyToySecret);
        Report(toy, target, withSuffix: true);
        rows.Add(Row(name, "secret_r_1_byte", 1, toy.Attempts, toy.ElapsedMs, IsTarget(toy, target) ? 1 : 0,
            Found(toy, withSuffix: true)));

        double rate = toy.Attempts / (toy.ElapsedMs / 1000);
        Console.WriteLine($"Before r is revealed: {candidates.Count} candidates x 256^(bytes of r) guesses, " +
                          $"at {rate:F0} hashes/s:");
        foreach (int length in new[] { 1, 2, 4, 8, SecretLength })
        {
            double space = candidates.Count * Math.Pow(256, length);
            Console.WriteLine($"  r of {length,2} bytes: {space:G3} guesses, about {FormatDuration(space / rate)}");
        }
        double fullSpace = candidates.Count * Math.Pow(256, SecretLength);
        rows.Add(Row(name, $"secret_r_{SecretLength}_bytes_estimate", 1, fullSpace, fullSpace / rate * 1000, 0, "not_run"));

        Console.WriteLine("After r is revealed:");
        byte[] other = candidates[(targetIndex + 1) % candidates.Count];
        Console.WriteLine($"Opening ({Text(target)}, r) matches the published hash: {hash(Concat(target, secret)) == commitment}");
        Console.WriteLine($"Opening ({Text(other)}, r) matches the published hash: {hash(Concat(other, secret)) == commitment}");
        Console.WriteLine("If only r is revealed, the input falls to the same search as with a public salt:");
        var revealed = BruteForce(hash, commitment, candidates, new[] { secret });
        Report(revealed, target);
        rows.Add(Row(name, "secret_r_revealed", 1, revealed.Attempts, revealed.ElapsedMs, IsTarget(revealed, target) ? 1 : 0,
            Found(revealed)));

        string outputPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..",
            "Results",
            $"Experiment7_{name}.csv"));

        File.WriteAllLines(outputPath, rows);
        Console.WriteLine($"Saved: {outputPath}");
    }

    /// <summary>
    /// The attack. It gets the hash to invert, the public candidates and the suffixes to try
    /// (none, the public salt, or every possible r), never the chosen input. It checks every
    /// candidate instead of stopping at the first match, so all matches are found.
    /// </summary>
    private static AttackResult BruteForce(Func<byte[], string> hash, string targetHash,
        IReadOnlyList<byte[]> candidates, IReadOnlyList<byte[]> suffixes)
    {
        var matches = new List<Match>();
        long attempts = 0;
        long firstMatch = 0;
        var timer = Stopwatch.StartNew();

        foreach (var suffix in suffixes)
        {
            foreach (var candidate in candidates)
            {
                attempts++;
                if (hash(Concat(candidate, suffix)) != targetHash) continue;

                matches.Add(new Match(candidate, suffix));
                if (firstMatch == 0) firstMatch = attempts;
            }
        }

        timer.Stop();
        return new AttackResult(attempts, firstMatch, matches, timer.Elapsed.TotalMilliseconds);
    }

    /// <summary>Precomputes hash -> candidate for one suffix, so any later hash with that suffix is a lookup.</summary>
    private static Dictionary<string, byte[]> BuildTable(Func<byte[], string> hash,
        IReadOnlyList<byte[]> candidates, byte[] suffix)
    {
        var table = new Dictionary<string, byte[]>();
        foreach (var candidate in candidates)
            table.TryAdd(hash(Concat(candidate, suffix)), candidate);
        return table;
    }

    /// <summary>input || suffix as plain byte concatenation.</summary>
    private static byte[] Concat(byte[] input, byte[] suffix)
    {
        byte[] message = new byte[input.Length + suffix.Length];
        input.CopyTo(message, 0);
        suffix.CopyTo(message, input.Length);
        return message;
    }

    private static bool IsTarget(AttackResult result, byte[] target) =>
        result.Matches.Count == 1 && result.Matches[0].Input.AsSpan().SequenceEqual(target);

    private static int CountCracked(IReadOnlyList<byte[]?> found, IReadOnlyList<byte[]> targets) =>
        Enumerable.Range(0, targets.Count)
            .Count(i => found[i] is { } input && input.AsSpan().SequenceEqual(targets[i]));

    private static void Report(AttackResult result, byte[] target, bool withSuffix = false)
    {
        double rate = result.Attempts / (result.ElapsedMs / 1000);
        string first = result.FirstMatch == 0 ? "no match" : $"first match after {result.FirstMatch}";
        Console.WriteLine($"Attempts: {result.Attempts} ({first}), time {result.ElapsedMs:F3} ms, {rate:F0} hashes/s");
        Console.WriteLine($"Matching candidates: {Found(result, withSuffix)}; " +
                          $"single match equal to the target: {(IsTarget(result, target) ? "yes" : "no")}");
    }

    private static string Found(AttackResult result, bool withSuffix = false) =>
        result.Matches.Count == 0
            ? "none"
            : string.Join(" ", result.Matches.Select(m =>
                withSuffix ? $"{Text(m.Input)}||{Convert.ToHexString(m.Suffix)}" : Text(m.Input)));

    private static string Found(IEnumerable<byte[]?> inputs) =>
        string.Join(" ", inputs.Select(input => input is null ? "-" : Text(input)));

    private static string Row(string name, string scenario, int targets, double attempts, double elapsedMs,
        int cracked, string found) =>
        string.Create(CultureInfo.InvariantCulture,
            $"{name},{scenario},{targets},{attempts},{elapsedMs:G6},{cracked},{found}");

    private static byte[] RandomBytes(int length, Random random)
    {
        byte[] bytes = new byte[length];
        random.NextBytes(bytes);
        return bytes;
    }

    private static string Text(byte[] input) => Encoding.ASCII.GetString(input);

    private static string FormatDuration(double seconds) => seconds switch
    {
        < 1 => $"{seconds * 1000:F1} ms",
        < 60 => $"{seconds:F1} s",
        < 3600 => $"{seconds / 60:F1} min",
        < 86400 => $"{seconds / 3600:F1} h",
        < 86400 * 365.0 => $"{seconds / 86400:F1} days",
        _ => $"{seconds / (86400 * 365.0):G3} years"
    };
}
