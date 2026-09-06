using System.Security.Cryptography.X509Certificates;

namespace OurSigner;

public sealed class SigningResult
{
    public bool Ready { get; init; }
    public string Message { get; init; } = "";
    public string? CertificateSubject { get; init; }
    public string? ProvisioningProfilePath { get; init; }
}

public sealed class SigningManager
{
    private const string BundleIdentifier = "com.anar.oursign";

    public X509Certificate2? FindAppleDevelopmentCertificate()
    {
        using X509Store store = new(
            StoreName.My,
            StoreLocation.CurrentUser
        );

        store.Open(OpenFlags.ReadOnly);

        X509Certificate2Collection certificates = store.Certificates.Find(
            X509FindType.FindBySubjectName,
            "Apple Development",
            validOnly: false
        );

        return certificates
            .Cast<X509Certificate2>()
            .Where(certificate => certificate.HasPrivateKey)
            .OrderByDescending(certificate => certificate.NotAfter)
            .FirstOrDefault();
    }

    public bool HasAppleDevelopmentCertificate()
    {
        return FindAppleDevelopmentCertificate() != null;
    }

    public SigningResult PrepareSigning(
        string? deviceIdentifier = null)
    {
        X509Certificate2? certificate =
            FindAppleDevelopmentCertificate();

        if (certificate == null)
        {
            return new SigningResult
            {
                Ready = false,
                Message = "No Apple Development certificate with a private key was found."
            };
        }

        var profileManager = new ProvisioningProfileManager();

        ProvisioningProfileInfo? profile =
            profileManager.FindOurSignProfile(deviceIdentifier);

        if (profile == null)
        {
            return new SigningResult
            {
                Ready = false,
                Message = "No usable provisioning profile was found for com.anar.oursign.",
                CertificateSubject = certificate.Subject
            };
        }

        if (certificate.NotAfter <= DateTime.UtcNow)
        {
            return new SigningResult
            {
                Ready = false,
                Message = "The Apple Development certificate has expired.",
                CertificateSubject = certificate.Subject,
                ProvisioningProfilePath = profile.FilePath
            };
        }

        return new SigningResult
        {
            Ready = true,
            Message = "Signing requirements are ready.",
            CertificateSubject = certificate.Subject,
            ProvisioningProfilePath = profile.FilePath
        };
    }
}