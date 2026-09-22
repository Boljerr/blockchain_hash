using Hash.Input;

var inputReader = new InputReader();
string filePath = Path.Combine(AppContext.BaseDirectory, "data", "test.txt");

byte[] data = inputReader.ReadFileFromConsole();
Console.WriteLine(Convert.ToHexString(data));