using Hash.Input;
using Hash.Hashing;

var inputReader = new InputReader();
var hashFunc = new HashFunc();

string filePath = Path.Combine(AppContext.BaseDirectory, "data", "test.txt");

byte[] data = inputReader.ReadFile(filePath);
string hash = hashFunc.ComputeHash(data);
Console.WriteLine(hash);