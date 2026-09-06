using System.Security.Cryptography.X509Certificates;

namespace OurSigner;

public sealed class SigningManager
{
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
            .FirstOrDefault(certificate => certificate.HasPrivateKey);
    }

    public bool HasAppleDevelopmentCertificate()
    {
        return FindAppleDevelopmentCertificate() != null;
    }
}