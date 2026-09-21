using System.Text;
using IO;
 
public static class CryptoHash {
    public static void Main() {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;
        while (true) {
            byte[]? data;
            Console.WriteLine();
            Console.WriteLine("=== HašaMaša ===");
            Console.WriteLine("1) Pritaikyk maišos funkciją tekstui");
            Console.WriteLine("2) Pritaikyk maišos funkciją failui");
            Console.WriteLine("0) Išeiti");
            Console.Write("Pasirinkti: ");
            string? choice = Console.ReadLine()?.Trim();
            if (choice == null || choice == "0") break;
            switch (choice) {
                case "1": 
                    data = HashIO.getHashTextFromUser(); 
                    break;
                case "2": 
                    data = HashIO.getHashFileFromUser(); 
                    break;
                default: 
                    Console.WriteLine("Įveskite 1, 2 arba 0."); 
                    continue;
            }
            if (data == null) continue;
        }
    }
}