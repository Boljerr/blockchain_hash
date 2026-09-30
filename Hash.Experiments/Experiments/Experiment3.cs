using System.Diagnostics;
using System.Reflection;
using System.Text;
using Hash.Input;

namespace Hash.Experiments.Experiments;

public static class Experiment3
{
    public const string ChildArgument = "--experiment3-child";
    private const int Repeats = 5;
    private const int SeparateProcesses = 3;

    public static void Run(string functionName, Func<byte[], string> hash)
    {
        Console.WriteLine($"=== Experiment 3 ({functionName}): determinism and hidden state ===");

        var inputs = BuildInputs();
        var problems = new List<string>();
        var reference = new Dictionary<string, string>();

        // 1. Repeated hashing of the same input
        int repeatFailures = 0;
        foreach (var (name, data) in inputs)
        {
            var first = Compute(hash, name, data, problems);
            if (first is null) continue;
            reference[name] = first;

            for (int i = 1; i < Repeats; i++)
            {
                var again = Compute(hash, name, data, problems);
                if (again is not null && again != first)
                {
                    repeatFailures++;
                    problems.Add($"[repeat] '{name}': call 1 = {Short(first)}, call {i + 1} = {Short(again)}");
                }
            }
        }
        Report("Repeated calls", repeatFailures);

        // 2. Interleaved sequences X, Y, X for every ordered pair of inputs
        int sequenceFailures = 0;
        foreach (var (nameX, dataX) in inputs)
        {
            if (!reference.TryGetValue(nameX, out var expected)) continue;

            foreach (var (nameY, dataY) in inputs)
            {
                if (nameX == nameY) continue;

                var x1 = Compute(hash, nameX, dataX, problems);
                Compute(hash, nameY, dataY, problems);
                var x2 = Compute(hash, nameX, dataX, problems);

                if (x1 != expected || x2 != expected)
                {
                    sequenceFailures++;
                    problems.Add(
                        $"[sequence] {nameX}, {nameY}, {nameX}: expected {Short(expected)}, " +
                        $"got {Short(x1)} then {Short(x2)}");
                }
            }
        }

        // Whole set once more in reverse order
        foreach (var (name, data) in Enumerable.Reverse(inputs))
        {
            if (!reference.TryGetValue(name, out var expected)) continue;
            var h = Compute(hash, name, data, problems);
            if (h != expected)
            {
                sequenceFailures++;
                problems.Add($"[reverse pass] '{name}': expected {Short(expected)}, got {Short(h)}");
            }
        }
        Report("Interleaved sequences (A, B, A)", sequenceFailures);

        // 3. The same inputs hashed by new processes of this program
        CompareWithSeparateProcesses(functionName, reference, problems);

        // 4. Comparison with a previous run of the program
        // One baseline per hash function and seed, otherwise different functions or inputs get compared
        CompareWithBaseline($"experiment3_baseline_{functionName}_{Helpers.Seed}.txt", reference, problems);

        // Summary
        Console.WriteLine();
        var distinct = problems.Distinct().ToList();
        if (distinct.Count == 0)
        {
            Console.WriteLine("RESULT: no discrepancies. The hash function is deterministic " +
                              "and keeps no state between calls.");
            return;
        }

        Console.WriteLine($"RESULT: {distinct.Count} discrepancies found. Explain them before " +
                          "running the statistical experiments:");
        foreach (var p in distinct)
            Console.WriteLine("  - " + p);

        Console.WriteLine();
        Console.WriteLine("Common causes:");
        Console.WriteLine("  * static or instance fields (buffers, counters, state) not reset between calls");
        Console.WriteLine("  * the input array is modified in place (e.g. padding written into it)");
        Console.WriteLine("  * string.GetHashCode() or HashCode used - randomized per process in .NET");
        Console.WriteLine("  * Random without a fixed seed, DateTime, Guid, or environment-dependent values");
        Console.WriteLine("  * culture-dependent formatting (ToString without CultureInfo.InvariantCulture)");
        Console.WriteLine("  * relying on Dictionary/HashSet iteration order");
    }

    /// <summary>Hashes a fresh copy of the input and checks that the input was not modified.</summary>
    private static string? Compute(Func<byte[], string> hash, string name, byte[] data, List<string> problems)
    {
        var copy = (byte[])data.Clone();
        string result;
        try
        {
            result = hash(copy);
        }
        catch (Exception ex)
        {
            problems.Add($"[exception] '{name}': {ex.GetType().Name}: {ex.Message}");
            return null;
        }

        if (!copy.AsSpan().SequenceEqual(data))
            problems.Add($"[mutation] '{name}': the hash function modified its input array");

        if (result is null)
        {
            problems.Add($"[null] '{name}': the hash function returned null");
            return null;
        }
        return result;
    }

    /// <summary>Child mode: prints "name TAB hash" for every input, for the parent process to compare.</summary>
    public static void PrintHashes(Func<byte[], string> hash)
    {
        foreach (var (name, data) in BuildInputs())
            Console.WriteLine($"{name}\t{hash(data)}");
    }

