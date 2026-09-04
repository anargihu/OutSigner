using System;
using System.Collections.Generic;

namespace OurSigner;

class Program
{
    static readonly List<Certificate> Certificates = new();

    static void Main()
    {
        Console.Title = "OurSigner";

        while (true)
        {
            Console.Clear();
            Console.WriteLine("OurSigner");
            Console.WriteLine("────────────────────────");
            Console.WriteLine();
            Console.WriteLine("1. Apple Account");
            Console.WriteLine("2. Certificates");
            Console.WriteLine("3. iPhone");
            Console.WriteLine("4. Exit");
            Console.WriteLine();
            Console.Write("Select: ");

            string choice = Console.ReadLine() ?? "";

            switch (choice)
            {
                case "1":
                    AppleAccount();
                    break;

                case "2":
                    CertificateManager();
                    break;

                case "3":
                    IPhone();
                    break;

                case "4":
                    return;
            }
        }
    }

    static void AppleAccount()
    {
        Console.Clear();
        Console.WriteLine("Apple Account");
        Console.WriteLine("────────────────────────");
        Console.WriteLine();
        Console.WriteLine("Status: Not signed in");
        Console.WriteLine();
        Console.WriteLine("OurSigner will use Apple's authentication");
        Console.WriteLine("system instead of collecting your password.");
        Console.WriteLine();
        Console.WriteLine("Press Enter to return.");
        Console.ReadLine();
    }

    static void CertificateManager()
    {
        Console.Clear();
        Console.WriteLine("Certificates");
        Console.WriteLine("────────────────────────");
        Console.WriteLine();

        if (Certificates.Count == 0)
        {
            Console.WriteLine("No signing certificates available.");
        }
        else
        {
            foreach (var certificate in Certificates)
            {
                Console.WriteLine($"Name: {certificate.Name}");
                Console.WriteLine($"Type: {certificate.Type}");
                Console.WriteLine($"Status: {certificate.Status}");
                Console.WriteLine();
            }
        }

        Console.WriteLine("Press Enter to return.");
        Console.ReadLine();
    }

    static void IPhone()
    {
        Console.Clear();
        Console.WriteLine("iPhone");
        Console.WriteLine("────────────────────────");
        Console.WriteLine();
        Console.WriteLine("Status: Waiting for iPhone...");
        Console.WriteLine();
        Console.WriteLine("Connect your iPhone with USB.");
        Console.WriteLine();
        Console.WriteLine("Press Enter to return.");
        Console.ReadLine();
    }

    class Certificate
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string Status { get; set; } = "";
    }
}