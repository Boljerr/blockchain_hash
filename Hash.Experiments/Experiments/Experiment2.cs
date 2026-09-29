using System.Text;
using Hash.Input;

namespace Hash.Experiments.Experiments;

public static class Experiment2
{
    public static void Run(Func<byte[], string> hash)
    {
        byte[][] inputs =
        {
            Array.Empty<byte>(),
            Encoding.UTF8.GetBytes("a"),
            Encoding.UTF8.GetBytes("b"),
            Encoding.UTF8.GetBytes("aaaa"),
            Encoding.UTF8.GetBytes("ba"),
            Encoding.UTF8.GetBytes(" abc "),
            Encoding.UTF8.GetBytes("abc\n"),
            Encoding.UTF8.GetBytes("ąčęėįšųūž 🥀🥀🥀🥀"),
            Encoding.UTF8.GetBytes("🥀🥀🥀🥀🥀🥀🥀🥀🥀🥀🥀🥀"),
        };
        //Hashes everything
        foreach (var input in inputs)
        {
            string result = hash(input);
            HashChecks.AssertValidHash(result);
        }
        //Leading 0 
        bool found = false;
        for (int i = 0; i < int.MaxValue-1; i++)
        {
            byte[] input = Encoding.UTF8.GetBytes(i.ToString());
            string result = hash(input);
            HashChecks.AssertValidHash(result);
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
        
        HashChecks.AssertValidHash(fileHash);
        HashChecks.AssertValidHash(typedHash);
        
        if(typedHash != fileHash)
            throw new Exception("Same bytes gave different results :(");
        Console.WriteLine("Hash is the same!");
    }
    //TODO: in readme.md write that this experiment failed without removing the BOM. Once removed it passes :)
    

}