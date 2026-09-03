using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using MissionPlanner.Controls;
using MissionPlanner.Utilities;

namespace MissionPlanner.GCSViews
{
    public partial class FlightData
    {
        private const float FlyioDefaultLeftPaneRatio = 0.44F;
        private bool flyioResponsiveLayoutEnabled;
        private bool flyioApplyingLayout;
        private float flyioMainSplitRatio = FlyioDefaultLeftPaneRatio;
        private TableLayoutPanel flyioDataStatusBar;
        private Label flyioMapLatitude;
        private Label flyioMapLongitude;
        private Label flyioMapAltitude;
        private Label flyioMapHeading;
        private Label flyioMapSatellites;
        private Label flyioModeStatus;
        private Label flyioArmStatus;
        private Label flyioGpsStatus;
        private Label flyioSatelliteStatus;
        private Label flyioHdopStatus;
        private Label flyioVibeStatus;
        private Label flyioLinkStatus;
        private Label flyioRcStatus;

        private void InitializeFlyioDataLayout()
        {
            flyioResponsiveLayoutEnabled = true;

            MainH.FixedPanel = FixedPanel.None;
            MainH.BorderStyle = BorderStyle.None;
            MainH.SplitterWidth = 1;
            MainH.Panel1MinSize = 360;
            MainH.Panel2MinSize = 420;
            MainH.Panel2.Padding = Padding.Empty;

            // FlightData's nested splitter reserves Panel1 for the optional tuning graph.
            // The Fly.io DATA presentation uses the full map row and exposes no tuning
            // graph there, so the legacy panel must never reserve map width or height.
            splitContainer1.Panel1Collapsed = true;
            splitContainer1.IsSplitterFixed = true;
            splitContainer1.Panel1MinSize = 0;
            splitContainer1.SplitterWidth = 1;
            splitContainer1.Margin = Padding.Empty;
            splitContainer1.Padding = Padding.Empty;
            splitContainer1.Panel2.Padding = Padding.Empty;
            tableMap.Margin = Padding.Empty;
            tableMap.Padding = Padding.Empty;

            SubMainLeft.FixedPanel = FixedPanel.None;
            SubMainLeft.BorderStyle = BorderStyle.None;
            SubMainLeft.SplitterWidth = 1;
            SubMainLeft.Panel1MinSize = 180;
            SubMainLeft.Panel2MinSize = 180;

            hud1.Margin = Padding.Empty;
            hud1.Dock = DockStyle.Fill;
            hud1.Paint += DrawFlyioHudFrame;
            tabControlactions.Margin = Padding.Empty;
            tabControlactions.Dock = DockStyle.Fill;
            tabControlactions.Multiline = false;
            tabControlactions.SizeMode = TabSizeMode.Normal;
            tabControlactions.ItemSize = new Size(0, 40);
            tabControlactions.Padding = new Point(14, 7);

            StyleFlyioQuickTelemetry();

            CreateFlyioMapInformationRow();
            CreateFlyioDataStatusBar();

            Load += FlyioDataLoaded;
            MainH.SizeChanged += FlyioDataLayoutChanged;
            MainH.SplitterMoved += FlyioMainSplitterMoved;
            SubMainLeft.SizeChanged += FlyioLeftPanelSizeChanged;
            ApplyFlyioResponsiveGeometry();
        }

        private void DrawFlyioHudFrame(object sender, PaintEventArgs e)
        {
            using (var border = new Pen(FlyioDataBorder))
                e.Graphics.DrawRectangle(border, 0, 0, Math.Max(0, hud1.Width - 1), Math.Max(0, hud1.Height - 1));
        }

