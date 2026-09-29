using Hash.Hashing;
using Hash.Experiments;
using Hash.Experiments.Experiments;

var igno = new HashFunc();
var aivaro = new HashaMasha();

var functions = new (string Name, Func<byte[], string> Hash)[]
{
    ("Igno", igno.ComputeHash),
    ("Aivaro", bytes => aivaro.ComputeHash(bytes)),
};
foreach (var function in functions)
{
    Console.WriteLine($"Testing {function.Name}");
    
    Experiment2.Run(function.Hash);
}