using MissionPlanner.Maps;
using MissionPlanner.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

namespace MissionPlanner.GCSViews
{
    public partial class FlightData
    {
        private static readonly Color FlyioBackground = Color.FromArgb(11, 31, 58);
        private static readonly Color FlyioSurface = Color.FromArgb(19, 43, 79);
        private static readonly Color FlyioSecondarySurface = Color.FromArgb(16, 38, 66);
        private static readonly Color FlyioAccent = Color.FromArgb(0, 173, 238);
        private static readonly Color FlyioHover = Color.FromArgb(37, 190, 244);
        private static readonly Color FlyioPressed = Color.FromArgb(0, 131, 184);
        private static readonly Color FlyioText = Color.FromArgb(242, 245, 248);
        private static readonly Color FlyioSecondaryText = Color.FromArgb(170, 183, 199);
        private static readonly Color FlyioDivider = Color.FromArgb(43, 72, 105);
        private static readonly Color FlyioHealthy = Color.FromArgb(71, 196, 116);
        private static readonly Color FlyioDanger = Color.FromArgb(235, 87, 87);
        private static readonly Color FlyioUnavailable = Color.FromArgb(128, 139, 153);

        private readonly Dictionary<string, Label> flyioStatusValues = new Dictionary<string, Label>();
        private Panel flyioWorkspaceHost;
        private Control flyioMapWorkspace;
        private SplitContainer flyioWorkspaceSplit;
        private Button flyioMapButton;
        private Button flyioHudButton;

        private Font CreateFlyioFont(float size, FontStyle style = FontStyle.Regular)
        {
            const string preferred = "Kantumruy Pro";
            const string fallback = "Segoe UI";
            try
            {
                using (var fonts = new InstalledFontCollection())
                {
                    foreach (var family in fonts.Families)
                    {
                        if (string.Equals(family.Name, preferred, StringComparison.OrdinalIgnoreCase))
                            return new Font(family, size, style, GraphicsUnit.Point);
                    }
                }
            }
            catch
            {
                // A missing or inaccessible optional UI font must never prevent startup.
            }
            return new Font(fallback, size, style, GraphicsUnit.Point);
        }

        private void InitializeFlyioOperationsLayout()
        {
            SuspendLayout();
            MainH.SuspendLayout();

            flyioMapWorkspace = tableMap;
            tableMap.Parent = null;
            hud1.Parent = null;
            tabControlactions.Parent = null;
            panel_persistent.Parent = null;
            SubMainLeft.Parent = null;

            MainH.Panel1.Controls.Clear();
            MainH.Panel2.Controls.Clear();
            MainH.BorderStyle = BorderStyle.None;
            MainH.FixedPanel = FixedPanel.Panel1;
            MainH.IsSplitterFixed = false;
            MainH.SplitterWidth = 5;
            int outerMaximum = MainH.ClientSize.Width - MainH.SplitterWidth - MainH.Panel2MinSize;
            if (outerMaximum >= 205)
            {
                MainH.SplitterDistance = Math.Min(220, outerMaximum);
                MainH.Panel1MinSize = 205;
            }
            MainH.BackColor = FlyioBackground;

            var aircraftPanel = BuildAircraftStatusPanel();
            MainH.Panel1.Controls.Add(aircraftPanel);

            var operations = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = FlyioBackground,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            var statusStrip = BuildBottomStatusStrip();
            statusStrip.Dock = DockStyle.Bottom;
            statusStrip.Height = 42;
            flyioWorkspaceSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                FixedPanel = FixedPanel.Panel2,
                IsSplitterFixed = false,
                SplitterWidth = 5,
                BackColor = FlyioBackground
            };
            flyioWorkspaceSplit.Panel1.Controls.Add(BuildCenterWorkspace());
            flyioWorkspaceSplit.Panel2.Controls.Add(BuildFlightControlsPanel());
            flyioWorkspaceSplit.SizeChanged += (sender, args) => UpdateFlyioWorkspaceSplitter();
            operations.Controls.Add(flyioWorkspaceSplit);
            operations.Controls.Add(statusStrip);
            statusStrip.BringToFront();
            MainH.Panel2.Controls.Add(operations);

            ShowFlyioWorkspace(flyioMapWorkspace);
            MainH.ResumeLayout(true);
            ResumeLayout(true);
            UpdateFlyioWorkspaceSplitter();
        }