        private void StyleFlyioQuickTelemetry()
        {
            tableLayoutPanelQuick.BackColor = FlyioDataBackground;
            tableLayoutPanelQuick.Padding = new Padding(3);
            tableLayoutPanelQuick.Margin = Padding.Empty;

            QuickView[] cards = { quickView1, quickView2, quickView3, quickView4, quickView5, quickView6 };
            foreach (QuickView card in cards)
            {
                card.Dock = DockStyle.Fill;
                card.Margin = new Padding(3);
                card.BackColor = FlyioDataSurface;
                card.ForeColor = FlyioDataSecondaryText;
                card.Font = CreateFlyioFont(9F, FontStyle.Bold);
                card.numberColor = FlyioDataText;
                card.numberColorBackup = FlyioDataText;
                card.ModernCardStyle = true;
                card.MaximumValueFontSize = 36F;
                card.CardBorderColor = FlyioDataBorder;
            }
        }

        private void FlyioDataLoaded(object sender, EventArgs e)
        {
            int availableWidth = MainH.Width - MainH.SplitterWidth;
            if (availableWidth > 0)
            {
                float restoredRatio = MainH.SplitterDistance / (float)availableWidth;
                flyioMainSplitRatio = restoredRatio >= 0.30F && restoredRatio <= 0.70F
                    ? restoredRatio
                    : FlyioDefaultLeftPaneRatio;
            }

            ApplyFlyioResponsiveGeometry();
        }

        private void FlyioDataLayoutChanged(object sender, EventArgs e)
        {
            ApplyFlyioResponsiveGeometry();
        }

        private void FlyioMainSplitterMoved(object sender, SplitterEventArgs e)
        {
            if (!flyioResponsiveLayoutEnabled || flyioApplyingLayout)
                return;

            int availableWidth = MainH.Width - MainH.SplitterWidth;
            if (availableWidth > 0)
                flyioMainSplitRatio = MainH.SplitterDistance / (float)availableWidth;

            Settings.Instance["FlightSplitter"] = MainH.SplitterDistance.ToString(CultureInfo.InvariantCulture);
            ApplyFlyioLeftVerticalGeometry();
            gMapControl1.Invalidate();
        }

        private void FlyioLeftPanelSizeChanged(object sender, EventArgs e)
        {
            if (!flyioResponsiveLayoutEnabled || flyioApplyingLayout)
                return;

            flyioApplyingLayout = true;
            try
            {
                ApplyFlyioLeftVerticalGeometry();
            }
            finally
            {
                flyioApplyingLayout = false;
            }
        }

        private void ApplyFlyioResponsiveGeometry()
        {
            if (!flyioResponsiveLayoutEnabled || flyioApplyingLayout ||
                MainH.Width <= 0 || SubMainLeft.Height <= 0)
                return;

            flyioApplyingLayout = true;
            try
            {
                if (!splitContainer1.Panel1Collapsed)
                    splitContainer1.Panel1Collapsed = true;

                int availableWidth = MainH.Width - MainH.SplitterWidth;
                int left = (int)Math.Round(availableWidth * flyioMainSplitRatio);
                int preferredLeftMinimum = availableWidth >= 940 ? 460 : MainH.Panel1MinSize;
                int maximumLeft = Math.Max(preferredLeftMinimum, availableWidth - MainH.Panel2MinSize);
                left = Math.Max(preferredLeftMinimum, Math.Min(left, maximumLeft));
                if (left > 0 && left < availableWidth && MainH.SplitterDistance != left)
                    MainH.SplitterDistance = left;

                ApplyFlyioLeftVerticalGeometry();
            }
            finally
            {
                flyioApplyingLayout = false;
            }
        }

        private void ApplyFlyioLeftVerticalGeometry()
        {
            if (SubMainLeft.Height <= 0 || SubMainLeft.Width <= 0)
                return;

            int availableHeight = SubMainLeft.Height - SubMainLeft.SplitterWidth;
            int preferredHudMinimum = availableHeight >= 510 ? 230 : SubMainLeft.Panel1MinSize;
            int preferredTabsMinimum = availableHeight >= 510 ? 280 : SubMainLeft.Panel2MinSize;
            int widescreenHeight = Math.Max(preferredHudMinimum,
                (int)Math.Round(SubMainLeft.Width * 0.44F));
            int maximumHud = Math.Max(preferredHudMinimum, availableHeight - preferredTabsMinimum);
            int hudHeight = Math.Min(widescreenHeight, maximumHud);
            if (hudHeight > 0 && hudHeight < availableHeight &&
                SubMainLeft.SplitterDistance != hudHeight)
                SubMainLeft.SplitterDistance = hudHeight;
        }

