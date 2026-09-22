using System.Text;

namespace Hash.Input;

public class InputReader
{
    public byte[] ReadText()
    {
        Console.WriteLine("Enter text: ");
        string text = Console.ReadLine();
        return Encoding.UTF8.GetBytes(text);
    }

    public byte[] ReadFile(string path)
    {
        Console.WriteLine($"Reading file: {path}");
        return File.ReadAllBytes(path);
    }
}