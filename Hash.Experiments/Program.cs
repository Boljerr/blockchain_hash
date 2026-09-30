using Hash.Hashing;
using Hash.Experiments.Experiments;

var igno = new HashFunc();
var aivaro = new HashaMasha();

var functions = new (string Name, Func<byte[], string> Hash)[]
{
    ("Igno", igno.ComputeHash),
    ("Aivaro", bytes => aivaro.ComputeHash(bytes)),
};
const int seed = 676767;
foreach (var function in functions)
{
    var random = new Random(seed);
    Console.WriteLine($"Testing {function.Name}");
    
    //Experiment2.Run(function.Hash);
    
    //Experiment4.Run(function.Name ,function.Hash);
    Experiment6.Run(function.Name, function.Hash, random);
}