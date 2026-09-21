using System;
using System.IO;
using System.Text;
 
public static class CryptoHash
{
    public static void Main()
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;
 
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("=== HašaMaša ===");
            Console.WriteLine("1) Pritaikyk maišos funkciją tekstui");
            Console.WriteLine("2) Pritaikyk maišos funkciją failui");
            Console.WriteLine("0) Išeiti");
            Console.Write("Choose: ");
            string? choice = Console.ReadLine()?.Trim();
            if (choice == null || choice == "0") break;
            switch (choice)
            {
                case "1":; break;
                case "2":; break;
                default: Console.WriteLine("Please enter 1, 2 or 0."); break;
            }
        }
    }
}