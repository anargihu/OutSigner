using System;
using System.Drawing;
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
    Label accountStatus = new();
    Label certificateStatus = new();
    Label signingStatus = new();
    Button pairButton = new();

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
    }

    void AddPhoneCard()
    {
        var card = CreateCard(35, 115, 510, 145);

        var title = CreateLabel("📱  iPhone", 20, 18, 18, true);
        phoneStatus = CreateLabel("●  Not detected", 20, 55, 11, false);

        pairButton = new Button
        {
            Text = "Detect iPhone",
            Location = new Point(20, 88),
            Width = 470,
            Height = 35
        };

        pairButton.Click += (_, _) =>
        {
            phoneStatus.Text = "●  iPhone detected";
            phoneStatus.ForeColor = Color.LightGreen;
            pairButton.Text = "Pair iPhone";
            pairButton.Click -= (_, _) => { };
            pairButton.Click += (_, _) =>
            {
                phoneStatus.Text = "●  Paired";
                pairButton.Text = "Paired ✓";
            };
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

        accountStatus = CreateLabel(
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
            accountStatus.Text = "●  Authentication handled by Apple";
            accountStatus.ForeColor = Color.LightGreen;
        };

        card.Controls.Add(title);
        card.Controls.Add(accountStatus);
        card.Controls.Add(button);
        Controls.Add(card);
    }

    void AddSigningCard()
    {
        var card = CreateCard(35, 435, 510, 120);

        var title = CreateLabel("🔐  Signing", 20, 18, 18, true);

        signingStatus = CreateLabel(
            "Ready",
            20, 58, 11, false
        );

        card.Controls.Add(title);
        card.Controls.Add(signingStatus);
        Controls.Add(card);
    }

    void AddCertificateCard()
    {
        var card = CreateCard(35, 570, 510, 120);

        var title = CreateLabel("📜  Certificates", 20, 18, 18, true);

        certificateStatus = CreateLabel(
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
            certificateStatus.Text = "Checking local signing credentials...";
            signingStatus.Text = "Checking...";
        };

        card.Controls.Add(title);
        card.Controls.Add(certificateStatus);
        card.Controls.Add(button);
        Controls.Add(card);
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