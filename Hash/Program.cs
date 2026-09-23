using Hash.Input;
using Hash.Hashing;

var inputReader = new InputReader();
var hashFunc = new HashFunc();

string sampleFilePath = Path.Combine(AppContext.BaseDirectory, "data", "test.txt");

byte[] data;

while (true)
{
    Console.WriteLine("1 - Write text");
    Console.WriteLine("2 - Enter file path");
    Console.WriteLine("3 - Use sample file");
    Console.WriteLine("4 - Exit");

    string? choice = Console.ReadLine();
    
    switch (choice)
    {
        case "1":
            data = inputReader.ReadText();
            break;
        case "2":
            data = inputReader.ReadFileFromConsole();
            break;
        case "3":
            data = inputReader.ReadFile(sampleFilePath);
            break;
        case "4":
            return;
        default:
            Console.WriteLine("Invalid choice. Try again.");
            continue;
    }

    break;
}

while (true)
{
    Console.WriteLine("1 - Igno implementation");
    Console.WriteLine("2 - Aivaro implementation");
    
    string? choice = Console.ReadLine();
    
    switch (choice)
    {
        case "1":
            string hashIgno = hashFunc.ComputeHash(data);
            Console.WriteLine();
            Console.WriteLine($"Hash: {hashIgno}");
            break;
        case "2":
            string hashAivaro = HashaMasha.ComputeHash(data);
            Console.WriteLine();
            Console.WriteLine($"Hash: {hashAivaro}");
            break;
        default:
            Console.WriteLine("Invalid choice. Try again.");
            continue;
    }

    break;
}
