namespace OurSigner;

public class OurSignInstaller
{
    private readonly DeviceManager deviceManager;
    private readonly ITunesManager iTunesManager;
    private readonly IPAManager ipaManager;

    public OurSignInstaller(
        DeviceManager deviceManager,
        ITunesManager iTunesManager,
        IPAManager ipaManager)
    {
        this.deviceManager = deviceManager;
        this.iTunesManager = iTunesManager;
        this.ipaManager = ipaManager;
    }

    public InstallResult CheckRequirements()
    {
        if (!iTunesManager.IsInstalled())
            return InstallResult.ITunesMissing;

        if (!deviceManager.IsIPhoneConnected())
            return InstallResult.IPhoneMissing;

        if (string.IsNullOrEmpty(ipaManager.SelectedIPA))
            return InstallResult.IPAMissing;

        return InstallResult.Ready;
    }
}

public enum InstallResult
{
    Ready,
    ITunesMissing,
    IPhoneMissing,
    IPAMissing
}
