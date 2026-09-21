using System.Text;

namespace IO;


public static class HashIO {

    public static void HashTextFromUser() {
        Console.Write("Įveskite tekstą: ");
        string? text = Console.ReadLine();
        if (text == null) return;
        int byteCount = Encoding.UTF8.GetByteCount(text);
    }

    public static void HashFileFromUser() {
        Console.Write("Įveskite failo kelią: ");
        string? path = Console.ReadLine()?.Trim().Trim('"');
        if (string.IsNullOrEmpty(path)) { Console.WriteLine("Nėra įvestas failo kelias."); return; }
        if (!File.Exists(path)) { Console.WriteLine("Failas nerastas."); return; }
        try {
            byte[] data = File.ReadAllBytes(path);
        } catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) {
            Console.WriteLine($"Negalima perskaityti failų: {ex.Message}");
        }
    }

}