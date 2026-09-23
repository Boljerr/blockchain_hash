using System.Text;

namespace Hash.Input;

public class InputReader
{
    public byte[] ReadText()
    {
        Console.WriteLine("Enter text: ");

        string? text = Console.ReadLine();
        if (text is null)
            throw new InvalidOperationException("Console input was closed.");

        return Encoding.UTF8.GetBytes(text);
    }

    public byte[] ReadFile(string path)
    {
        Console.WriteLine($"Reading file: {path}");
        return File.ReadAllBytes(path);
    }

    public byte[] ReadFileFromConsole()
    {
        while (true)
        {
            Console.Write("Enter file path: ");
            string? path = Console.ReadLine();

            if (path is null)
            {
                throw new InvalidOperationException("Console input was closed.");
            }

            try
            {
                return ReadFile(path);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}