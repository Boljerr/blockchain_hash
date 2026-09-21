using System.Text;

namespace IO;


public static class HashIO {

    public static byte[] getHashTextFromUser() {
        Console.Write("Įveskite tekstą: ");
        string? text = Console.ReadLine();
        if (text == null) return null;
        return Encoding.UTF8.GetBytes(text);
    }

    public static byte[] getHashFileFromUser() {
        byte[] data;
        Console.Write("Įveskite failo kelią: ");
        string? path = Console.ReadLine()?.Trim().Trim('"');
        if (string.IsNullOrEmpty(path)) { Console.WriteLine("Nėra įvestas failo kelias."); return null; }
        if (!File.Exists(path)) { Console.WriteLine("Failas nerastas."); return null; }
        try {
            data = File.ReadAllBytes(path);
        } catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) {
            Console.WriteLine($"Negalima perskaityti failų: {ex.Message}");
            return null;
        }
        return data;
    }

}