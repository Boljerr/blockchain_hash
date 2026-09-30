using System.Security.Cryptography;
using Hash.Hashing;
using Hash.Experiments.Experiments;

var igno = new HashFunc();
var aivaro = new HashaMasha();

var functions = new (string Name, int HexLength, Func<byte[], string> Hash)[]
{
    ("Igno", 64, igno.ComputeHash),
    ("Aivaro", 64, bytes => aivaro.ComputeHash(bytes)),
    ("MD5", 32, bytes => Convert.ToHexString(MD5.HashData(bytes))),
    ("SHA1", 40, bytes => Convert.ToHexString(SHA1.HashData(bytes))),
    ("SHA256", 64, bytes => Convert.ToHexString(SHA256.HashData(bytes))),
};

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
    
    //Experiment2.Run(function.Hash, function.HexLength);
    
    Experiment4.Run(function.Name ,function.Hash, function.HexLength);
    Experiment6.Run(function.Name, function.Hash, random);
}