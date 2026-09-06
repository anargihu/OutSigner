namespace OurSigner;

public sealed class SigningPipelineResult
{
    public bool Success { get; init; }
    public string Status { get; init; } = "";
    public string? IPAPath { get; init; }
    public string? Certificate { get; init; }
    public string? ProvisioningProfile { get; init; }
}

public sealed class SigningPipeline
{
    private readonly SigningManager signingManager = new();
    private readonly ProvisioningProfileManager profileManager = new();

    public SigningPipelineResult Prepare(
        string ipaPath,
        string? deviceIdentifier = null)
    {
        if (string.IsNullOrWhiteSpace(ipaPath))
        {
            return Failure("No IPA was selected.");
        }

        if (!File.Exists(ipaPath))
        {
            return Failure("The selected IPA could not be found.");
        }

        if (!string.Equals(
                Path.GetExtension(ipaPath),
                ".ipa",
                StringComparison.OrdinalIgnoreCase))
        {
            return Failure("The selected file is not an IPA.");
        }

        var signingResult =
            signingManager.PrepareSigning(deviceIdentifier);

        if (!signingResult.Ready)
        {
            return new SigningPipelineResult
            {
                Success = false,
                Status = signingResult.Message,
                IPAPath = ipaPath,
                Certificate = signingResult.CertificateSubject,
                ProvisioningProfile = signingResult.ProvisioningProfilePath
            };
        }

        ProvisioningProfileInfo? profile =
            profileManager.FindOurSignProfile(deviceIdentifier);

        if (profile == null)
        {
            return Failure(
                "No usable provisioning profile was found."
            );
        }

        return new SigningPipelineResult
        {
            Success = true,
            Status = "Signing requirements validated. Ready for Apple signing.",
            IPAPath = ipaPath,
            Certificate = signingResult.CertificateSubject,
            ProvisioningProfile = profile.FilePath
        };
    }

    public bool CanPrepare(string ipaPath)
    {
        return Prepare(ipaPath).Success;
    }

    private static SigningPipelineResult Failure(string message)
    {
        return new SigningPipelineResult
        {
            Success = false,
            Status = message
        };
    }
}
