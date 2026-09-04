using System;
using System.Management;

namespace OurSigner;

class Program
{
    static void Main()
    {
        Console.Title = "OurSigner";
        Console.WriteLine("OurSigner");
        Console.WriteLine("iPhone USB Detection");
        Console.WriteLine();

        CheckForIPhone();

        using var watcher = new ManagementEventWatcher(
            new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent")
        );

        watcher.EventArrived += (_, _) =>
        {
            System.Threading.Thread.Sleep(1000);
            CheckForIPhone();
        };

        watcher.Start();

        Console.WriteLine("Waiting for an iPhone...");
        Console.WriteLine("Connect an iPhone using USB-A or USB-C.");
        Console.WriteLine();
        Console.WriteLine("Press Enter to exit.");

        Console.ReadLine();

        watcher.Stop();
    }

    static void CheckForIPhone()
    {
        using var searcher = new ManagementObjectSearcher(
            "SELECT Name, Manufacturer, PNPDeviceID FROM Win32_PnPEntity"
        );

        bool found = false;

        foreach (ManagementObject device in searcher.Get())
        {
            string name = device["Name"]?.ToString() ?? "";
            string manufacturer = device["Manufacturer"]?.ToString() ?? "";
            string pnpId = device["PNPDeviceID"]?.ToString() ?? "";

            if (
                name.Contains("iPhone", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("Apple Mobile Device", StringComparison.OrdinalIgnoreCase) ||
                (
                    manufacturer.Contains("Apple", StringComparison.OrdinalIgnoreCase) &&
                    pnpId.Contains("VID_05AC", StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                found = true;

                Console.WriteLine("================================");
                Console.WriteLine("📱 IPHONE CONNECTED");
                Console.WriteLine("================================");
                Console.WriteLine($"Device: {name}");
                Console.WriteLine($"Manufacturer: {manufacturer}");
                Console.WriteLine();
            }
        }

        if (!found)
        {
            Console.WriteLine("❌ iPhone not detected.");
        }
    }
}