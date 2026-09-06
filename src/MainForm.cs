using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OurSigner;

public class MainForm : Form
{
    readonly DeviceManager deviceManager = new();
    readonly ITunesManager iTunesManager = new();
    readonly IPAManager ipaManager = new();
    readonly SigningManager signingManager = new();

    Label deviceStatus = new();
    Label itunesStatus = new();
    Label ourSignStatus = new();
    Label activityStatus = new();
    Label ipaStatus = new();
    Label signingStatus = new();

    [DllImport("dwmapi.dll")]
    static extern int DwmSetWindowAttribute(
        IntPtr hwnd,
        int attribute,
        ref int value,
        int attributeSize);

    public MainForm()
    {
        Text = "OurSigner";
        Width = 1100;
        Height = 720;
        MinimumSize = new Size(950, 620);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(10, 11, 15);
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10);
        DoubleBuffered = true;

        ApplyWindowStyle();
        BuildUI();
        RefreshStatus();
    }

    void ApplyWindowStyle()
    {
        const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        const int DWMWCP_ROUND = 2;

        try
        {
            int preference = DWMWCP_ROUND;
            DwmSetWindowAttribute(
                Handle,
                DWMWA_WINDOW_CORNER_PREFERENCE,
                ref preference,
                sizeof(int));
        }
        catch
        {
        }
    }

    void BuildUI()
    {
        var sidebar = new GlassPanel
        {
            Location = new Point(18, 18),
            Size = new Size(220, ClientSize.Height - 36),
            Radius = 24,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left
        };

        sidebar.Controls.Add(new Label
        {
            Text = "OurSigner",
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(25, 25)
        });

        sidebar.Controls.Add(new Label
        {
            Text = "iOS sideloading",
            Font = new Font("Segoe UI", 9),
            AutoSize = true,
            Location = new Point(27, 58),
            ForeColor = Color.FromArgb(135, 138, 148)
        });

        Controls.Add(sidebar);

        var main = new Panel
        {
            Location = new Point(260, 18),
            Size = new Size(ClientSize.Width - 278, ClientSize.Height - 36),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            BackColor = Color.Transparent,
            AutoScroll = true
        };

        main.Controls.Add(new Label
        {
            Text = "Overview",
            Font = new Font("Segoe UI", 28, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(5, 10)
        });

        main.Controls.Add(new Label
        {
            Text = "Manage your iPhone and iOS apps.",
            Font = new Font("Segoe UI", 10),
            AutoSize = true,
            Location = new Point(8, 52),
            ForeColor = Color.FromArgb(135, 138, 148)
        });

        AddDeviceCard(main);
        AddOurSignCard(main);
        AddIpaCard(main);
        AddSigningCard(main);
        AddActivityCard(main);
        AddSystemCard(main);

        Controls.Add(main);
    }

    void AddDeviceCard(Panel parent)
    {
        var card = CreateCard(5, 90, 355, 190);
        AddTitle(card, "iPhone", 22, 20);

        deviceStatus = new Label
        {
            Text = "Checking...",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(22, 65)
        };

        card.Controls.Add(deviceStatus);

        card.Controls.Add(new Label
        {
            Text = "Connect your iPhone using USB.",
            Font = new Font("Segoe UI", 9),
            AutoSize = true,
            Location = new Point(23, 99),
            ForeColor = Color.FromArgb(125, 128, 138)
        });

        var refresh = ActionButton("Refresh", 22, 137, 110, 34);

        refresh.Click += (_, _) =>
        {
            RefreshStatus();
            RefreshSigningStatus();
            activityStatus.Text = "Device and signing status refreshed.";
        };

        card.Controls.Add(refresh);
        parent.Controls.Add(card);
    }

    void AddOurSignCard(Panel parent)
    {
        var card = CreateCard(378, 90, 410, 190);
        AddTitle(card, "OurSign", 22, 20);

        ourSignStatus = new Label
        {
            Text = "Ready",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(22, 65),
            ForeColor = Color.White
        };

        card.Controls.Add(ourSignStatus);

        card.Controls.Add(new Label
        {
            Text = "Install the OurSign iOS app.",
            Font = new Font("Segoe UI", 9),
            AutoSize = true,
            Location = new Point(23, 99),
            ForeColor = Color.FromArgb(125, 128, 138)
        });

        var install = ActionButton("Install OurSign", 22, 137, 170, 34);

        install.Click += (_, _) =>
        {
            if (!deviceManager.IsIPhoneConnected())
            {
                activityStatus.Text = "Connect an iPhone first.";
                return;
            }

            if (!iTunesManager.IsInstalled())
            {
                activityStatus.Text = "iTunes is required.";
                return;
            }

            SigningResult result = signingManager.PrepareSigning();

            if (!result.Ready)
            {
                activityStatus.Text = result.Message;
                ourSignStatus.Text = "Not ready";
                ourSignStatus.ForeColor = Color.FromArgb(255, 180, 90);
                return;
            }

            activityStatus.Text = "Signing requirements are ready.";
            ourSignStatus.Text = "Ready to sign";
            ourSignStatus.ForeColor = Color.LightGreen;
        };

        card.Controls.Add(install);
        parent.Controls.Add(card);
    }

    void AddIpaCard(Panel parent)
    {
        var card = CreateCard(5, 298, 783, 185);
        AddTitle(card, "IPA", 22, 20);

        var dropArea = new Panel
        {
            Location = new Point(22, 58),
            Size = new Size(739, 70),
            BackColor = Color.FromArgb(25, 27, 33)
        };

        dropArea.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using var pen = new Pen(Color.FromArgb(70, 72, 82), 1);
            using var path = RoundedRectangle(
                new Rectangle(0, 0, dropArea.Width - 1, dropArea.Height - 1),
                15);

            e.Graphics.DrawPath(pen, path);
        };

        ipaStatus = new Label
        {
            Text = "No IPA selected",
            AutoSize = true,
            Location = new Point(18, 25),
            ForeColor = Color.FromArgb(130, 133, 143)
        };

        dropArea.Controls.Add(ipaStatus);
        card.Controls.Add(dropArea);

        var choose = ActionButton("Choose IPA", 22, 140, 125, 34);

        choose.Click += (_, _) =>
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "iOS App (*.ipa)|*.ipa",
                Title = "Select IPA"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (ipaManager.SelectIPA(dialog.FileName))
                {
                    ipaStatus.Text = System.IO.Path.GetFileName(dialog.FileName);
                    ipaStatus.ForeColor = Color.White;
                    activityStatus.Text = "IPA selected.";
                }
            }
        };

        card.Controls.Add(choose);
        parent.Controls.Add(card);
    }

    void AddSigningCard(Panel parent)
    {
        var card = CreateCard(5, 496, 783, 185);
        AddTitle(card, "Signing", 22, 20);

        signingStatus = new Label
        {
            Text = "Checking...",
            AutoSize = false,
            Size = new Size(700, 70),
            Location = new Point(22, 58),
            ForeColor = Color.FromArgb(170, 173, 183)
        };

        card.Controls.Add(signingStatus);

        var refresh = ActionButton("Check Signing", 22, 135, 140, 34);

        refresh.Click += (_, _) =>
        {
            RefreshSigningStatus();
            activityStatus.Text = "Signing requirements checked.";
        };

        card.Controls.Add(refresh);
        parent.Controls.Add(card);
    }

    void AddActivityCard(Panel parent)
    {
        var card = CreateCard(5, 699, 500, 125);
        AddTitle(card, "Activity", 22, 20);

        activityStatus = new Label
        {
            Text = "OurSigner is ready.",
            AutoSize = false,
            Size = new Size(450, 45),
            Location = new Point(22, 60),
            ForeColor = Color.FromArgb(170, 173, 183)
        };

        card.Controls.Add(activityStatus);
        parent.Controls.Add(card);
    }

    void AddSystemCard(Panel parent)
    {
        var card = CreateCard(523, 699, 265, 125);
        AddTitle(card, "System", 22, 20);

        itunesStatus = new Label
        {
            Text = "Checking iTunes...",
            AutoSize = false,
            Size = new Size(220, 45),
            Location = new Point(22, 60),
            ForeColor = Color.FromArgb(170, 173, 183)
        };

        card.Controls.Add(itunesStatus);
        parent.Controls.Add(card);
    }

    GlassPanel CreateCard(int x, int y, int width, int height)
    {
        return new GlassPanel
        {
            Location = new Point(x, y),
            Size = new Size(width, height),
            Radius = 24
        };
    }

    void AddTitle(Control parent, string text, int x, int y)
    {
        parent.Controls.Add(new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(x, y)
        });
    }

    Button ActionButton(string text, int x, int y, int width, int height)
    {
        var button = new Button
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(width, height),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(65, 67, 77),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Cursor = Cursors.Hand
        };

        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(82, 84, 95);

        return button;
    }

    void RefreshStatus()
    {
        try
        {
            if (deviceManager.IsIPhoneConnected())
            {
                deviceStatus.Text = "iPhone detected";
                deviceStatus.ForeColor = Color.LightGreen;
            }
            else
            {
                deviceStatus.Text = "Not connected";
                deviceStatus.ForeColor = Color.FromArgb(190, 192, 200);
            }
        }
        catch
        {
            deviceStatus.Text = "Unable to check";
        }

        if (iTunesManager.IsInstalled())
        {
            itunesStatus.Text = "iTunes detected";
            itunesStatus.ForeColor = Color.LightGreen;
        }
        else
        {
            itunesStatus.Text = "iTunes required";
            itunesStatus.ForeColor = Color.FromArgb(255, 180, 90);
        }
    }

    void RefreshSigningStatus()
    {
        try
        {
            string? deviceIdentifier = null;

            SigningResult result =
                signingManager.PrepareSigning(deviceIdentifier);

            if (result.Ready)
            {
                signingStatus.Text =
                    $"Ready to sign\nCertificate: {result.CertificateSubject}\nProfile: {System.IO.Path.GetFileName(result.ProvisioningProfilePath)}";

                signingStatus.ForeColor = Color.LightGreen;
                ourSignStatus.Text = "Signing ready";
                ourSignStatus.ForeColor = Color.LightGreen;
            }
            else
            {
                signingStatus.Text = result.Message;
                signingStatus.ForeColor = Color.FromArgb(255, 180, 90);
                ourSignStatus.Text = "Not ready";
                ourSignStatus.ForeColor = Color.FromArgb(255, 180, 90);
            }
        }
        catch (Exception ex)
        {
            signingStatus.Text = $"Signing check failed: {ex.Message}";
            signingStatus.ForeColor = Color.FromArgb(255, 120, 120);
            ourSignStatus.Text = "Check failed";
            ourSignStatus.ForeColor = Color.FromArgb(255, 120, 120);
        }
    }

    GraphicsPath RoundedRectangle(Rectangle rectangle, int radius)
    {
        var path = new GraphicsPath();
        int d = radius * 2;

        path.AddArc(rectangle.X, rectangle.Y, d, d, 180, 90);
        path.AddArc(rectangle.Right - d, rectangle.Y, d, d, 270, 90);
        path.AddArc(rectangle.Right - d, rectangle.Bottom - d, d, d, 0, 90);
        path.AddArc(rectangle.X, rectangle.Bottom - d, d, d, 90, 90);
        path.CloseFigure();

        return path;
    }
}

public class GlassPanel : Panel
{
    public int Radius { get; set; } = 24;

    public GlassPanel()
    {
        DoubleBuffered = true;
        BackColor = Color.FromArgb(27, 29, 36);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var rectangle = new Rectangle(0, 0, Width - 1, Height - 1);
        var path = new GraphicsPath();
        int d = Radius * 2;

        path.AddArc(rectangle.X, rectangle.Y, d, d, 180, 90);
        path.AddArc(rectangle.Right - d, rectangle.Y, d, d, 270, 90);
        path.AddArc(rectangle.Right - d, rectangle.Bottom - d, d, d, 0, 90);
        path.AddArc(rectangle.X, rectangle.Bottom - d, d, d, 90, 90);
        path.CloseFigure();

        using var brush = new SolidBrush(Color.FromArgb(27, 29, 36));
        using var pen = new Pen(Color.FromArgb(52, 54, 63), 1);

        e.Graphics.FillPath(brush, path);
        e.Graphics.DrawPath(pen, path);
    }
}