        private void CreateFlyioMapInformationRow()
        {
            panel1.Height = 38;
            if (tableMap.RowStyles.Count > 1)
            {
                tableMap.RowStyles[1].SizeType = SizeType.Absolute;
                tableMap.RowStyles[1].Height = 38;
            }
            tableMap.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
            panel1.Padding = new Padding(10, 4, 10, 4);
            panel1.BackColor = FlyioDataSurface;

            coords1.Visible = false;
            var mapInfo = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = FlyioDataSurface,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                ColumnCount = 5,
                RowCount = 1
            };
            foreach (float width in new[] { 26F, 26F, 16F, 17F, 15F })
                mapInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, width));
            mapInfo.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            flyioMapLatitude = NewFlyioMapInfoLabel(ContentAlignment.MiddleLeft);
            flyioMapLongitude = NewFlyioMapInfoLabel(ContentAlignment.MiddleLeft);
            flyioMapAltitude = NewFlyioMapInfoLabel(ContentAlignment.MiddleLeft);
            flyioMapHeading = NewFlyioMapInfoLabel(ContentAlignment.MiddleRight);
            flyioMapSatellites = NewFlyioMapInfoLabel(ContentAlignment.MiddleRight);
            mapInfo.Controls.Add(flyioMapLatitude, 0, 0);
            mapInfo.Controls.Add(flyioMapLongitude, 1, 0);
            mapInfo.Controls.Add(flyioMapAltitude, 2, 0);
            mapInfo.Controls.Add(flyioMapHeading, 3, 0);
            mapInfo.Controls.Add(flyioMapSatellites, 4, 0);
            panel1.Controls.Add(mapInfo);
            mapInfo.BringToFront();

            CHK_autopan.Visible = false;
            CB_tuning.Visible = false;
        }

        private Label NewFlyioMapInfoLabel(ContentAlignment alignment)
        {
            return new Label
            {
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                TextAlign = alignment,
                BackColor = FlyioDataSurface,
                ForeColor = FlyioDataSecondaryText,
                Font = CreateFlyioFont(9F, FontStyle.Regular),
                AutoEllipsis = true
            };
        }

        private void CreateFlyioDataStatusBar()
        {
            flyioDataStatusBar = new TableLayoutPanel
            {
                Name = "flyioDataStatusBar",
                Dock = DockStyle.Bottom,
                Height = 48,
                BackColor = FlyioDataBackground,
                Padding = new Padding(8, 5, 8, 5),
                Margin = Padding.Empty,
                ColumnCount = 8,
                RowCount = 1,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            foreach (float width in new[] { 17F, 11F, 15F, 10F, 12F, 12F, 12F, 11F })
                flyioDataStatusBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, width));
            flyioDataStatusBar.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            flyioDataStatusBar.Paint += DrawFlyioStatusSeparators;

            flyioModeStatus = NewFlyioStatusLabel();
            flyioArmStatus = NewFlyioStatusLabel();
            flyioGpsStatus = NewFlyioStatusLabel();
            flyioSatelliteStatus = NewFlyioStatusLabel();
            flyioHdopStatus = NewFlyioStatusLabel();
            flyioVibeStatus = NewFlyioStatusLabel();
            flyioLinkStatus = NewFlyioStatusLabel();
            flyioRcStatus = NewFlyioStatusLabel();

            Control[] statuses = { flyioModeStatus, flyioArmStatus, flyioGpsStatus, flyioSatelliteStatus,
                flyioHdopStatus, flyioVibeStatus, flyioLinkStatus, flyioRcStatus };
            for (int index = 0; index < statuses.Length; index++)
                flyioDataStatusBar.Controls.Add(statuses[index], index, 0);
            Controls.Add(flyioDataStatusBar);
            flyioDataStatusBar.BringToFront();
        }

        private void DrawFlyioStatusSeparators(object sender, PaintEventArgs e)
        {
            using (var pen = new Pen(FlyioDataBorder))
            {
                e.Graphics.DrawLine(pen, 0, 0, flyioDataStatusBar.Width, 0);
                int[] widths = flyioDataStatusBar.GetColumnWidths();
                for (int column = 1; column < widths.Length; column++)
                {
                    int x = widths.Take(column).Sum();
                    e.Graphics.DrawLine(pen, x, 8, x, flyioDataStatusBar.Height - 8);
                }
            }
        }

        private Label NewFlyioStatusLabel()
        {
            return new Label
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(8, 0, 6, 0),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = FlyioDataBackground,
                ForeColor = FlyioDataText,
                Font = CreateFlyioFont(9.25F, FontStyle.Bold),
                AutoEllipsis = true
            };
        }

        private void UpdateFlyioDataStatus(CurrentState state)
        {
            if (flyioDataStatusBar == null || flyioDataStatusBar.IsDisposed)
                return;

            flyioMapLatitude.Text = "LAT  " + state.lat.ToString("0.0000000", CultureInfo.InvariantCulture);
            flyioMapLongitude.Text = "LON  " + state.lng.ToString("0.0000000", CultureInfo.InvariantCulture);
            flyioMapAltitude.Text = "ALT  " + state.alt.ToString("0", CultureInfo.InvariantCulture) + " " + CurrentState.AltUnit;
            flyioMapHeading.Text = "HDG: " + NormalizeHeading(state.yaw).ToString("000", CultureInfo.InvariantCulture) + "°";
            flyioMapSatellites.Text = "SATS  " + state.satcount.ToString("0", CultureInfo.InvariantCulture);

            flyioModeStatus.Text = "MODE\r\n" + (string.IsNullOrWhiteSpace(state.mode) ? "UNKNOWN" : state.mode.ToUpperInvariant());
            flyioModeStatus.ForeColor = state.connected ? Color.FromArgb(32, 151, 94) : FlyioDataSecondaryText;
            flyioArmStatus.Text = "STATE\r\n" + (state.armed ? "ARMED" : "DISARMED");
            flyioArmStatus.ForeColor = state.armed ? Color.FromArgb(32, 191, 107) : Color.FromArgb(235, 87, 87);
            flyioGpsStatus.Text = "GPS\r\n" + FormatGpsFix(state.gpsstatus);
            flyioGpsStatus.ForeColor = state.gpsstatus >= 3 ? Color.FromArgb(32, 151, 94) :
                state.gpsstatus >= 2 ? Color.FromArgb(245, 158, 11) : Color.FromArgb(211, 47, 47);
            flyioSatelliteStatus.Text = "SAT\r\n" + state.satcount.ToString("0", CultureInfo.InvariantCulture);
            flyioHdopStatus.Text = "HDOP\r\n" + state.gpshdop.ToString("0.0", CultureInfo.InvariantCulture);
            float vibration = Math.Max(state.vibex, Math.Max(state.vibey, state.vibez));
            flyioVibeStatus.Text = "VIBE\r\n" + vibration.ToString("0.0", CultureInfo.InvariantCulture);
            flyioVibeStatus.ForeColor = vibration > 60 ? Color.FromArgb(211, 47, 47) :
                vibration > 30 ? Color.FromArgb(245, 158, 11) : FlyioDataText;
            flyioLinkStatus.Text = "LINK\r\n" + state.linkqualitygcs.ToString(CultureInfo.InvariantCulture) + "%";
            flyioRcStatus.Text = "RC\r\n" + Math.Max(0, Math.Min(100, state.rxrssi)).ToString(CultureInfo.InvariantCulture) + "%";
        }

        private static int NormalizeHeading(float heading)
        {
            int normalized = (int)Math.Round(heading) % 360;
            return normalized < 0 ? normalized + 360 : normalized;
        }
    }
}
