namespace OurSigner;

public sealed class IPAManager
{
    public string? SelectedIPA { get; private set; }

    public bool SelectIPA(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        if (!File.Exists(path))
            return false;

        if (!string.Equals(
                Path.GetExtension(path),
                ".ipa",
                StringComparison.OrdinalIgnoreCase))
            return false;

        SelectedIPA = path;
        return true;
    }

    public void ClearIPA()
    {
        SelectedIPA = null;
    }

    public bool HasIPA()
    {
        return !string.IsNullOrWhiteSpace(SelectedIPA)
            && File.Exists(SelectedIPA);
    }
}
