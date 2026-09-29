namespace Hash.Experiments.Experiments;

public static class Experiment2
{
    public static void Run(Func<byte[], string> hash)
    {
        byte[] input = { 65 };
        string result = hash(input);
        Console.WriteLine(result);
    }

}