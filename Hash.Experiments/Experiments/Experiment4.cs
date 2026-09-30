using System.Diagnostics;

namespace Hash.Experiments.Experiments;

public static class Experiment4
{
    public static void Run(string name, Func<byte[], string> hash)
    {
        string filePath = Path.Combine(AppContext.BaseDirectory, "data", "konstitucija.txt");
        byte [] file = File.ReadAllBytes(filePath);

        var lineEnds = new List<int>();

        for (int i = 0; i < file.Length; i++)
        {
            if (file[i] == (byte)'\n')
                lineEnds.Add(i+1);
        }
        var inputs = new List<(int Lines, byte[] Bytes)>();

        for (int lines = 1; lines < lineEnds.Count; lines *= 2)
        {
            int byteCount = lineEnds[lines - 1];
            byte[] prefix = new byte[byteCount];
            Array.Copy(file, prefix, byteCount);
            inputs.Add((lines, prefix));    
        }
        inputs.Add((lineEnds.Count, file)); // whole file

        var rows = new List<String>
        {
            "function,lines,bytes,trial,calls,elapsed_ms"
        };
        const int calls = 1000;
        foreach (var input in inputs)
        {
            Helpers.AssertValidHash(hash(input.Bytes));
            
            for (int i = 0; i < 5; i++) //warm up
                hash(input.Bytes);

            for (int trial = 1; trial <= 5; trial++)
            {
                int totalDigits = 0; //each hash has 64 chars so to count them as "used"
                var timer = Stopwatch.StartNew();
                
                for (int i = 0; i < calls; i++)
                    totalDigits += hash(input.Bytes).Length;
                
                timer.Stop();

                string elapsed = timer.Elapsed.TotalMilliseconds.ToString("F6");

                if (totalDigits != calls * 64)
                    throw new Exception("Unexpected hash lenght");
                
                rows.Add($"{name}, {input.Lines}, {input.Bytes.Length}, {trial}, {calls}, {elapsed}");
            }
        }

        string outputPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..",
            "Results",
            $"Experiment4_{name}.csv"));

        File.WriteAllLines(outputPath, rows);
        
        Console.WriteLine($"Saved: {outputPath}");
    }
}