using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Management;
using System.Runtime.InteropServices;
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
    Label deviceValue = new();
    Label signingValue = new();
    Label ipaValue = new();
    Label activityValue = new();
    Button installButton = new();
    ManagementEventWatcher? watcher;

    [DllImport("dwmapi.dll")]
    static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

    public MainForm()
    {
        Text = "OurSigner";
        Width = 1050;
        Height = 700;
        MinimumSize = new Size(900, 600);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(13, 14, 18);
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10);
        DoubleBuffered = true;

        int darkMode = 1;
        DwmSetWindowAttribute(Handle, 20, ref darkMode, sizeof(int));

        BuildInterface();
        StartDeviceWatcher();
        CheckForIPhone();
    }

    void BuildInterface()
    {
        var sidebar = new GlassPanel
        {
            Location = new Point(18, 18),
            Size = new Size(215, 625),
            Radius = 24
        };

        var logo = new Label
        {
            Text = "OurSigner",
            Font = new Font("Segoe UI", 19, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(25, 25),
            ForeColor = Color.White
        };

        var version = new Label
        {
            Text = "iOS sideloading",
            Font = new Font("Segoe UI", 9),
            AutoSize = true,
            Location = new Point(27, 56),
            ForeColor = Color.FromArgb(145, 147, 156)
        };

        sidebar.Controls.Add(logo);
        sidebar.Controls.Add(version);

        AddNavButton(sidebar, "Overview", 105, true);
        AddNavButton(sidebar, "Devices", 155, false);
        AddNavButton(sidebar, "Library", 205, false);
        AddNavButton(sidebar, "Signing", 255, false);
        AddNavButton(sidebar, "Settings", 305, false);

        var footer = new Label
        {
            Text = "OurSigner",
            Font = new Font("Segoe UI", 8),
            AutoSize = true,
            Location = new Point(27, 585),
            ForeColor = Color.FromArgb(100, 102, 110)
        };

        sidebar.Controls.Add(footer);
        Controls.Add(sidebar);

        var heading = new Label
        {
            Text = "Overview",
            Font = new Font("Segoe UI", 27, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(265, 28),
            ForeColor = Color.White
        };

        var subheading = new Label
        {
            Text = "Everything you need to manage your iOS apps.",
            Font = new Font("Segoe UI", 10),
            AutoSize = true,
            Location = new Point(269, 69),
            ForeColor = Color.FromArgb(145, 147, 156)
        };

        Controls.Add(heading);
        Controls.Add(subheading);

        AddDeviceCard();
        AddIpaCard();
        AddSigningCard();
        AddActivityCard();
    }

    void AddNavButton(Control parent, string text, int y, bool selected)
    {
        var button = new Button
        {
            Text = text,
            Location = new Point(14, y),
            Size = new Size(187, 40),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, selected ? FontStyle.Bold : FontStyle.Regular),
            ForeColor = selected ? Color.White : Color.FromArgb(150, 152, 160),
            BackColor = selected ? Color.FromArgb(55, 57, 66) : Color.Transparent,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(14, 0, 0, 0),
            Cursor = Cursors.Hand
        };

        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(48, 50, 58);

        parent.Controls.Add(button);
    }

    void AddDeviceCard()
    {
        var card = CreateGlassCard(265, 115, 340, 190);

        AddCardTitle(card, "iPhone", 22, 20);

        deviceValue = new Label
        {
            Text = "Not connected",
            Font = new Font("Segoe UI", 17, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(22, 65),
            ForeColor = Color.FromArgb(190, 192, 200)
        };

        var detail = new Label
        {
            Text = "Connect your iPhone using USB.",
            Font = new Font("Segoe UI", 9),
            AutoSize = true,
            Location = new Point(23, 99),
            ForeColor = Color.FromArgb(125, 127, 135)
        };

        var pair = CreateActionButton("Pair", 22, 137, 95, 34);

        pair.Click += (_, _) =>
        {
            deviceValue.Text = "Connected";
            deviceValue.ForeColor = Color.LightGreen;
            activityValue.Text = "iPhone connection confirmed.";
        };

        card.Controls.Add(deviceValue);
        card.Controls.Add(detail);
        card.Controls.Add(pair);
        Controls.Add(card);
    }

    void AddIpaCard()
    {
        var card = CreateGlassCard(625, 115, 375, 190);

        AddCardTitle(card, "IPA", 22, 20);

        ipaValue = new Label
        {
            Text = "No app selected",
            Font = new Font("Segoe UI", 15, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(22, 65),
            ForeColor = Color.FromArgb(190, 192, 200)
        };

        var choose = CreateActionButton("Choose IPA", 22, 105, 140, 36);

        choose.Click += (_, _) =>
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "iOS App (*.ipa)|*.ipa",
                Title = "Select IPA"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                ipaValue.Text = System.IO.Path.GetFileName(dialog.FileName);
                ipaValue.ForeColor = Color.White;
                installButton.Enabled = true;
                activityValue.Text = "IPA selected.";
            }
        };

        installButton = CreateActionButton("Install", 172, 105, 140, 36);
        installButton.Enabled = false;

        installButton.Click += (_, _) =>
        {
            signingValue.Text = "Ready";
            signingValue.ForeColor = Color.LightGreen;
            activityValue.Text = "Signing workflow ready.";
        };

        card.Controls.Add(ipaValue);
        card.Controls.Add(choose);
        card.Controls.Add(installButton);
        Controls.Add(card);
    }

    void AddSigningCard()
    {
        var card = CreateGlassCard(265, 325, 340, 150);

        AddCardTitle(card, "Signing", 22, 20);

        signingValue = new Label
        {
            Text = "Waiting for IPA",
            Font = new Font("Segoe UI", 15, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(22, 67),
            ForeColor = Color.FromArgb(190, 192, 200)
        };

        var detail = new Label
        {
            Text = "No signing operation running.",
            Font = new Font("Segoe UI", 9),
            AutoSize = true,
            Location = new Point(23, 101),
            ForeColor = Color.FromArgb(125, 127, 135)
        };

        card.Controls.Add(signingValue);
        card.Controls.Add(detail);
        Controls.Add(card);
    }

    void AddActivityCard()
    {
        var card = CreateGlassCard(625, 325, 375, 150);

        AddCardTitle(card, "Activity", 22, 20);

        activityValue = new Label
        {
            Text = "OurSigner is ready.",
            Font = new Font("Segoe UI", 10),
            AutoSize = false,
            Size = new Size(320, 55),
            Location = new Point(22, 67),
            ForeColor = Color.FromArgb(170, 172, 180)
        };

        card.Controls.Add(activityValue);
        Controls.Add(card);
    }

    GlassPanel CreateGlassCard(int x, int y, int width, int height)
    {
        var panel = new GlassPanel
        {
            Location = new Point(x, y),
            Size = new Size(width, height),
            Radius = 24
        };

        Controls.Add(panel);
        return panel;
    }

    void AddCardTitle(Control parent, string text, int x, int y)
    {
        parent.Controls.Add(new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(x, y),
            ForeColor = Color.White
        });
    }

    Button CreateActionButton(string text, int x, int y, int width, int height)
    {
        var button = new Button
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(width, height),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(63, 65, 75),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Cursor = Cursors.Hand
        };

        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(82, 84, 96);

        return button;
    }

    void StartDeviceWatcher()
    {
        watcher = new ManagementEventWatcher(
            new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent")
        );

        watcher.EventArrived += (_, _) =>
        {
            if (!IsDisposed)
                BeginInvoke(CheckForIPhone);
        };

        watcher.Start();
    }

    void CheckForIPhone()
    {
        try
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
                deviceValue.Text = "iPhone detected";
                deviceValue.ForeColor = Color.LightGreen;
                activityValue.Text = "iPhone detected over USB.";
            }
        }
        catch
        {
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        watcher?.Stop();
        watcher?.Dispose();
        base.OnFormClosed(e);
    }
}

class GlassPanel : Panel
{
    public int Radius { get; set; } = 24;

    public GlassPanel()
    {
        DoubleBuffered = true;
        BackColor = Color.FromArgb(25, 27, 33);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        using var path = RoundedPath(new Rectangle(0, 0, Width - 1, Height - 1), Radius);
        using var brush = new SolidBrush(Color.FromArgb(25, 27, 33));
        using var border = new Pen(Color.FromArgb(55, 57, 65), 1);

        e.Graphics.FillPath(brush, path);
        e.Graphics.DrawPath(border, path);
    }

    GraphicsPath RoundedPath(Rectangle rectangle, int radius)
    {
        var path = new GraphicsPath();
        int diameter = radius * 2;

        path.AddArc(rectangle.X, rectangle.Y, diameter, diameter, 180, 90);
        path.AddArc(rectangle.Right - diameter, rectangle.Y, diameter, diameter, 270, 90);
        path.AddArc(rectangle.Right - diameter, rectangle.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rectangle.X, rectangle.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();

        return path;
    }
}