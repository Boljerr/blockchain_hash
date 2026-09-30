using Hash.Hashing;
using Hash.Experiments.Experiments;

var igno = new HashFunc();
var aivaro = new HashaMasha();

var functions = new (string Name, Func<byte[], string> Hash)[]
{
    ("Igno", igno.ComputeHash),
    ("Aivaro", bytes => aivaro.ComputeHash(bytes)),
};

// Experiment 3 starts this program again with these arguments to hash its inputs in a separate process
if (args is [Experiment3.ChildArgument, var childName])
{
    Experiment3.PrintHashes(functions.Single(f => f.Name == childName).Hash);
    return;
}

const int seed = Helpers.Seed;
foreach (var function in functions)
{
    var random = new Random(seed);
    Console.WriteLine($"Testing {function.Name}");
    //Experiment
    Experiment2.Run(function.Hash);
    Experiment3.Run(function.Name, function.Hash);
    Experiment4.Run(function.Name ,function.Hash);
    Experiment5.Run(function.Name, function.Hash, seed);
    Experiment7.Run(function.Name, function.Hash, seed);
    //Experiment6.Run(function.Name, function.Hash, random);
}