using System;
using System.Management;

namespace OurSigner;

public class DeviceManager
{
    public bool IsIPhoneConnected()
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
                (
                    manufacturer.Contains("Apple", StringComparison.OrdinalIgnoreCase) &&
                    pnpId.Contains("VID_05AC", StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                return true;
            }
        }

        return false;
    }
}
