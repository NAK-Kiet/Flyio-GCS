using MissionPlanner.Controls;
using MissionPlanner.Properties;
using MissionPlanner.Utilities;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace MissionPlanner.GCSViews
{
    [PreventTheming]
    public partial class Help : MyUserControl, IActivate
    {
        private const string GitHubUrl = "https://github.com/NAK-Kiet/Flyio-GCS";
        private const string GitLabRemote = "git@192.168.100.120:Kaine/flyio-gcs.git";
        private const int CommunityPartnersTimerInterval = 25;
        private const int CommunityPartnersPixelsPerTick = 1;
        private const int CommunityPartnerHorizontalMargin = 24;
        private static readonly Color HelpPageBackground = Color.White;
        private static readonly Color HelpCardBackground = Color.FromArgb(248, 249, 250);
        private static readonly Color HelpInnerCardBackground = Color.White;
        private static readonly Color HelpBorder = Color.FromArgb(218, 221, 225);
        private static readonly Color HelpTextPrimary = Color.FromArgb(17, 17, 17);
        private static readonly Color HelpTextSecondary = Color.FromArgb(75, 85, 99);
        private static readonly Color HelpTextMuted = Color.FromArgb(107, 114, 128);
        private static readonly Color HelpHeroAccent = Color.FromArgb(255, 90, 0);
        private static readonly Color HelpButton = Color.Black;
        private static readonly Color HelpButtonHover = Color.FromArgb(31, 31, 31);
        private static readonly Color HelpButtonPressed = Color.FromArgb(51, 51, 51);
        private Timer communityPartnersTimer;
        private MarqueeViewport communityPartnersViewport;
        private MarqueeTrack communityPartnersTrack;
        private int communityPartnersSequenceWidth;
        private int communityPartnersOffset;

        public Help()
        {
            InitializeComponent();
            BuildHelpPage();
        }

        public void Activate()
        {
            try { CHK_showconsole.Checked = Settings.Instance.GetBoolean("showconsole"); } catch { }
            if (Program.WindowsStoreApp)
            {
                BUT_betaupdate.Visible = false;
                BUT_updatecheck.Visible = false;
            }
            UpdateCommunityPartnersTimer();
        }

        public void BUT_updatecheck_Click(object sender, EventArgs e)
        {
            try
            {
                if (Program.WindowsStoreApp) return;
                Utilities.Update.CheckForUpdate(true);
            }
            catch (Exception ex) { CustomMessageBox.Show(ex.ToString(), Strings.ERROR); }
        }

        private void CHK_showconsole_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Instance["showconsole"] = CHK_showconsole.Checked.ToString();
        }

        private void Help_Load(object sender, EventArgs e) { ApplyHelpTheme(); }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenUrl("https://firmware.ardupilot.org/Tools/MissionPlanner/upgrade/ChangeLog.txt");
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
            catch (Exception ex) { CustomMessageBox.Show(ex.ToString(), Strings.ERROR); }
        }

        private void BuildHelpPage()
        {
            contentLayout.SuspendLayout();
            contentLayout.Controls.Clear();
            contentLayout.RowStyles.Clear();
            contentLayout.RowCount = 0;
            BuildHero();
            BuildOnlineServices();
            BuildShortcutSection();
            BuildUpdatesSection();
            BuildAboutSection();
            BuildCommunityPartnersSection();
            contentLayout.ResumeLayout(true);
            ApplyHelpTheme();
        }

        private void BuildHero()
        {
            var hero = CreateSurface(new Padding(24, 22, 24, 22), new Padding(0, 0, 0, 14));
            var copy = new TableLayoutPanel
            {
                AutoSize = true, BackColor = Color.Transparent, ColumnCount = 1,
                Dock = DockStyle.Fill, Margin = new Padding(0)
            };
            copy.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            hero.BackColor = HelpPageBackground;
            AddText(copy, "FLY.IO / DRDC GCS", 10F, FontStyle.Bold, HelpHeroAccent, new Padding(0, 0, 0, 7));
            AddText(copy, "Welcome to Fly.io", 22F, FontStyle.Bold, HelpTextPrimary, new Padding(0, 0, 0, 8));
            AddText(copy, "Ground Control Station for UAV and UGV systems.", 12F, FontStyle.Bold,
                HelpTextPrimary, new Padding(0, 0, 0, 9));
            AddText(copy,
                "Fly.io provides vehicle monitoring, mission planning, configuration, telemetry, embedded DRDC services, and system integration tools.",
                10.5F, FontStyle.Regular, HelpTextSecondary, new Padding(0));
            hero.Controls.Add(copy, 0, 0);
            AddContent(hero);
        }

        private void BuildOnlineServices()
        {
            var section = CreateSection("ONLINE SERVICES", "Connected Fly.io services and collaboration resources.");
            var host = CreateWrapPanel();
            host.AutoSize = false;
            host.Height = 155;
            var dashboard = CreateServiceCard("DRDC GCS Dashboard",
                "Monitor UAV fleets, manage missions, and access embedded DRDC system services.",
                "OPEN DASHBOARD", OpenDashboard_Click);
            var partnerships = CreateServiceCard("Brands & Partnerships",
                "Explore collaborative projects, products, partnerships, and related services.",
                "VIEW PARTNERSHIPS", OpenPartnerships_Click);
            var cards = new Control[] { dashboard, partnerships };
            host.Controls.Add(dashboard);
            host.Controls.Add(partnerships);
            host.SizeChanged += delegate { LayoutCards(host, cards, 2, 330); };
            AddRow(section, host);
            LayoutCards(host, cards, 2, 330);
        }

        private void BuildShortcutSection()
        {
            var section = CreateSection("QUICK HELP & KEYBOARD SHORTCUTS",
                "Frequently used navigation and mission workflow commands.");
            var host = CreateWrapPanel();
            host.AutoSize = false;
            host.Height = 364;
            var cards = new Control[]
            {
                CreateShortcutCard("MAIN SCREEN", new[] { "F2|Flight Data", "F3|Flight Planner", "F4|Tuning",
                    "F5|Refresh Parameters", "Ctrl+F|Temp Screen", "Ctrl+T|Blind Connect" }),
                CreateShortcutCard("FLIGHT DATA", new[] { "Ctrl+1|Tab 1", "Ctrl+2|Tab 2", "Ctrl+3|Tab 3",
                    "Ctrl+4|Tab 4", "Ctrl+5|Tab 5", "Ctrl+6|Tab 6", "Ctrl+7|Tab 7", "Ctrl+8|Tab 8",
                    "Ctrl+9|Tab 9", "Ctrl+0|Tab 10" }),
                CreateShortcutCard("FLIGHT PLANNER", new[] { "Ctrl+Z|Undo", "Ctrl+O|Open waypoint file",
                    "Ctrl+S|Save waypoint file" }),
                CreateShortcutCard("AUTOWP GRID", new[] { "Ctrl+O|Open grid file", "Ctrl+S|Save grid file" })
            };
            foreach (var card in cards) host.Controls.Add(card);
            host.SizeChanged += delegate { LayoutCards(host, cards, 2, 300); };
            AddRow(section, host);
            LayoutCards(host, cards, 2, 300);
        }

        private void BuildUpdatesSection()
        {
            var section = CreateSection("SYSTEM & UPDATES", "Application maintenance and diagnostics.", true);
            var actions = CreateWrapPanel();
            actions.AutoSize = false;
            actions.Height = 42;
            BUT_updatecheck.Size = new Size(165, 34);
            BUT_betaupdate.Size = new Size(185, 34);
            BUT_updatecheck.Margin = new Padding(0, 0, 8, 0);
            BUT_betaupdate.Margin = new Padding(0, 0, 8, 0);
            CHK_showconsole.Margin = new Padding(4, 7, 0, 0);
            actions.Controls.Add(BUT_updatecheck);
            actions.Controls.Add(BUT_betaupdate);
            actions.Controls.Add(CHK_showconsole);
            AddRow(section, actions);
            linkLabel1.Text = "View Upstream Mission Planner Changelog";
            linkLabel1.Margin = new Padding(0, 2, 0, 0);
            AddRow(section, linkLabel1);
        }

        private void BuildAboutSection()
        {
            var section = CreateSection("ABOUT & OPEN SOURCE",
                "Product information, repositories, and upstream acknowledgements.");
            var facts = new TableLayoutPanel
            {
                AutoSize = true, BackColor = Color.Transparent, ColumnCount = 2,
                Dock = DockStyle.Top, Margin = new Padding(0, 2, 0, 12)
            };
            facts.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            facts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            AddFact(facts, "Product", "Fly.io Ground Control Station");
            AddFact(facts, "Based on", "ArduPilot Mission Planner");
            AddRow(section, facts);

            AddSubheading(section, "PROJECT REPOSITORIES");
            AddLinkRow(section, "Project Repository — GitHub", GitHubUrl);
            AddText(section, "Project Repository — GitLab (SSH remote)", 9.5F, FontStyle.Bold,
                HelpTextPrimary, new Padding(0, 5, 0, 2));
            AddText(section, GitLabRemote, 9.5F, FontStyle.Regular, HelpTextSecondary, new Padding(0, 0, 0, 3));
            AddText(section,
                "A browser URL is not shown because the configured private GitLab host does not provide a confirmed HTTP/HTTPS mapping in this repository.",
                8.5F, FontStyle.Italic, HelpTextMuted, new Padding(0, 0, 0, 12));

            AddSubheading(section, "UPSTREAM MISSION PLANNER");
            AddLinkRow(section, "Upstream Mission Planner", "https://github.com/ArduPilot/MissionPlanner");
            AddLinkRow(section, "Upstream Contributors", "https://github.com/ArduPilot/MissionPlanner/graphs/contributors");
            AddText(section,
                "Upstream Mission Planner contributors and community acknowledgements include Andrew Radford, Marooned, Ryan Beall, Tom Pittenger, and the Mission Planner contributor community.",
                9.5F, FontStyle.Regular, HelpTextSecondary, new Padding(0, 5, 0, 8));
            AddSubheading(section, "OPEN-SOURCE ECOSYSTEM ACKNOWLEDGEMENTS");
            AddText(section,
                "Libraries used by the upstream Mission Planner ecosystem include GMap.NET, SharpKML, SharpZipLib, IronPython, KMLib, OpenTK, ZedGraph, alglib, BouncyCastle, DotSpatial, LibVLC, and netDXF.",
                9.5F, FontStyle.Regular, HelpTextSecondary, new Padding(0, 3, 0, 7));
            AddText(section,
                "Organizations historically contributing to the upstream Mission Planner ecosystem include 3D Robotics, Falcon Unmanned, and UAV Solutions. These acknowledgements do not imply direct contribution to Fly.io.",
                9.5F, FontStyle.Regular, HelpTextSecondary, new Padding(0, 0, 0, 12));
            AddSubheading(section, "ARDUPILOT DOCUMENTATION");
            AddInlineLinks(section, new[]
            {
                new LinkItem("ArduPlane documentation", "https://ardupilot.org/plane"),
                new LinkItem("ArduCopter documentation", "https://ardupilot.org/copter"),
                new LinkItem("ArduRover documentation", "https://ardupilot.org/rover")
            });
        }

        private void BuildCommunityPartnersSection()
        {
            var section = CreateSurface(new Padding(20), new Padding(0, 0, 0, 14));
            var heading = CreateLabel("OUR COMMUNITY PARTNERS", 13F, FontStyle.Bold, HelpTextPrimary);
            heading.Name = "SectionHeading";
            heading.Margin = new Padding(0, 0, 0, 13);
            AddRow(section, heading);

            communityPartnersViewport = new MarqueeViewport
            {
                BackColor = HelpPageBackground,
                Dock = DockStyle.Top,
                Height = 88,
                Margin = new Padding(0)
            };
            communityPartnersTrack = new MarqueeTrack
            {
                BackColor = HelpPageBackground,
                Height = communityPartnersViewport.Height,
                Location = Point.Empty
            };
            communityPartnersViewport.Controls.Add(communityPartnersTrack);

            var images = new Image[]
            {
                Resources.community_partner_aerobyte,
                Resources.community_partner_aerospace,
                Resources.community_partner_dev,
                Resources.community_partner_drdc,
                Resources.community_partner_electronic,
                Resources.community_partner_factoryio,
                Resources.community_partner_factoryx,
                Resources.community_partner_flyio,
                Resources.community_partner_maker,
                Resources.community_partner_racingdrone
            };
            var logoSizes = new[]
            {
                new Size(200, 80),
                new Size(62, 52),
                new Size(148, 52),
                new Size(160, 88),
                new Size(230, 72),
                new Size(350, 88),
                new Size(65, 52),
                new Size(130, 72),
                new Size(170, 64),
                new Size(115, 88)
            };

            communityPartnersSequenceWidth = 0;
            foreach (var size in logoSizes)
                communityPartnersSequenceWidth += size.Width + CommunityPartnerHorizontalMargin * 2;

            const int sequenceCopies = 3;
            for (var copy = 0; copy < sequenceCopies; copy++)
            {
                var x = copy * communityPartnersSequenceWidth;
                for (var index = 0; index < images.Length; index++)
                {
                    x += CommunityPartnerHorizontalMargin;
                    var logoSize = logoSizes[index];
                    communityPartnersTrack.Controls.Add(new PartnerLogoPictureBox
                    {
                        BackColor = Color.Transparent,
                        Bounds = new Rectangle(x, (communityPartnersViewport.Height - logoSize.Height) / 2,
                            logoSize.Width, logoSize.Height),
                        Image = images[index],
                        SizeMode = PictureBoxSizeMode.Zoom,
                        TabStop = false
                    });
                    x += logoSize.Width + CommunityPartnerHorizontalMargin;
                }
            }
            communityPartnersTrack.Width = communityPartnersSequenceWidth * sequenceCopies;
            communityPartnersOffset = 0;
            PositionCommunityPartnersTrack();
            AddRow(section, communityPartnersViewport);
            AddContent(section);

            communityPartnersTimer = new Timer { Interval = CommunityPartnersTimerInterval };
            communityPartnersTimer.Tick += CommunityPartnersTimer_Tick;
            VisibleChanged += Help_VisibleChanged;
            Disposed += Help_Disposed;
        }

        private void CommunityPartnersTimer_Tick(object sender, EventArgs e)
        {
            communityPartnersOffset += CommunityPartnersPixelsPerTick;
            if (communityPartnersOffset >= communityPartnersSequenceWidth)
                communityPartnersOffset -= communityPartnersSequenceWidth;
            PositionCommunityPartnersTrack();
        }

        private void PositionCommunityPartnersTrack()
        {
            if (communityPartnersTrack != null)
                communityPartnersTrack.Left = communityPartnersOffset - communityPartnersSequenceWidth;
        }

        private void Help_VisibleChanged(object sender, EventArgs e) { UpdateCommunityPartnersTimer(); }

        private void UpdateCommunityPartnersTimer()
        {
            if (communityPartnersTimer == null) return;
            if (Visible && !DesignMode) communityPartnersTimer.Start();
            else communityPartnersTimer.Stop();
        }

        private void Help_Disposed(object sender, EventArgs e)
        {
            if (communityPartnersTimer == null) return;
            communityPartnersTimer.Stop();
            communityPartnersTimer.Dispose();
            communityPartnersTimer = null;
        }

        private HelpCard CreateSurface(Padding padding, Padding margin)
        {
            var card = new HelpCard
            {
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = HelpCardBackground, Dock = DockStyle.Top,
                Margin = margin, Padding = padding, ColumnCount = 1
            };
            card.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            return card;
        }

        private HelpCard CreateSection(string title, string description, bool compact = false)
        {
            var section = CreateSurface(compact ? new Padding(18, 15, 18, 15) : new Padding(20), new Padding(0, 0, 0, 14));
            var heading = CreateLabel(title, compact ? 11F : 13F, FontStyle.Bold, HelpTextPrimary);
            heading.Name = "SectionHeading";
            heading.Margin = new Padding(0, 0, 0, 3);
            AddRow(section, heading);
            var intro = CreateLabel(description, 9.5F, FontStyle.Regular, HelpTextSecondary);
            intro.Name = "SectionIntro";
            intro.Margin = new Padding(0, 0, 0, compact ? 9 : 13);
            AddRow(section, intro);
            AddContent(section);
            return section;
        }

        private HelpCard CreateServiceCard(string title, string description, string buttonText, EventHandler handler)
        {
            var card = CreateSurface(new Padding(16), new Padding(0, 0, 12, 10));
            card.AutoSize = false;
            card.Dock = DockStyle.None;
            card.Height = 145;
            card.BackColor = HelpInnerCardBackground;
            card.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            card.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            card.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            card.Controls.Add(CreateLabel(title, 11.5F, FontStyle.Bold, HelpTextPrimary), 0, 0);
            var descriptionLabel = CreateLabel(description, 9.5F, FontStyle.Regular, HelpTextSecondary);
            descriptionLabel.Dock = DockStyle.Fill;
            descriptionLabel.Margin = new Padding(0, 7, 0, 8);
            card.Controls.Add(descriptionLabel, 0, 1);
            card.Controls.Add(CreateActionButton(buttonText, handler, 175), 0, 2);
            return card;
        }

        private HelpCard CreateShortcutCard(string title, string[] shortcuts)
        {
            var card = CreateSurface(new Padding(14), new Padding(0, 0, 12, 10));
            card.AutoSize = false;
            card.Dock = DockStyle.None;
            card.Height = 172;
            card.BackColor = HelpInnerCardBackground;
            AddText(card, title, 9.5F, FontStyle.Bold, HelpTextPrimary, new Padding(0, 0, 0, 7));
            var useTwoPairs = shortcuts.Length > 6;
            var grid = new TableLayoutPanel
            {
                AutoSize = true, BackColor = Color.Transparent, ColumnCount = useTwoPairs ? 4 : 2,
                Dock = DockStyle.Top, Margin = new Padding(0)
            };
            if (useTwoPairs)
            {
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 62F));
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 62F));
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            }
            else
            {
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 82F));
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            }
            var rowsPerPair = useTwoPairs ? (shortcuts.Length + 1) / 2 : shortcuts.Length;
            for (var index = 0; index < shortcuts.Length; index++)
            {
                var parts = shortcuts[index].Split('|');
                var column = useTwoPairs && index >= rowsPerPair ? 2 : 0;
                var row = useTwoPairs ? index % rowsPerPair : index;
                grid.Controls.Add(CreateLabel(parts[0], 8.5F, FontStyle.Bold, HelpTextPrimary), column, row);
                grid.Controls.Add(CreateLabel(parts[1], 8.5F, FontStyle.Regular, HelpTextSecondary), column + 1, row);
            }
            AddRow(card, grid);
            return card;
        }

        private FlowLayoutPanel CreateWrapPanel()
        {
            return new FlowLayoutPanel
            {
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, BackColor = Color.Transparent,
                Dock = DockStyle.Top, FlowDirection = FlowDirection.LeftToRight, Margin = new Padding(0), WrapContents = true
            };
        }

        private void LayoutCards(FlowLayoutPanel host, Control[] cards, int maximumColumns, int minimumWidth)
        {
            var available = host.ClientSize.Width;
            if (available <= 0) return;
            const int gap = 12;
            var columns = Math.Min(maximumColumns, Math.Max(1, (available + gap) / (minimumWidth + gap)));
            var width = Math.Max(240, (available - gap * (columns - 1)) / columns);
            for (var index = 0; index < cards.Length; index++)
            {
                cards[index].Width = width;
                cards[index].Margin = new Padding(0, 0, (index % columns == columns - 1) ? 0 : gap, 10);
            }
            var maximumHeight = 0;
            foreach (var card in cards)
                maximumHeight = Math.Max(maximumHeight, card.Height + card.Margin.Vertical);
            host.Height = ((cards.Length + columns - 1) / columns) * maximumHeight;
        }

        private void AddFact(TableLayoutPanel facts, string key, string value)
        {
            var row = facts.RowCount++;
            facts.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            facts.Controls.Add(CreateLabel(key, 9.5F, FontStyle.Bold, HelpTextPrimary), 0, row);
            facts.Controls.Add(CreateLabel(value, 9.5F, FontStyle.Regular, HelpTextSecondary), 1, row);
        }

        private void AddSubheading(TableLayoutPanel section, string text)
        {
            AddText(section, text, 9F, FontStyle.Bold, HelpTextPrimary, new Padding(0, 5, 0, 7));
        }

        private void AddLinkRow(TableLayoutPanel card, string key, string url)
        {
            var row = CreateWrapPanel();
            row.Controls.Add(CreateLabel(key + ":", 9.5F, FontStyle.Bold, HelpTextPrimary));
            row.Controls.Add(CreateLink(url, url));
            AddRow(card, row);
        }

        private void AddInlineLinks(TableLayoutPanel card, LinkItem[] links)
        {
            var row = CreateWrapPanel();
            foreach (var item in links) row.Controls.Add(CreateLink(item.Text, item.Url));
            AddRow(card, row);
        }

        private void AddText(TableLayoutPanel card, string text, float size, FontStyle style, Color color, Padding? margin)
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
                AutoSize = true, BackColor = Color.Transparent, Font = new Font("Segoe UI", size, style),
                ForeColor = color, Margin = new Padding(0, 2, 10, 2), Text = text
            };
        }

        private LinkLabel CreateLink(string text, string url)
        {
            var link = new LinkLabel
            {
                AutoSize = true, BackColor = Color.Transparent, Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                LinkColor = HelpTextPrimary, ActiveLinkColor = Color.Black,
                VisitedLinkColor = HelpTextPrimary, LinkBehavior = LinkBehavior.AlwaysUnderline,
                Margin = new Padding(0, 2, 22, 5), Text = text, Tag = url
            };
            link.LinkClicked += ExternalLink_LinkClicked;
            link.MouseEnter += HelpLink_MouseEnter;
            link.MouseLeave += HelpLink_MouseLeave;
            return link;
        }

        private void HelpLink_MouseEnter(object sender, EventArgs e)
        {
            var link = sender as LinkLabel;
            if (link != null) link.LinkColor = HelpTextSecondary;
        }

        private void HelpLink_MouseLeave(object sender, EventArgs e)
        {
            var link = sender as LinkLabel;
            if (link != null) link.LinkColor = HelpTextPrimary;
        }

        private MyButton CreateActionButton(string text, EventHandler handler, int width)
        {
            var button = new MyButton { Text = text, Size = new Size(width, 34), Margin = new Padding(0, 0, 12, 0) };
            button.Click += handler;
            StyleActionButton(button);
            return button;
        }

        private void StyleActionButton(MyButton button)
        {
            button.BackColor = HelpPageBackground;
            button.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            button.BGGradTop = HelpButton;
            button.BGGradBot = HelpButton;
            button.ColorMouseOver = HelpButtonHover;
            button.ColorMouseDown = HelpButtonPressed;
            button.ColorNotEnabled = HelpBorder;
            button.Outline = HelpButton;
            button.TextColor = Color.White;
            button.TextColorNotEnabled = HelpTextMuted;
        }

        private void AddContent(Control control)
        {
            contentLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            contentLayout.Controls.Add(control, 0, contentLayout.RowCount++);
        }

        private void AddRow(TableLayoutPanel panel, Control control)
        {
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.Controls.Add(control, 0, panel.RowCount++);
        }

        private void ApplyHelpTheme()
        {
            BackColor = HelpPageBackground;
            helpScrollPanel.BackColor = HelpPageBackground;
            contentLayout.BackColor = HelpPageBackground;
            if (communityPartnersViewport != null)
                communityPartnersViewport.BackColor = HelpPageBackground;
            if (communityPartnersTrack != null)
                communityPartnersTrack.BackColor = HelpPageBackground;
            CHK_showconsole.BackColor = Color.Transparent;
            CHK_showconsole.ForeColor = HelpTextSecondary;
            linkLabel1.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            linkLabel1.LinkColor = HelpTextPrimary;
            linkLabel1.ActiveLinkColor = Color.Black;
            linkLabel1.VisitedLinkColor = HelpTextPrimary;
            linkLabel1.LinkBehavior = LinkBehavior.AlwaysUnderline;
            linkLabel1.MouseEnter -= HelpLink_MouseEnter;
            linkLabel1.MouseLeave -= HelpLink_MouseLeave;
            linkLabel1.MouseEnter += HelpLink_MouseEnter;
            linkLabel1.MouseLeave += HelpLink_MouseLeave;
            StyleActionButton(BUT_updatecheck);
            StyleActionButton(BUT_betaupdate);
        }

        private void OpenDashboard_Click(object sender, EventArgs e) { OpenUrl("https://drdt-dev-gcs-frontend.drsys.asia/en/login"); }
        private void OpenPartnerships_Click(object sender, EventArgs e) { OpenUrl("https://aerobyte-dev-customer.drsys.asia/"); }

        private void ExternalLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var link = sender as LinkLabel;
            if (link != null && link.Tag is string) OpenUrl((string)link.Tag);
        }

        private void OpenUrl(string url)
        {
            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                CustomMessageBox.Show("Only HTTP and HTTPS web links can be opened.", Strings.ERROR);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = uri.AbsoluteUri,
                    UseShellExecute = true
                });
            }
            catch (Exception ex) { CustomMessageBox.Show("Failed to open URL:\n" + url + "\n\n" + ex.Message, Strings.ERROR); }
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
                using (var pen = new Pen(HelpBorder))
                    e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }

        [PreventTheming]
        private sealed class MarqueeViewport : Panel
        {
            public MarqueeViewport()
            {
                DoubleBuffered = true;
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            }
        }

        [PreventTheming]
        private sealed class MarqueeTrack : Panel
        {
            public MarqueeTrack()
            {
                DoubleBuffered = true;
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.UserPaint, true);
            }
        }

        [PreventTheming]
        private sealed class PartnerLogoPictureBox : PictureBox
        {
        }
    }
}
