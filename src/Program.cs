using System;
using System.Drawing;
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
    Label phoneStatus = new();
    Button pairButton = new();
    ManagementEventWatcher? watcher;

    public MainForm()
    {
        Text = "OurSigner";
        Width = 600;
        Height = 760;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(11, 11, 15);
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10);

        var title = new Label
        {
            Text = "OurSigner",
            Font = new Font("Segoe UI", 24, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(35, 30)
        };

        var subtitle = new Label
        {
            Text = "Apple device signing manager",
            ForeColor = Color.Gray,
            AutoSize = true,
            Location = new Point(38, 72)
        };

        Controls.Add(title);
        Controls.Add(subtitle);

        AddPhoneCard();
        AddAccountCard();
        AddSigningCard();
        AddCertificateCard();

        StartDeviceWatcher();
        CheckForIPhone();
    }

    void AddPhoneCard()
    {
        var card = CreateCard(35, 115, 510, 145);

        var title = CreateLabel("📱  iPhone", 20, 18, 18, true);

        phoneStatus = CreateLabel(
            "●  Waiting for iPhone...",
            20, 55, 11, false
        );

        pairButton = new Button
        {
            Text = "Pair iPhone",
            Location = new Point(20, 88),
            Width = 470,
            Height = 35,
            Enabled = false
        };

        pairButton.Click += (_, _) =>
        {
            phoneStatus.Text = "●  Paired";
            phoneStatus.ForeColor = Color.LightGreen;
            pairButton.Text = "Paired ✓";
            pairButton.Enabled = false;
        };

        card.Controls.Add(title);
        card.Controls.Add(phoneStatus);
        card.Controls.Add(pairButton);
        Controls.Add(card);
    }

    void AddAccountCard()
    {
        var card = CreateCard(35, 275, 510, 145);

        var title = CreateLabel("🍎  Apple Account", 20, 18, 18, true);

        var status = CreateLabel(
            "●  Not authenticated",
            20, 55, 11, false
        );

        var button = new Button
        {
            Text = "Authenticate with Apple",
            Location = new Point(20, 88),
            Width = 470,
            Height = 35
        };

        button.Click += (_, _) =>
        {
            status.Text = "●  Use Apple's authentication flow";
            status.ForeColor = Color.LightGreen;
        };

        card.Controls.Add(title);
        card.Controls.Add(status);
        card.Controls.Add(button);
        Controls.Add(card);
    }

    void AddSigningCard()
    {
        var card = CreateCard(35, 435, 510, 120);

        var title = CreateLabel("🔐  Signing", 20, 18, 18, true);

        var status = CreateLabel(
            "Ready",
            20, 58, 11, false
        );

        card.Controls.Add(title);
        card.Controls.Add(status);
        Controls.Add(card);
    }

    void AddCertificateCard()
    {
        var card = CreateCard(35, 570, 510, 120);

        var title = CreateLabel("📜  Certificates", 20, 18, 18, true);

        var status = CreateLabel(
            "No signing credentials loaded",
            20, 58, 11, false
        );

        var button = new Button
        {
            Text = "Refresh Certificate Info",
            Location = new Point(250, 50),
            Width = 235,
            Height = 35
        };

        button.Click += (_, _) =>
        {
            status.Text = "Checking local credentials...";
        };

        card.Controls.Add(title);
        card.Controls.Add(status);
        card.Controls.Add(button);
        Controls.Add(card);
    }

    void StartDeviceWatcher()
    {
        watcher = new ManagementEventWatcher(
            new WqlEventQuery(
                "SELECT * FROM Win32_DeviceChangeEvent"
            )
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
            phoneStatus.Text = "●  iPhone detected";
            phoneStatus.ForeColor = Color.LightGreen;
            pairButton.Enabled = true;
        }
        else
        {
            phoneStatus.Text = "●  Waiting for iPhone...";
            phoneStatus.ForeColor = Color.Gray;
            pairButton.Enabled = false;
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        watcher?.Stop();
        watcher?.Dispose();
        base.OnFormClosed(e);
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
}