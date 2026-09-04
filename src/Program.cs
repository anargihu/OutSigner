using System;
using System.Drawing;
using System.Drawing.Drawing2D;
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
    Label ourSignStatus = new();
    Label activityStatus = new();
    Button installOurSignButton = new();
    ManagementEventWatcher? deviceWatcher;

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

        BuildUI();
        StartDeviceWatcher();
        CheckDevices();
        CheckITunes();
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

        AddNavigation(sidebar);

        sidebar.Controls.Add(new Label
        {
            Text = "Windows",
            Font = new Font("Segoe UI", 8),
            AutoSize = true,
            Location = new Point(27, sidebar.Height - 35),
            ForeColor = Color.FromArgb(85, 88, 98)
        });

        Controls.Add(sidebar);

        var main = new Panel
        {
            Location = new Point(260, 18),
            Size = new Size(ClientSize.Width - 278, ClientSize.Height - 36),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            BackColor = Color.Transparent
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
        AddActivityCard(main);
        AddSystemCard(main);

        Controls.Add(main);
    }

    void AddNavigation(Panel sidebar)
    {
        string[] items =
        {
            "Overview",
            "Devices",
            "Apps",
            "Signing",
            "Settings"
        };

        for (int i = 0; i < items.Length; i++)
        {
            var button = new Button
            {
                Text = items[i],
                Location = new Point(14, 110 + i * 48),
                Size = new Size(192, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = i == 0 ? Color.FromArgb(48, 50, 58) : Color.Transparent,
                ForeColor = i == 0 ? Color.White : Color.FromArgb(145, 148, 158),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(14, 0, 0, 0),
                Font = new Font("Segoe UI", 10, i == 0 ? FontStyle.Bold : FontStyle.Regular),
                Cursor = Cursors.Hand
            };

            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(55, 57, 65);

            sidebar.Controls.Add(button);
        }
    }

    void AddDeviceCard(Panel parent)
    {
        var card = CreateCard(5, 90, 355, 190);
        AddTitle(card, "iPhone", 22, 20);

        deviceStatus = new Label
        {
            Text = "Not connected",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(22, 65),
            ForeColor = Color.FromArgb(190, 192, 200)
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
            CheckDevices();
            activityStatus.Text = "Device status refreshed.";
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
            Text = "Not installed",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(22, 65),
            ForeColor = Color.FromArgb(190, 192, 200)
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

        installOurSignButton = ActionButton(
            "Install OurSign",
            22,
            137,
            170,
            34
        );

        installOurSignButton.Click += (_, _) =>
        {
            if (!IsIPhoneConnected())
            {
                activityStatus.Text = "Connect an iPhone first.";
                return;
            }

            activityStatus.Text = "OurSign installation requested.";
            ourSignStatus.Text = "Ready to install";
            ourSignStatus.ForeColor = Color.White;
        };

        card.Controls.Add(installOurSignButton);
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
            using var pen = new Pen(Color.FromArgb(70, 72, 82), 1);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using var path = RoundedRectangle(
                new Rectangle(0, 0, dropArea.Width - 1, dropArea.Height - 1),
                15
            );

            e.Graphics.DrawPath(pen, path);
        };

        var ipaLabel = new Label
        {
            Text = "No IPA selected",
            AutoSize = true,
            Location = new Point(18, 25),
            ForeColor = Color.FromArgb(130, 133, 143),
            Font = new Font("Segoe UI", 10)
        };

        dropArea.Controls.Add(ipaLabel);
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
                ipaLabel.Text = System.IO.Path.GetFileName(dialog.FileName);
                ipaLabel.ForeColor = Color.White;
                activityStatus.Text = "IPA selected.";
            }
        };

        card.Controls.Add(choose);
        parent.Controls.Add(card);
    }

    void AddActivityCard(Panel parent)
    {
        var card = CreateCard(5, 501, 500, 125);
        AddTitle(card, "Activity", 22, 20);

        activityStatus = new Label
        {
            Text = "OurSigner is ready.",
            AutoSize = false,
            Size = new Size(450, 45),
            Location = new Point(22, 60),
            ForeColor = Color.FromArgb(170, 173, 183),
            Font = new Font("Segoe UI", 10)
        };

        card.Controls.Add(activityStatus);
        parent.Controls.Add(card);
    }

    void AddSystemCard(Panel parent)
    {
        var card = CreateCard(523, 501, 265, 125);
        AddTitle(card, "System", 22, 20);

        itunesStatus = new Label
        {
            Text = "Checking iTunes...",
            AutoSize = false,
            Size = new Size(220, 45),
            Location = new Point(22, 60),
            ForeColor = Color.FromArgb(170, 173, 183),
            Font = new Font("Segoe UI", 10)
        };

        card.Controls.Add(itunesStatus);
        parent.Controls.Add(card);
    }

    GlassPanel CreateCard(int x, int y, int width, int height)
    {
        var card = new GlassPanel
        {
            Location = new Point(x, y),
            Size = new Size(width, height),
            Radius = 24
        };

        return card;
    }

    void AddTitle(Control parent, string text, int x, int y)
    {
        parent.Controls.Add(new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(x, y),
            ForeColor = Color.White
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

    bool IsIPhoneConnected()
    {
        using var searcher = new ManagementObjectSearcher(
            "SELECT Name, Manufacturer, PNPDeviceID FROM Win32_PnPEntity"
        );

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
                return true;
            }
        }

        return false;
    }

    void CheckDevices()
    {
        try
        {
            if (IsIPhoneConnected())
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
    }

    void CheckITunes()
    {
        string[] paths =
        {
            @"C:\Program Files\iTunes\iTunes.exe",
            @"C:\Program Files (x86)\iTunes\iTunes.exe"
        };

        foreach (string path in paths)
        {
            if (System.IO.File.Exists(path))
            {
                itunesStatus.Text = "iTunes detected";
                itunesStatus.ForeColor = Color.LightGreen;
                return;
            }
        }

        itunesStatus.Text = "iTunes required";
        itunesStatus.ForeColor = Color.FromArgb(255, 180, 90);
    }

    void StartDeviceWatcher()
    {
        deviceWatcher = new ManagementEventWatcher(
            new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent")
        );

        deviceWatcher.EventArrived += (_, _) =>
        {
            if (!IsDisposed)
                BeginInvoke(CheckDevices);
        };

        deviceWatcher.Start();
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

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        deviceWatcher?.Stop();
        deviceWatcher?.Dispose();
        base.OnFormClosed(e);
    }
}

class GlassPanel : Panel
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