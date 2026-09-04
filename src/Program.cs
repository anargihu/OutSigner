using System;
using System.Drawing;
using System.IO;
using System.Management;
using System.Windows.Forms;

namespace OurSigner;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}

class MainForm : Form
{
    Label deviceStatus = new();
    Label itunesStatus = new();
    Label accountStatus = new();
    Label signingStatus = new();
    Label ipaStatus = new();
    Label logLabel = new();
    Button pairButton = new();
    Button ipaButton = new();
    Button installButton = new();
    ManagementEventWatcher? watcher;
    string? selectedIpa;

    public MainForm()
    {
        Text = "OurSigner";
        Width = 650;
        Height = 850;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(11, 11, 15);
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10);

        var title = new Label
        {
            Text = "OurSign",
            Font = new Font("Segoe UI", 28, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(35, 25)
        };

        var subtitle = new Label
        {
            Text = "iOS sideloading",
            ForeColor = Color.Gray,
            AutoSize = true,
            Location = new Point(38, 70)
        };

        Controls.Add(title);
        Controls.Add(subtitle);

        AddDeviceCard();
        AddAccountCard();
        AddIpaCard();
        AddSigningCard();
        AddLogCard();

        StartDeviceWatcher();
        CheckForIPhone();
        CheckForITunes();
    }

    void AddDeviceCard()
    {
        var card = CreateCard(35, 110, 560, 145);

        card.Controls.Add(CreateLabel("📱  iPhone", 20, 18, 18, true));

        deviceStatus = CreateLabel(
            "●  Waiting for iPhone...",
            20, 55, 11, false
        );

        pairButton = new Button
        {
            Text = "Pair iPhone",
            Location = new Point(20, 88),
            Width = 520,
            Height = 35,
            Enabled = false
        };

        pairButton.Click += (_, _) =>
        {
            deviceStatus.Text = "●  iPhone connected";
            deviceStatus.ForeColor = Color.LightGreen;
            pairButton.Text = "Connected ✓";
            pairButton.Enabled = false;
            AddLog("iPhone connection detected.");
        };

        card.Controls.Add(deviceStatus);
        card.Controls.Add(pairButton);
        Controls.Add(card);
    }

    void AddAccountCard()
    {
        var card = CreateCard(35, 270, 560, 125);

        card.Controls.Add(CreateLabel("🍎  Apple Account", 20, 18, 18, true));

        accountStatus = CreateLabel(
            "●  Not authenticated",
            20, 55, 11, false
        );

        var button = new Button
        {
            Text = "Authenticate with Apple",
            Location = new Point(300, 48),
            Width = 240,
            Height = 35
        };

        button.Click += (_, _) =>
        {
            accountStatus.Text = "●  Apple authentication required";
            accountStatus.ForeColor = Color.Gold;
            AddLog("Apple authentication flow requested.");
        };

        card.Controls.Add(accountStatus);
        card.Controls.Add(button);
        Controls.Add(card);
    }

    void AddIpaCard()
    {
        var card = CreateCard(35, 410, 560, 155);

        card.Controls.Add(CreateLabel("📦  IPA", 20, 18, 18, true));

        ipaStatus = CreateLabel(
            "No IPA selected",
            20, 55, 11, false
        );

        ipaButton = new Button
        {
            Text = "Choose IPA",
            Location = new Point(20, 88),
            Width = 250,
            Height = 35
        };

        installButton = new Button
        {
            Text = "Install IPA",
            Location = new Point(290, 88),
            Width = 250,
            Height = 35,
            Enabled = false
        };

        ipaButton.Click += (_, _) =>
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "iOS App (*.ipa)|*.ipa",
                Title = "Select IPA"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                selectedIpa = dialog.FileName;
                ipaStatus.Text = Path.GetFileName(selectedIpa);
                ipaStatus.ForeColor = Color.LightGreen;
                installButton.Enabled = true;
                AddLog("IPA selected: " + Path.GetFileName(selectedIpa));
            }
        };

        installButton.Click += (_, _) =>
        {
            if (string.IsNullOrEmpty(selectedIpa))
                return;

            AddLog("Install requested for " + Path.GetFileName(selectedIpa));
            signingStatus.Text = "●  Ready for signing";
            signingStatus.ForeColor = Color.LightGreen;
        };

        card.Controls.Add(ipaStatus);
        card.Controls.Add(ipaButton);
        card.Controls.Add(installButton);
        Controls.Add(card);
    }

    void AddSigningCard()
    {
        var card = CreateCard(35, 580, 560, 100);

        card.Controls.Add(CreateLabel("🔐  Signing", 20, 18, 18, true));

        signingStatus = CreateLabel(
            "●  Waiting for IPA",
            20, 55, 11, false
        );

        card.Controls.Add(signingStatus);
        Controls.Add(card);
    }

    void AddLogCard()
    {
        var card = CreateCard(35, 695, 560, 85);

        card.Controls.Add(CreateLabel("📋  Activity", 20, 12, 16, true));

        logLabel = CreateLabel(
            "OurSign ready.",
            20, 43, 9, false
        );

        logLabel.MaximumSize = new Size(520, 30);
        card.Controls.Add(logLabel);
        Controls.Add(card);
    }

    void StartDeviceWatcher()
    {
        watcher = new ManagementEventWatcher(
            new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent")
        );

        watcher.EventArrived += (_, _) =>
        {
            BeginInvoke(CheckForIPhone);
        };

        watcher.Start();
    }

    void CheckForIPhone()
    {
        using var searcher = new ManagementObjectSearcher(
            "SELECT Name, Manufacturer, PNPDeviceID FROM Win32_PnPEntity"
        );

        bool found = false;

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
                found = true;
                break;
            }
        }

        if (found)
        {
            deviceStatus.Text = "●  iPhone detected";
            deviceStatus.ForeColor = Color.LightGreen;
            pairButton.Enabled = true;
        }
        else
        {
            deviceStatus.Text = "●  Waiting for iPhone...";
            deviceStatus.ForeColor = Color.Gray;
            pairButton.Enabled = false;
        }
    }

    void CheckForITunes()
    {
        string[] paths =
        {
            @"C:\Program Files\iTunes\iTunes.exe",
            @"C:\Program Files (x86)\iTunes\iTunes.exe"
        };

        bool installed = false;

        foreach (string path in paths)
        {
            if (File.Exists(path))
            {
                installed = true;
                break;
            }
        }

        var card = CreateCard(35, 795, 560, 80);

        card.Controls.Add(CreateLabel("🍎  iTunes", 20, 12, 16, true));

        itunesStatus = CreateLabel(
            installed ? "●  iTunes detected" : "●  iTunes required",
            20, 43, 10, false
        );

        itunesStatus.ForeColor = installed ? Color.LightGreen : Color.OrangeRed;

        card.Controls.Add(itunesStatus);
        Controls.Add(card);
    }

    void AddLog(string message)
    {
        logLabel.Text = message;
    }

    Panel CreateCard(int x, int y, int width, int height)
    {
        return new Panel
        {
            Location = new Point(x, y),
            Size = new Size(width, height),
            BackColor = Color.FromArgb(32, 32, 39)
        };
    }

    Label CreateLabel(string text, int x, int y, int size, bool bold)
    {
        return new Label
        {
            Text = text,
            Location = new Point(x, y),
            AutoSize = true,
            ForeColor = Color.White,
            Font = new Font(
                "Segoe UI",
                size,
                bold ? FontStyle.Bold : FontStyle.Regular
            )
        };
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        watcher?.Stop();
        watcher?.Dispose();
        base.OnFormClosed(e);
    }
}