        private void UpdateFlyioWorkspaceSplitter()
        {
            const int centerMinimum = 400;
            const int rightMinimum = 260;
            const int rightPreferred = 285;

            if (flyioWorkspaceSplit == null || flyioWorkspaceSplit.IsDisposed)
                return;

            int availableWidth = flyioWorkspaceSplit.ClientSize.Width - flyioWorkspaceSplit.SplitterWidth;
            if (availableWidth < centerMinimum + rightMinimum)
                return;

            int rightWidth = Math.Min(rightPreferred, availableWidth - centerMinimum);
            rightWidth = Math.Max(rightMinimum, rightWidth);
            int desiredDistance = availableWidth - rightWidth;

            // Establish a valid distance while both minimums are still zero, then
            // apply the minimums. This avoids WinForms validating them against the
            // SplitContainer's small construction-time size.
            if (flyioWorkspaceSplit.Panel1MinSize != centerMinimum ||
                flyioWorkspaceSplit.Panel2MinSize != rightMinimum)
            {
                flyioWorkspaceSplit.SplitterDistance = desiredDistance;
                flyioWorkspaceSplit.Panel1MinSize = centerMinimum;
                flyioWorkspaceSplit.Panel2MinSize = rightMinimum;
            }

            int minimumDistance = flyioWorkspaceSplit.Panel1MinSize;
            int maximumDistance = flyioWorkspaceSplit.ClientSize.Width -
                                  flyioWorkspaceSplit.SplitterWidth -
                                  flyioWorkspaceSplit.Panel2MinSize;
            if (maximumDistance < minimumDistance)
                return;

            desiredDistance = Math.Max(minimumDistance, Math.Min(desiredDistance, maximumDistance));
            if (flyioWorkspaceSplit.SplitterDistance != desiredDistance)
                flyioWorkspaceSplit.SplitterDistance = desiredDistance;
        }

