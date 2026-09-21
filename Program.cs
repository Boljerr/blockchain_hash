using System.Text;
using IO;
 
public static class CryptoHash {
    public static void Main() {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;
        while (true) {
            Console.WriteLine();
            Console.WriteLine("=== HašaMaša ===");
            Console.WriteLine("1) Pritaikyk maišos funkciją tekstui");
            Console.WriteLine("2) Pritaikyk maišos funkciją failui");
            Console.WriteLine("0) Išeiti");
            Console.Write("Pasirinkti: ");
            string? choice = Console.ReadLine()?.Trim();
            if (choice == null || choice == "0") break;
            switch (choice) {
                case "1": HashIO.HashTextFromUser(); break;
                case "2": HashIO.HashFileFromUser(); break;
                default: Console.WriteLine("Įveskite 1, 2 arba 0."); break;
            }
        }
    }
}