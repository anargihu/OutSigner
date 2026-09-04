using System.IO;

namespace OurSigner;

public class ITunesManager
{
    public bool IsInstalled()
    {
        string[] paths =
        {
            @"C:\Program Files\iTunes\iTunes.exe",
            @"C:\Program Files (x86)\iTunes\iTunes.exe"
        };

        foreach (string path in paths)
        {
            if (File.Exists(path))
                return true;
        }

        return false;
    }
}
