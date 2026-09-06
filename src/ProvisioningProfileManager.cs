using System.Security.Cryptography.Pkcs;
using System.Xml.Linq;

namespace OurSigner;

public sealed class ProvisioningProfileInfo
{
    public string FilePath { get; init; } = "";
    public string Name { get; init; } = "";
    public string AppIdentifier { get; init; } = "";
    public string TeamIdentifier { get; init; } = "";
    public DateTime ExpirationDate { get; init; }
    public bool IsExpired => DateTime.UtcNow > ExpirationDate.ToUniversalTime();
    public List<string> ProvisionedDevices { get; init; } = [];
    public List<string> DeveloperCertificates { get; init; } = [];
    public Dictionary<string, string> Entitlements { get; init; } = [];
}

public sealed class ProvisioningProfileManager
{
    public const string OurSignBundleIdentifier = "com.anar.oursign";

    public IReadOnlyList<string> FindProfileFiles()
    {
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        string[] roots =
        [
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Apple Computer",
                "MobileDevice",
                "Provisioning Profiles"
            ),
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Apple Computer",
                "MobileDevice",
                "Provisioning Profiles"
            )
        ];

        foreach (string root in roots)
        {
            if (!Directory.Exists(root))
                continue;

            foreach (string file in Directory.EnumerateFiles(
                root,
                "*.mobileprovision",
                SearchOption.AllDirectories))
            {
                paths.Add(file);
            }
        }

        return paths.ToList();
    }

    public IReadOnlyList<ProvisioningProfileInfo> FindProfiles()
    {
        var profiles = new List<ProvisioningProfileInfo>();

        foreach (string path in FindProfileFiles())
        {
            try
            {
                ProvisioningProfileInfo? profile = ReadProfile(path);

                if (profile != null)
                    profiles.Add(profile);
            }
            catch
            {
            }
        }

        return profiles;
    }

    public ProvisioningProfileInfo? FindUsableProfile(
        string bundleIdentifier,
        string? deviceIdentifier = null)
    {
        foreach (ProvisioningProfileInfo profile in FindProfiles())
        {
            if (profile.IsExpired)
                continue;

            if (!BundleIdentifierMatches(
                    profile.AppIdentifier,
                    bundleIdentifier))
                continue;

            if (deviceIdentifier != null &&
                profile.ProvisionedDevices.Count > 0 &&
                !profile.ProvisionedDevices.Contains(
                    deviceIdentifier,
                    StringComparer.OrdinalIgnoreCase))
                continue;

            return profile;
        }

        return null;
    }

    public ProvisioningProfileInfo? FindOurSignProfile(
        string? deviceIdentifier = null)
    {
        return FindUsableProfile(
            OurSignBundleIdentifier,
            deviceIdentifier
        );
    }

    public bool HasUsableOurSignProfile(
        string? deviceIdentifier = null)
    {
        return FindOurSignProfile(deviceIdentifier) != null;
    }

    private static ProvisioningProfileInfo? ReadProfile(string path)
    {
        byte[] data = File.ReadAllBytes(path);

        var cms = new SignedCms();
        cms.Decode(data);

        byte[] plistData = cms.ContentInfo.Content;

        XDocument document = XDocument.Parse(
            System.Text.Encoding.UTF8.GetString(plistData)
        );

        XElement? dict = document
            .Root?
            .Element("dict");

        if (dict == null)
            return null;

        Dictionary<string, XElement> values = ParseDictionary(dict);

        string name = GetString(values, "Name") ?? "";
        string appIdentifier = "";
        string teamIdentifier = "";

        if (values.TryGetValue("Entitlements", out XElement? entitlements))
        {
            Dictionary<string, XElement> entitlementValues =
                ParseDictionary(entitlements);

            appIdentifier =
                GetString(
                    entitlementValues,
                    "application-identifier"
                ) ?? "";

            teamIdentifier =
                GetString(
                    entitlementValues,
                    "com.apple.developer.team-identifier"
                ) ?? "";
        }

        List<string> devices =
            GetArray(values, "ProvisionedDevices");

        List<string> certificates =
            GetDataArray(values, "DeveloperCertificates");

        DateTime expiration =
            GetDate(values, "ExpirationDate") ?? DateTime.MinValue;

        Dictionary<string, string> entitlementMap = [];

        if (values.TryGetValue("Entitlements", out XElement? entitlementDict))
        {
            foreach (var pair in ParseDictionary(entitlementDict))
            {
                string? value = GetString(
                    new Dictionary<string, XElement>
                    {
                        [pair.Key] = pair.Value
                    },
                    pair.Key
                );

                if (value != null)
                    entitlementMap[pair.Key] = value;
            }
        }

        return new ProvisioningProfileInfo
        {
            FilePath = path,
            Name = name,
            AppIdentifier = appIdentifier,
            TeamIdentifier = teamIdentifier,
            ExpirationDate = expiration,
            ProvisionedDevices = devices,
            DeveloperCertificates = certificates,
            Entitlements = entitlementMap
        };
    }

    private static Dictionary<string, XElement> ParseDictionary(
        XElement dict)
    {
        var result = new Dictionary<string, XElement>();

        XElement[] elements = dict.Elements().ToArray();

        for (int i = 0; i < elements.Length - 1; i++)
        {
            if (elements[i].Name.LocalName != "key")
                continue;

            string key = elements[i].Value;
            result[key] = elements[i + 1];
            i++;
        }

        return result;
    }

    private static string? GetString(
        Dictionary<string, XElement> values,
        string key)
    {
        if (!values.TryGetValue(key, out XElement? element))
            return null;

        return element.Name.LocalName == "string"
            ? element.Value
            : null;
    }

    private static DateTime? GetDate(
        Dictionary<string, XElement> values,
        string key)
    {
        if (!values.TryGetValue(key, out XElement? element))
            return null;

        if (element.Name.LocalName != "date")
            return null;

        return DateTime.TryParse(
            element.Value,
            out DateTime result
        )
            ? result
            : null;
    }

    private static List<string> GetArray(
        Dictionary<string, XElement> values,
        string key)
    {
        var result = new List<string>();

        if (!values.TryGetValue(key, out XElement? element))
            return result;

        if (element.Name.LocalName != "array")
            return result;

        foreach (XElement item in element.Elements("string"))
            result.Add(item.Value);

        return result;
    }

    private static List<string> GetDataArray(
        Dictionary<string, XElement> values,
        string key)
    {
        var result = new List<string>();

        if (!values.TryGetValue(key, out XElement? element))
            return result;

        if (element.Name.LocalName != "array")
            return result;

        foreach (XElement item in element.Elements("data"))
            result.Add(item.Value.Trim());

        return result;
    }

    private static bool BundleIdentifierMatches(
        string appIdentifier,
        string bundleIdentifier)
    {
        if (string.IsNullOrWhiteSpace(appIdentifier))
            return false;

        int separator = appIdentifier.IndexOf('.');

        if (separator < 0)
            return false;

        string profileBundleIdentifier =
            appIdentifier[(separator + 1)..];

        if (profileBundleIdentifier == bundleIdentifier)
            return true;

        if (profileBundleIdentifier == "*")
            return true;

        if (profileBundleIdentifier.EndsWith(".*"))
        {
            string prefix =
                profileBundleIdentifier[..^2];

            return bundleIdentifier.StartsWith(
                prefix + ".",
                StringComparison.Ordinal
            );
        }

        return false;
    }
}