    private static void CompareWithSeparateProcesses(string functionName, Dictionary<string, string> reference,
        List<string> problems)
    {
        int failures = 0;
        for (int run = 1; run <= SeparateProcesses; run++)
        {
            Dictionary<string, string> hashes;
            try
            {
                hashes = HashInSeparateProcess(functionName);
            }
            catch (Exception ex)
            {
                failures++;
                problems.Add($"[separate process] run {run} failed: {ex.Message}");
                continue;
            }

            foreach (var (name, expected) in reference)
            {
                if (!hashes.TryGetValue(name, out var other))
                {
                    failures++;
                    problems.Add($"[separate process] run {run}: no hash printed for '{name}'");
                }
                else if (other != expected)
                {
                    failures++;
                    problems.Add($"[separate process] run {run}, '{name}': this process {Short(expected)}, " +
                                 $"new process {Short(other)}");
                }
            }
        }
        Report($"Separate processes ({SeparateProcesses} new runs)", failures);
    }

    /// <summary>Starts this program again in child mode and returns the input name -> hash it printed.</summary>
    private static Dictionary<string, string> HashInSeparateProcess(string functionName)
    {
        string program = Environment.ProcessPath ?? throw new InvalidOperationException("unknown program path");
        var start = new ProcessStartInfo(program)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        // Started as "dotnet Hash.Experiments.dll" instead of through the app host
        if (Path.GetFileNameWithoutExtension(program) == "dotnet")
            start.ArgumentList.Add(Assembly.GetEntryAssembly()!.Location);
        start.ArgumentList.Add(ChildArgument);
        start.ArgumentList.Add(functionName);

        using var process = Process.Start(start) ?? throw new InvalidOperationException("process did not start");
        var error = process.StandardError.ReadToEndAsync();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
            throw new InvalidOperationException($"exit code {process.ExitCode}: {error.Result.Trim()}");

        var hashes = new Dictionary<string, string>();
        foreach (var line in output.Split('\n'))
        {
            var parts = line.TrimEnd('\r').Split('\t');
            if (parts.Length == 2)
                hashes[parts[0]] = parts[1];
        }
        return hashes;
    }

    private static void CompareWithBaseline(string baselineFile, Dictionary<string, string> current, List<string> problems)
    {
        if (!File.Exists(baselineFile))
        {
            var lines = current.Select(kv =>
                $"{kv.Key}\t{Convert.ToBase64String(Encoding.UTF8.GetBytes(kv.Value))}");
            File.WriteAllLines(baselineFile, lines);
            Console.WriteLine($"Previous program run: baseline saved to {Path.GetFullPath(baselineFile)}. " +
                              "Run the program again to compare.");
            return;
        }

        var saved = new Dictionary<string, string>();
        foreach (var line in File.ReadAllLines(baselineFile))
        {
            var parts = line.Split('\t');
            if (parts.Length == 2)
                saved[parts[0]] = Encoding.UTF8.GetString(Convert.FromBase64String(parts[1]));
        }

        int failures = 0;
        foreach (var (name, hash) in current)
        {
            if (!saved.TryGetValue(name, out var old))
            {
                problems.Add($"[previous run] '{name}' is missing from the baseline (inputs changed? delete {baselineFile})");
                failures++;
            }
            else if (old != hash)
            {
                problems.Add($"[previous run] '{name}': previous run {Short(old)}, this run {Short(hash)}");
                failures++;
            }
        }
        Report("Previous program run", failures);
    }

    private static List<(string Name, byte[] Data)> BuildInputs()
    {
        var rng = new Random(Helpers.Seed); // fixed seed: identical inputs in every run

        byte[] RandomBytes(int n) { var b = new byte[n]; rng.NextBytes(b); return b; }
        byte[] RandomAscii(int n)
        {
            var b = new byte[n];
            for (int i = 0; i < n; i++) b[i] = (byte)rng.Next(32, 127);
            return b;
        }
        byte[] Ascii(string s) => Encoding.ASCII.GetBytes(s);

        return new List<(string, byte[])>
        {
            ("A", Ascii("A")),
            ("B", Ascii("B")),
            ("empty", Array.Empty<byte>()),
            ("abc", Ascii("abc")),
            ("abd", Ascii("abd")),
            ("fox", Ascii("The quick brown fox jumps over the lazy dog")),
            ("fox.", Ascii("The quick brown fox jumps over the lazy dog.")),
            ("zeros16", new byte[16]),
            ("ff16", Enumerable.Repeat((byte)0xFF, 16).ToArray()),
            ("ascii1000", RandomAscii(1000)),
            ("binary4096", RandomBytes(4096)),
            ("large100k", RandomBytes(100_000)),
        };
    }

    private static void Report(string check, int failures) =>
        Console.WriteLine($"{check}: {(failures == 0 ? "OK" : $"{failures} discrepancies")}");

    private static string Short(string? h) =>
        h is null ? "<null>" : h.Length <= 16 ? h : h[..16] + "...";
}