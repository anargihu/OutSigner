using System;
using System.Management;

namespace OurSigner;

class Program
{
    static void Main()
    {
        Console.Title = "OurSigner";
        Console.WriteLine("OurSigner");
        Console.WriteLine("USB Device Detection");
        Console.WriteLine();

        using var watcher = new ManagementEventWatcher(
            new WqlEventQuery(
                "SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2"
            )
        );

        watcher.EventArrived += (_, _) => CheckForIPhone();

        CheckForIPhone();

        Console.WriteLine("Waiting for iPhone...");
        Console.WriteLine("Connect your iPhone with a USB cable.");
        Console.WriteLine();
        Console.WriteLine("Press Enter to exit.");

        watcher.Start();
        Console.ReadLine();
        watcher.Stop();
    }

    static void CheckForIPhone()
    {
        using var searcher = new ManagementObjectSearcher(
            "SELECT Name, Manufacturer, PNPDeviceID FROM Win32_PnPEntity"
        );

        foreach (ManagementObject device in searcher.Get())
        {
            string name = device["Name"]?.ToString() ?? "";
            string manufacturer = device["Manufacturer"]?.ToString() ?? "";
            string pnpId = device["PNPDeviceID"]?.ToString() ?? "";

            if (
                name.Contains("iPhone", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("Apple Mobile Device", StringComparison.OrdinalIgnoreCase) ||
                manufacturer.Contains("Apple", StringComparison.OrdinalIgnoreCase) &&
                pnpId.Contains("VID_05AC", StringComparison.OrdinalIgnoreCase)
            )
            {
                Console.WriteLine("📱 iPhone detected!");
                Console.WriteLine($"Device: {name}");
                Console.WriteLine($"Manufacturer: {manufacturer}");
                Console.WriteLine();
                return;
            }
        }
    }
}