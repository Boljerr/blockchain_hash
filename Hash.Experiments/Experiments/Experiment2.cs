using System.Text;
using Hash.Input;

namespace Hash.Experiments.Experiments;

public static class Experiment2
{
    public static void Run(Func<byte[], string> hash)
    {
        //Hashes every txt file in data
        string dataDirectory = Path.Combine(AppContext.BaseDirectory, "data");
        foreach (string path in Directory.GetFiles(dataDirectory, "*.txt").Order(StringComparer.Ordinal))
        {
            byte[] input = File.ReadAllBytes(path);
            string result = hash(input);
            Helpers.AssertValidHash(result);
            Console.WriteLine($"{Path.GetFileName(path)} ({input.Length} bytes): {result}");
        }
        //Leading 0 
        bool found = false;
        for (int i = 0; i < int.MaxValue-1; i++)
        {
            byte[] input = Encoding.UTF8.GetBytes(i.ToString());
            string result = hash(input);
            Helpers.AssertValidHash(result);
            if (result[0] == '0')
            {
                Console.WriteLine("Leading zero: " + result + " At iteration: " + i);
                found = true;
                break;
            }
        }
        if (!found)
            throw new Exception("No leading-zero hash found");
        
        // Typed vs File
        var reader = new InputReader();
        string sampleFilePath = Path.Combine(AppContext.BaseDirectory, "data", "Experiment2.txt");
        
        byte[] typedBytes = reader.ReadText();
        byte[] fileBytes = reader.ReadFile(sampleFilePath);

        if (!typedBytes.SequenceEqual(fileBytes))
            throw new Exception("Typed and file bytes are different");
        
        string typedHash = hash(typedBytes);
        string fileHash = hash(fileBytes);
        
        Helpers.AssertValidHash(fileHash);
        Helpers.AssertValidHash(typedHash);
        
        if(typedHash != fileHash)
            throw new Exception("Same bytes gave different results :(");
        Console.WriteLine("Hash is the same!");
    }
    //TODO: in readme.md write that this experiment failed without removing the BOM. Once removed it passes :)
    

}