        private Control BuildAircraftStatusPanel()
        {
            var panel = NewSurfacePanel();
            panel.Padding = new Padding(12, 12, 12, 10);
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, AutoScroll = false, ColumnCount = 2, RowCount = 0, GrowStyle = TableLayoutPanelGrowStyle.AddRows, BackColor = FlyioSurface, Margin = Padding.Empty, Padding = Padding.Empty };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38F));
            AddFullWidthControl(layout, NewHeading("AIRCRAFT", 12.5F));
            var connection = NewStatusValue("connection", "● DISCONNECTED", 9.2F, FlyioDanger);
            connection.Margin = new Padding(0, 0, 0, 8);
            AddFullWidthControl(layout, connection);
            AddSectionHeader(layout, "STATUS");
            AddMetricRow(layout, "VEHICLE", "vehicle");
            AddMetricRow(layout, "MODE", "mode");
            AddMetricRow(layout, "GPS", "gps");
            AddMetricRow(layout, "BATTERY", "battery");
            AddMetricRow(layout, "LINK", "link");
            AddSectionHeader(layout, "FLIGHT");
            AddMetricRow(layout, "ALTITUDE", "altitude");
            AddMetricRow(layout, "GROUND SPEED", "groundspeed");
            AddMetricRow(layout, "VERTICAL SPEED", "verticalspeed");
            AddSectionHeader(layout, "POSITION");
            AddMetricRow(layout, "LATITUDE", "latitude");
            AddMetricRow(layout, "LONGITUDE", "longitude");
            int fillerRow = layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            var filler = new Panel { Dock = DockStyle.Fill, BackColor = FlyioSurface };
            layout.Controls.Add(filler, 0, fillerRow);
            layout.SetColumnSpan(filler, 2);
            panel.Controls.Add(layout);
            return panel;
        }

        private Control BuildCenterWorkspace()
        {
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = FlyioBackground, Padding = new Padding(0, 0, 5, 0), ColumnCount = 1, RowCount = 2 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = FlyioSecondarySurface, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(8, 5, 8, 5), WrapContents = false };
            flyioMapButton = NewSelectorButton("MAP", true);
            flyioMapButton.Width = 94;
            flyioHudButton = NewSelectorButton("HUD", false);
            flyioHudButton.Width = 94;
            var video = NewSelectorButton("VIDEO", false);
            video.Width = 156;
            video.Enabled = false;
            video.Text = "VIDEO  PLANNED";
            video.BackColor = FlyioBackground;
            video.ForeColor = FlyioUnavailable;
            video.FlatAppearance.BorderColor = FlyioDivider;
            flyioMapButton.Click += (sender, args) => ShowFlyioWorkspace(flyioMapWorkspace);
            flyioHudButton.Click += (sender, args) => ShowFlyioWorkspace(hud1);
            toolbar.Controls.Add(flyioMapButton);
            toolbar.Controls.Add(flyioHudButton);
            toolbar.Controls.Add(video);
            toolbar.Controls.Add(BuildMapLayersButton());
            flyioWorkspaceHost = new Panel { Dock = DockStyle.Fill, BackColor = Color.Black, Margin = Padding.Empty };
            layout.Controls.Add(toolbar, 0, 0);
            layout.Controls.Add(flyioWorkspaceHost, 0, 1);
            return layout;
        }

        private Control BuildFlightControlsPanel()
        {
            var panel = NewSurfacePanel();
            panel.Padding = new Padding(10, 12, 10, 8);
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = FlyioSurface, ColumnCount = 1, RowCount = 4, Margin = Padding.Empty };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 29F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.Controls.Add(NewHeading("FLIGHT CONTROLS", 12F), 0, 0);
            var flightMode = NewStatusValue("flightmode", "MODE  --", 9.5F, FlyioText);
            flightMode.BackColor = FlyioSecondarySurface;
            flightMode.Dock = DockStyle.Fill;
            flightMode.Padding = new Padding(8, 7, 8, 5);
            flightMode.Margin = new Padding(0, 0, 0, 6);
            layout.Controls.Add(flightMode, 0, 1);
            tabControlactions.Dock = DockStyle.Fill;
            tabControlactions.Margin = Padding.Empty;
            tabControlactions.BackColor = FlyioBackground;
            tabControlactions.ForeColor = FlyioText;
            tabControlactions.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControlactions.Font = CreateFlyioFont(8.2F, FontStyle.Bold);
            tabControlactions.ItemSize = new Size(tabControlactions.ItemSize.Width, 30);
            foreach (TabPage page in tabControlactions.TabPages)
            {
                page.UseVisualStyleBackColor = false;
                page.BackColor = FlyioBackground;
                page.ForeColor = FlyioText;
                page.BorderStyle = BorderStyle.None;
            }
            layout.Controls.Add(tabControlactions, 0, 2);
            panel_persistent.Dock = DockStyle.Top;
            layout.Controls.Add(panel_persistent, 0, 3);
            panel.Controls.Add(layout);
            return panel;
        }

        private Control BuildBottomStatusStrip()
        {
            var strip = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = FlyioSecondarySurface, Padding = new Padding(12, 8, 6, 5), WrapContents = false, AutoScroll = true, FlowDirection = FlowDirection.LeftToRight };
            string[] items = { "bottomgps", "bottomlink", "bottombattery", "bottommode", "bottomposition", "bottomsystem" };
            for (int index = 0; index < items.Length; index++)
            {
                if (index > 0)
                {
                    strip.Controls.Add(new Label { AutoSize = false, Width = 1, Height = 18, BackColor = FlyioDivider, Margin = new Padding(0, 2, 14, 0) });
                }
                var label = NewStatusValue(items[index], "--", 8.6F, FlyioSecondaryText);
                label.AutoSize = true;
                label.Margin = new Padding(0, 1, 14, 0);
                strip.Controls.Add(label);
            }
            return strip;
        }

        private Button BuildMapLayersButton()
        {
            var button = NewSelectorButton("MAP LAYERS", false);
            button.Width = 132;
            button.Margin = new Padding(12, 0, 0, 0);
            var menu = new ContextMenuStrip { BackColor = FlyioSurface, ForeColor = FlyioText, Font = CreateFlyioFont(8.8F), Padding = new Padding(2, 3, 2, 3) };
            AddLayerSection(menu, "BASE MAP");
            AddLayerItem(menu, "Standard  ·  CURRENT PROVIDER", true, false);
            AddLayerItem(menu, "Satellite  ·  USE EXISTING MAP PROVIDER MENU", false, false);
            AddLayerItem(menu, "Custom Map  ·  PLANNED", false, false);
            AddLayerSection(menu, "FLIGHT");
            AddLayerItem(menu, "My UAV  ·  CURRENT VIEW", true, false);
            AddLayerItem(menu, "Mission Route  ·  CURRENT VIEW", true, false);
            AddLayerItem(menu, "Waypoints  ·  CURRENT VIEW", true, false);
            AddLayerItem(menu, "Flight Trail  ·  CURRENT VIEW", true, false);
            AddLayerSection(menu, "AIRSPACE");
            AddLayerItem(menu, "Live Aircraft  ·  PLANNED", false, false);
            AddLayerItem(menu, "Aircraft Trails  ·  PLANNED", false, false);
            AddLayerItem(menu, "Airports  ·  PLANNED", false, false);
            AddLayerItem(menu, "Airspace Boundaries  ·  PLANNED", false, false);
            AddLayerSection(menu, "SAFETY");
            AddLayerItem(menu, "No-Fly Zones  ·  PLANNED", false, false);
            AddLayerItem(menu, "Warning Zones  ·  PLANNED", false, false);
            AddLayerItem(menu, "Authorized Areas  ·  PLANNED", false, false);
            AddLayerSection(menu, "OPERATIONS / ENVIRONMENT / IMPORT");
            AddLayerItem(menu, "DRDC Stations  ·  PLANNED", false, false);
            AddLayerItem(menu, "Landing Sites  ·  PLANNED", false, false);
            AddLayerItem(menu, "Weather / Wind / Precipitation  ·  PLANNED", false, false);
            AddLayerItem(menu, "Import KML / KMZ / GeoJSON  ·  PLANNED", false, false);
            button.Click += (sender, args) => menu.Show(button, new Point(0, button.Height));
            return button;
        }

        private static void AddLayerSection(ContextMenuStrip menu, string text)
        {
            menu.Items.Add(new ToolStripLabel(text) { ForeColor = FlyioAccent, Font = new Font(menu.Font, FontStyle.Bold), Margin = new Padding(5, 4, 5, 1), Padding = new Padding(2, 1, 2, 1) });
        }

        private static void AddLayerItem(ContextMenuStrip menu, string text, bool isChecked, bool enabled)
        {
            menu.Items.Add(new ToolStripMenuItem(text) { Checked = isChecked, Enabled = enabled, CheckOnClick = enabled, Padding = new Padding(2, 1, 2, 1) });
        }

        private void ShowFlyioWorkspace(Control workspace)
        {
            if (flyioWorkspaceHost == null || workspace == null)
                return;
            flyioWorkspaceHost.SuspendLayout();
            flyioWorkspaceHost.Controls.Clear();
            workspace.Dock = DockStyle.Fill;
            workspace.Visible = true;
            flyioWorkspaceHost.Controls.Add(workspace);
            bool mapSelected = workspace == flyioMapWorkspace;
            bool hudSelected = workspace == hud1;
            flyioMapButton.BackColor = mapSelected ? FlyioAccent : FlyioSecondarySurface;
            flyioMapButton.FlatAppearance.BorderColor = mapSelected ? FlyioAccent : FlyioDivider;
            flyioHudButton.BackColor = hudSelected ? FlyioAccent : FlyioSecondarySurface;
            flyioHudButton.FlatAppearance.BorderColor = hudSelected ? FlyioAccent : FlyioDivider;
            flyioWorkspaceHost.ResumeLayout(true);
        }

        private void AddFullWidthControl(TableLayoutPanel layout, Control control)
        {
            int row = layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.Controls.Add(control, 0, row);
            layout.SetColumnSpan(control, 2);
        }

        private void AddSectionHeader(TableLayoutPanel layout, string text)
        {
            var heading = new Label { AutoSize = false, Dock = DockStyle.Fill, Height = 22, Text = text, TextAlign = ContentAlignment.MiddleLeft, ForeColor = FlyioAccent, BackColor = FlyioSecondarySurface, Font = CreateFlyioFont(7.8F, FontStyle.Bold), Padding = new Padding(6, 2, 4, 1), Margin = new Padding(0, 5, 0, 3) };
            int row = layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            layout.Controls.Add(heading, 0, row);
            layout.SetColumnSpan(heading, 2);
        }

        private void AddMetricRow(TableLayoutPanel layout, string caption, string key)
        {
            int row = layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var captionLabel = new Label { AutoSize = true, Anchor = AnchorStyles.Left, Text = caption, ForeColor = FlyioSecondaryText, Font = CreateFlyioFont(7.8F, FontStyle.Bold), Margin = new Padding(2, 4, 6, 4) };
            var valueLabel = NewStatusValue(key, "--", 9.4F, FlyioText);
            valueLabel.AutoSize = false;
            valueLabel.Dock = DockStyle.Fill;
            valueLabel.TextAlign = ContentAlignment.MiddleRight;
            valueLabel.Margin = new Padding(2, 3, 2, 3);
            layout.Controls.Add(captionLabel, 0, row);
            layout.Controls.Add(valueLabel, 1, row);
        }

        private Label NewHeading(string text, float size)
        {
            return new Label { AutoSize = true, Text = text, ForeColor = FlyioText, Font = CreateFlyioFont(size, FontStyle.Bold), Margin = new Padding(0, 0, 0, 6) };
        }

        private Label NewStatusValue(string key, string text, float size, Color color)
        {
            var label = new Label { AutoSize = true, Text = text, ForeColor = color, Font = CreateFlyioFont(size), Margin = new Padding(0, 1, 0, 2), AutoEllipsis = true };
            flyioStatusValues[key] = label;
            return label;
        }

        private Panel NewSurfacePanel()
        {
            return new Panel { Dock = DockStyle.Fill, BackColor = FlyioSurface, Margin = Padding.Empty };
        }

        private Button NewSelectorButton(string text, bool selected)
        {
            var button = new Button { AutoSize = false, Width = 96, Height = 30, Text = text, FlatStyle = FlatStyle.Flat, ForeColor = FlyioText, BackColor = selected ? FlyioAccent : FlyioSecondarySurface, Font = CreateFlyioFont(8.8F, FontStyle.Bold), Cursor = Cursors.Hand, Margin = new Padding(0, 0, 5, 0), Padding = new Padding(7, 1, 7, 1), TextAlign = ContentAlignment.MiddleCenter };
            button.FlatAppearance.BorderColor = selected ? FlyioAccent : FlyioDivider;
            button.FlatAppearance.MouseOverBackColor = FlyioHover;
            button.FlatAppearance.MouseDownBackColor = FlyioPressed;
            return button;
        }

        private void UpdateFlyioOperationsStatus(CurrentState state)
        {
            if (state == null || flyioStatusValues.Count == 0)
                return;
            bool connected = state.connected;
            string unavailable = "--";
            SetFlyioStatus("connection", connected ? "● CONNECTED" : "● DISCONNECTED", connected ? FlyioHealthy : FlyioDanger);
            SetFlyioStatus("vehicle", connected ? state.firmware.ToString() : unavailable);
            SetFlyioStatus("mode", connected ? state.mode : unavailable);
            SetFlyioStatus("flightmode", "MODE  " + (connected ? state.mode : unavailable));
            SetFlyioStatus("gps", connected ? state.satcount + " SAT" : unavailable, connected && state.satcount >= 6 ? FlyioHealthy : FlyioUnavailable);
            SetFlyioStatus("battery", connected ? string.Format("{0:0}%   {1:0.0} V", state.battery_remaining, state.battery_voltage) : unavailable, connected && state.battery_remaining > 20 ? FlyioHealthy : FlyioUnavailable);
            SetFlyioStatus("link", connected ? string.Format("{0:0}%", state.linkqualitygcs) : unavailable, connected && state.linkqualitygcs > 40 ? FlyioHealthy : FlyioUnavailable);
            SetFlyioStatus("altitude", connected ? string.Format("{0:0.0} {1}", state.alt, CurrentState.AltUnit) : unavailable);
            SetFlyioStatus("groundspeed", connected ? string.Format("{0:0.0} {1}", state.groundspeed, CurrentState.SpeedUnit) : unavailable);
            SetFlyioStatus("verticalspeed", connected ? string.Format("{0:+0.0;-0.0;0.0} {1}", state.verticalspeed, CurrentState.SpeedUnit) : unavailable);
            SetFlyioStatus("latitude", connected ? state.lat.ToString("0.000000") : unavailable);
            SetFlyioStatus("longitude", connected ? state.lng.ToString("0.000000") : unavailable);
            SetFlyioStatus("bottomgps", connected ? "GPS ● " + state.satcount : "GPS --", connected && state.satcount >= 6 ? FlyioHealthy : FlyioUnavailable);
            SetFlyioStatus("bottomlink", connected ? string.Format("LINK {0:0}%", state.linkqualitygcs) : "LINK --");
            SetFlyioStatus("bottombattery", connected ? string.Format("BAT {0:0}%  {1:0.0}V", state.battery_remaining, state.battery_voltage) : "BAT --");
            SetFlyioStatus("bottommode", "MODE: " + (connected ? state.mode : unavailable));
            SetFlyioStatus("bottomposition", connected ? string.Format("LAT {0:0.000000}  LON {1:0.000000}", state.lat, state.lng) : "POSITION --");
            SetFlyioStatus("bottomsystem", connected ? (state.armed ? "ARMED" : "READY") : "OFFLINE", connected ? FlyioHealthy : FlyioDanger);
        }

        private void SetFlyioStatus(string key, string text, Color? color = null)
        {
            Label label;
            if (!flyioStatusValues.TryGetValue(key, out label))
                return;
            label.Text = text;
            if (color.HasValue)
                label.ForeColor = color.Value;
        }
    }
}
