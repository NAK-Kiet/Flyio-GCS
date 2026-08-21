using MissionPlanner.Controls;
using MissionPlanner.Properties;
using MissionPlanner.Utilities;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace MissionPlanner.GCSViews
{
    public partial class Help : MyUserControl, IActivate
    {
        public Help()
        {
            InitializeComponent();
            BuildHelpPage();
        }

        public void Activate()
        {
            try
            {
                CHK_showconsole.Checked = Settings.Instance.GetBoolean("showconsole");
            }
            catch
            {
            }

            if (Program.WindowsStoreApp)
            {
                BUT_betaupdate.Visible = false;
                BUT_updatecheck.Visible = false;
            }
        }

        public void BUT_updatecheck_Click(object sender, EventArgs e)
        {
            try
            {
                if (Program.WindowsStoreApp)
                {
                    return;
                }
                Utilities.Update.CheckForUpdate(true);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(ex.ToString(), Strings.ERROR);
            }
        }

        private void CHK_showconsole_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Instance["showconsole"] = CHK_showconsole.Checked.ToString();
        }

        private void Help_Load(object sender, EventArgs e)
        {
            ApplyHelpTheme();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://firmware.ardupilot.org/Tools/MissionPlanner/upgrade/ChangeLog.txt");
        }

        private void BUT_betaupdate_Click(object sender, EventArgs e)
        {
            try
            {
                Utilities.Update.dobeta = true;
                if (Control.ModifierKeys == Keys.Control)
                {
                    Utilities.Update.domaster = true;
                    CustomMessageBox.Show("This will update to MASTER release");
                }

                Utilities.Update.DoUpdate();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(ex.ToString(), Strings.ERROR);
            }
        }

        private void BuildHelpPage()
        {
            contentLayout.SuspendLayout();

            var overview = CreateCard("FLY.IO / DRDC GCS");
            AddText(overview, "Welcome to Fly.io", 20F, FontStyle.Bold, ThemeManager.TextPrimary);
            AddText(overview,
                "Ground control software for Unmanned Aerial Vehicles (UAV) and Unmanned Ground Vehicles (UGV).\n\n" +
                "Fly.io provides vehicle monitoring, mission planning, configuration, telemetry, and system integration tools.",
                11F, FontStyle.Regular, ThemeManager.TextSecondary);

            var services = CreateCard("ONLINE SERVICES");
            var serviceCards = CreateHorizontalPanel();
            serviceCards.Margin = new Padding(0);
            var dashboardCard = CreateServiceCard(
                "DRDC GCS Dashboard",
                "Monitor UAV fleets, manage missions, access system services,\nand use DRDC GCS web-based monitoring tools.",
                "OPEN DASHBOARD", OpenDashboard_Click);
            var partnershipsCard = CreateServiceCard(
                "Brands & Collaborative Partnerships",
                "Explore collaborative projects, products, partnerships,\nand related services.",
                "VIEW PARTNERSHIPS", OpenPartnerships_Click);
            serviceCards.Controls.Add(dashboardCard);
            serviceCards.Controls.Add(partnershipsCard);
            serviceCards.SizeChanged += (sender, args) => LayoutServiceCards(serviceCards, dashboardCard, partnershipsCard);
            AddRow(services, serviceCards);

            var shortcuts = CreateCard("QUICK HELP & KEYBOARD SHORTCUTS");
            AddShortcutGroup(shortcuts, "MAIN SCREEN", new[]
            {
                "F2|Flight Data", "F3|Flight Planner", "F4|Tuning", "F5|Refresh Full Parameter List",
                "Ctrl+F|Temp Screen", "Ctrl+T|Blind Connect"
            });
            AddShortcutGroup(shortcuts, "FLIGHT DATA", new[]
            {
                "Ctrl+1|Tab 1", "Ctrl+2|Tab 2", "Ctrl+3|Tab 3", "Ctrl+4|Tab 4", "Ctrl+5|Tab 5",
                "Ctrl+6|Tab 6", "Ctrl+7|Tab 7", "Ctrl+8|Tab 8", "Ctrl+9|Tab 9", "Ctrl+0|Tab 10"
            });
            AddShortcutGroup(shortcuts, "FLIGHT PLANNER", new[]
            {
                "Ctrl+Z|Undo", "Ctrl+O|Open waypoint file", "Ctrl+S|Save waypoint file"
            });
            AddShortcutGroup(shortcuts, "AUTOWP GRID", new[]
            {
                "Ctrl+O|Open grid file", "Ctrl+S|Save grid file"
            });

            var updates = CreateCard("SYSTEM & UPDATES");
            var updateActions = CreateHorizontalPanel();
            updateActions.Controls.Add(BUT_updatecheck);
            updateActions.Controls.Add(BUT_betaupdate);
            updateActions.Controls.Add(CHK_showconsole);
            AddRow(updates, updateActions);
            AddRow(updates, linkLabel1);

            var about = CreateCard("ABOUT & OPEN SOURCE", true);
            AddKeyValue(about, "Product", "Fly.io");
            AddKeyValue(about, "Based on", "ArduPilot Mission Planner");
            AddLinkRow(about, "Mission Planner upstream", "https://github.com/ArduPilot/MissionPlanner");
            AddLinkRow(about, "Upstream contributors", "https://github.com/ArduPilot/MissionPlanner/graphs/contributors");
            AddText(about,
                "Historical and upstream contributors include Andrew Radford, Marooned, Ryan Beall, Tom Pittenger, and the wider Mission Planner contributor community. Design: Samantha Nelson. ArduCopter illustrations: Max Levine.",
                9F, FontStyle.Regular, ThemeManager.TextSecondary);
            AddText(about, "Libraries", 9F, FontStyle.Bold, ThemeManager.TextSecondary, new Padding(0, 3, 0, 2));
            AddText(about,
                "GMap.NET  •  SharpKML  •  SharpZipLib  •  IronPython  •  KMLib  •  OpenTK  •  ZedGraph  •  alglib  •  BouncyCastle  •  DotSpatial  •  LibVLC  •  netDXF",
                9F, FontStyle.Regular, ThemeManager.TextSecondary, new Padding(0, 0, 0, 4));
            AddText(about, "Organizations that contributed to the upstream Mission Planner ecosystem include:  3D Robotics  •  Falcon Unmanned  •  UAV Solutions",
                9F, FontStyle.Regular, ThemeManager.TextSecondary, new Padding(0, 2, 0, 4));
            AddInlineLinks(about, new[]
            {
                new LinkItem("ArduPlane documentation", "http://ardupilot.org/plane"),
                new LinkItem("ArduCopter documentation", "http://ardupilot.org/copter"),
                new LinkItem("ArduRover documentation", "http://ardupilot.org/rover")
            });

            contentLayout.ResumeLayout(true);
            LayoutServiceCards(serviceCards, dashboardCard, partnershipsCard);
            ApplyHelpTheme();
        }

        private HelpCard CreateCard(string title, bool secondary = false)
        {
            var card = new HelpCard
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = ThemeManager.ControlBackground,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 0, 0, 12),
                Padding = secondary ? new Padding(16) : new Padding(18),
                ColumnCount = 1
            };
            card.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            AddText(card, title, secondary ? 11F : 14F, FontStyle.Bold,
                secondary ? ThemeManager.TextSecondary : ThemeManager.TextPrimary);
            var accent = new Panel { BackColor = ThemeManager.Accent, Dock = DockStyle.Top, Height = secondary ? 1 : 2, Margin = new Padding(0, 3, 0, secondary ? 8 : 10) };
            AddRow(card, accent);
            contentLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            contentLayout.Controls.Add(card, 0, contentLayout.RowCount++);
            return card;
        }

        private HelpCard CreateServiceCard(string title, string description, string buttonText, EventHandler handler)
        {
            var card = new HelpCard
            {
                BackColor = ThemeManager.PanelBackground,
                ColumnCount = 1,
                Height = 150,
                Margin = new Padding(0, 0, 0, 8),
                Padding = new Padding(16)
            };
            card.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            card.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            card.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            card.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            card.Controls.Add(CreateLabel(title, 12F, FontStyle.Bold, ThemeManager.TextPrimary), 0, 0);
            var descriptionLabel = CreateLabel(description, 10F, FontStyle.Regular, ThemeManager.TextSecondary);
            descriptionLabel.Dock = DockStyle.Fill;
            descriptionLabel.Margin = new Padding(0, 8, 0, 8);
            card.Controls.Add(descriptionLabel, 0, 1);
            card.Controls.Add(CreateActionButton(buttonText, handler, 180), 0, 2);
            return card;
        }

        private void LayoutServiceCards(FlowLayoutPanel host, Control first, Control second)
        {
            var availableWidth = host.ClientSize.Width;
            if (availableWidth <= 0)
                return;

            const int gap = 12;
            var sideBySide = availableWidth >= 700;
            var cardWidth = sideBySide ? (availableWidth - gap) / 2 : availableWidth;
            first.Margin = sideBySide ? new Padding(0, 0, gap, 8) : new Padding(0, 0, 0, 8);
            second.Margin = new Padding(0, 0, 0, 8);
            first.Width = Math.Max(260, cardWidth);
            second.Width = Math.Max(260, cardWidth);
        }

        private void AddKeyValue(TableLayoutPanel card, string key, string value)
        {
            AddText(card, key + ":  " + value, 10F, FontStyle.Regular, ThemeManager.TextSecondary, new Padding(0, 2, 0, 4));
        }

        private void AddLinkRow(TableLayoutPanel card, string key, string url)
        {
            var row = CreateHorizontalPanel();
            row.Controls.Add(CreateLabel(key + ":", 10F, FontStyle.Bold, ThemeManager.TextPrimary));
            row.Controls.Add(CreateLink(url, url));
            AddRow(card, row);
        }

        private void AddInlineLinks(TableLayoutPanel card, LinkItem[] links)
        {
            var row = CreateHorizontalPanel();
            foreach (var item in links)
                row.Controls.Add(CreateLink(item.Text, item.Url));
            AddRow(card, row);
        }

        private void AddShortcutGroup(TableLayoutPanel card, string heading, string[] shortcuts)
        {
            AddText(card, heading, 10F, FontStyle.Bold, ThemeManager.AccentHover, new Padding(0, 6, 0, 3));
            var grid = new TableLayoutPanel { AutoSize = true, Dock = DockStyle.Top, ColumnCount = 2, Margin = new Padding(0, 0, 0, 4) };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            foreach (var shortcut in shortcuts)
            {
                var parts = shortcut.Split('|');
                grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                grid.Controls.Add(CreateLabel(parts[0], 10F, FontStyle.Bold, ThemeManager.TextPrimary), 0, grid.RowCount);
                grid.Controls.Add(CreateLabel(parts[1], 10F, FontStyle.Regular, ThemeManager.TextSecondary), 1, grid.RowCount++);
            }
            AddRow(card, grid);
        }

        private void AddText(TableLayoutPanel card, string text, float size, FontStyle style, Color color, Padding? margin = null)
        {
            var label = CreateLabel(text, size, style, color);
            label.Dock = DockStyle.Fill;
            label.Margin = margin ?? new Padding(0, 2, 0, 6);
            AddRow(card, label);
        }

        private Label CreateLabel(string text, float size, FontStyle style, Color color)
        {
            return new Label
            {
                AutoSize = true,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", size, style),
                ForeColor = color,
                Margin = new Padding(0, 2, 10, 2),
                Text = text
            };
        }

        private LinkLabel CreateLink(string text, string url)
        {
            var link = new LinkLabel
            {
                AutoSize = true,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                LinkColor = ThemeManager.AccentHover,
                ActiveLinkColor = ThemeManager.TextOnAccent,
                VisitedLinkColor = ThemeManager.AccentHover,
                Margin = new Padding(0, 2, 22, 6),
                Text = text,
                Tag = url
            };
            link.LinkClicked += ExternalLink_LinkClicked;
            return link;
        }

        private FlowLayoutPanel CreateHorizontalPanel()
        {
            return new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.Transparent,
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = new Padding(0, 2, 0, 8),
                WrapContents = true
            };
        }

        private MyButton CreateActionButton(string text, EventHandler handler, int width)
        {
            var button = new MyButton { Text = text, Size = new Size(width, 38), Margin = new Padding(0, 0, 12, 0) };
            button.Click += handler;
            StyleActionButton(button);
            return button;
        }

        private void StyleActionButton(MyButton button)
        {
            button.BackColor = ThemeManager.ControlBackground;
            button.BGGradTop = ThemeManager.Accent;
            button.BGGradBot = ThemeManager.Accent;
            button.ColorMouseOver = ThemeManager.AccentHover;
            button.ColorMouseDown = ThemeManager.AccentPressed;
            button.ColorNotEnabled = ThemeManager.ControlBackground;
            button.Outline = ThemeManager.ControlBorder;
            button.TextColor = ThemeManager.TextOnAccent;
            button.TextColorNotEnabled = ThemeManager.TextSecondary;
        }

        private void AddRow(TableLayoutPanel panel, Control control)
        {
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.Controls.Add(control, 0, panel.RowCount++);
        }

        private void ApplyHelpTheme()
        {
            BackColor = ThemeManager.PanelBackground;
            helpScrollPanel.BackColor = ThemeManager.PanelBackground;
            contentLayout.BackColor = ThemeManager.PanelBackground;
            CHK_showconsole.BackColor = Color.Transparent;
            CHK_showconsole.ForeColor = ThemeManager.TextSecondary;
            linkLabel1.LinkColor = ThemeManager.AccentHover;
            linkLabel1.ActiveLinkColor = ThemeManager.TextOnAccent;
            linkLabel1.VisitedLinkColor = ThemeManager.AccentHover;
            StyleActionButton(BUT_updatecheck);
            StyleActionButton(BUT_betaupdate);
        }

        private void OpenDashboard_Click(object sender, EventArgs e)
        {
            OpenUrl("https://drdt-dev-gcs-frontend.drsys.asia/en/login");
        }

        private void OpenPartnerships_Click(object sender, EventArgs e)
        {
            OpenUrl("https://aerobyte-dev-customer.drsys.asia/");
        }

        private void ExternalLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var link = sender as LinkLabel;
            if (link != null && link.Tag is string)
                OpenUrl((string)link.Tag);
        }

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Failed to open URL:\n" + url + "\n\n" + ex.Message, Strings.ERROR);
            }
        }

        private sealed class LinkItem
        {
            public LinkItem(string text, string url) { Text = text; Url = url; }
            public string Text { get; private set; }
            public string Url { get; private set; }
        }

        [PreventTheming]
        private sealed class HelpCard : TableLayoutPanel
        {
            public HelpCard()
            {
                DoubleBuffered = true;
                SetStyle(ControlStyles.ResizeRedraw, true);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                using (var pen = new Pen(ThemeManager.ControlBorder))
                    e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }
    }
}
