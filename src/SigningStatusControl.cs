using System.Drawing;
using System.Windows.Forms;

namespace OurSigner;

public sealed class SigningStatusControl : UserControl
{
    private readonly Label titleLabel;
    private readonly Label certificateLabel;
    private readonly Label profileLabel;
    private readonly Label deviceLabel;
    private readonly Label overallLabel;
    private readonly Button refreshButton;

    public SigningStatusControl()
    {
        BackColor = Color.FromArgb(24, 24, 28);
        ForeColor = Color.White;
        Padding = new Padding(18);
        AutoSize = true;
        Dock = DockStyle.Top;

        titleLabel = CreateLabel("Signing Status", 16, true);
        certificateLabel = CreateLabel("Certificate: Checking...", 12, false);
        profileLabel = CreateLabel("Provisioning Profile: Checking...", 12, false);
        deviceLabel = CreateLabel("Device: Checking...", 12, false);
        overallLabel = CreateLabel("Status: Checking...", 13, true);

        refreshButton = new Button
        {
            Text = "Refresh",
            AutoSize = true,
            FlatStyle = FlatStyle.System
        };

        refreshButton.Click += (_, _) => RefreshStatus();

        var layout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            BackColor = Color.Transparent
        };

        layout.Controls.Add(titleLabel);
        layout.Controls.Add(certificateLabel);
        layout.Controls.Add(profileLabel);
        layout.Controls.Add(deviceLabel);
        layout.Controls.Add(overallLabel);
        layout.Controls.Add(refreshButton);

        Controls.Add(layout);
    }

    public void RefreshStatus(string? deviceIdentifier = null)
    {
        var signingManager = new SigningManager();
        var result = signingManager.PrepareSigning(deviceIdentifier);

        certificateLabel.Text =
            result.CertificateSubject == null
                ? "Certificate: Missing"
                : $"Certificate: {result.CertificateSubject}";

        profileLabel.Text =
            result.ProvisioningProfilePath == null
                ? "Provisioning Profile: Missing"
                : $"Provisioning Profile: {Path.GetFileName(result.ProvisioningProfilePath)}";

        deviceLabel.Text =
            string.IsNullOrWhiteSpace(deviceIdentifier)
                ? "Device: Not selected"
                : $"Device: {deviceIdentifier}";

        overallLabel.Text =
            result.Ready
                ? "Status: Ready to sign"
                : $"Status: {result.Message}";
    }

    private static Label CreateLabel(
        string text,
        float size,
        bool bold)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            Font = new Font(
                SystemFonts.DefaultFont.FontFamily,
                size,
                bold ? FontStyle.Bold : FontStyle.Regular
            ),
            ForeColor = Color.White,
            Margin = new Padding(0, 4, 0, 4)
        };
    }
}