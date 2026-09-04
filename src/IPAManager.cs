using System.IO;

namespace OurSigner;

public class IPAManager
{
    public string? SelectedIPA { get; private set; }

    public bool SelectIPA(string path)
    {
        if (!File.Exists(path))
            return false;

        if (!Path.GetExtension(path).Equals(".ipa", System.StringComparison.OrdinalIgnoreCase))
            return false;

        SelectedIPA = path;
        return true;
    }

    public void Clear()
    {
        SelectedIPA = null;
    }
}
