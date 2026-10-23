using GMap.NET.MapProviders;
using MissionPlanner.Utilities;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MissionPlanner.GCSViews
{
    public partial class FlightData
    {
        private FlowLayoutPanel flyioMapToolbar;
        private CheckBox flyioFollowButton;
        private ComboBox flyioProviderSelector;
        private ComboBox flyioGridSelector;
        private Panel flyioMapHost;
        private Button flyioZoomOut;
        private Button flyioZoomIn;
        private bool flyioProviderSelectionChanging;

        private void InitializeFlyioMapToolbar()
        {
            flyioMapToolbar = new FlowLayoutPanel
            {
                Name = "flyioMapToolbar",
                Dock = DockStyle.Top,
                Height = 52,
                WrapContents = true,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                AutoScroll = false,
                BackColor = FlyioDataBackground,
                Padding = new Padding(FlyioPixels(8)),
                Margin = Padding.Empty
            };
            flyioMapToolbar.Paint += (sender, args) =>
            {
                using (var divider = new Pen(FlyioDataBorder))
                    args.Graphics.DrawLine(divider, 0, flyioMapToolbar.Height - 1,
                        flyioMapToolbar.Width, flyioMapToolbar.Height - 1);
            };

            flyioFollowButton = new CheckBox
            {
                Appearance = Appearance.Button,
                AutoSize = false,
                Width = FlyioPixels(76),
                Height = FlyioPixels(32),
                Text = "Follow",
                TextAlign = ContentAlignment.MiddleCenter,
                Checked = CHK_autopan.Checked,
                FlatStyle = FlatStyle.Flat,
                BackColor = CHK_autopan.Checked ? FlyioDataAccent : FlyioDataSecondarySurface,
                ForeColor = CHK_autopan.Checked ? Color.White : FlyioDataText,
                Font = flyioDataButtonFont,
                Margin = new Padding(0, 0, FlyioPixels(5), 0)
            };
            flyioFollowButton.FlatAppearance.BorderColor = FlyioDataBorder;
            flyioFollowButton.CheckedChanged += (sender, args) =>
            {
                CHK_autopan.Checked = flyioFollowButton.Checked;
                flyioFollowButton.BackColor = flyioFollowButton.Checked ? FlyioDataAccent : FlyioDataSecondarySurface;
                flyioFollowButton.ForeColor = flyioFollowButton.Checked ? Color.FromArgb(22, 28, 38) : FlyioDataText;
            };

            flyioProviderSelector = NewFlyioToolbarComboBox(180);
            flyioProviderSelector.DisplayMember = "Name";
            flyioProviderSelector.ValueMember = "Name";
            flyioProviderSelector.DataSource = GMapProviders.List.ToArray();
            SyncFlyioProviderSelector(gMapControl1.MapProvider);
            flyioProviderSelector.SelectedValueChanged += FlyioProviderSelectorChanged;

            flyioGridSelector = NewFlyioToolbarComboBox(94);
            flyioGridSelector.Items.AddRange(new object[] { "Grid: Off", "Tile Grid" });
            flyioGridSelector.SelectedIndex = gMapControl1.ShowTileGridLines ? 1 : 0;
            flyioGridSelector.SelectedIndexChanged += (sender, args) =>
            {
                gMapControl1.ShowTileGridLines = flyioGridSelector.SelectedIndex == 1;
                gMapControl1.Invalidate();
            };

            var location = NewFlyioToolbarButton("Location", 76);
            location.Click += (sender, args) =>
            {
                if (MainV2.comPort.MAV.cs.lat != 0 || MainV2.comPort.MAV.cs.lng != 0)
                    gMapControl1.Position = new GMap.NET.PointLatLng(MainV2.comPort.MAV.cs.lat, MainV2.comPort.MAV.cs.lng);
            };

            var more = NewFlyioToolbarButton("More", 76);
            more.Click += (sender, args) => contextMenuStripMap.Show(more, new Point(0, more.Height));

            flyioMapToolbar.Controls.Add(flyioFollowButton);
            flyioMapToolbar.Controls.Add(flyioProviderSelector);
            flyioMapToolbar.Controls.Add(flyioGridSelector);
            flyioMapToolbar.Controls.Add(location);
            flyioMapToolbar.Controls.Add(more);
            // Reuse the real tuning checkbox and its existing graph handler.
            CB_tuning.Visible = true;
            CB_tuning.AutoSize = true;
            CB_tuning.Text = "Tuning";
            CB_tuning.Font = flyioDataControlFont;
            toolTip1.SetToolTip(CB_tuning, "Show tuning graph");
            CB_tuning.Margin = new Padding(6, 8, 5, 0);
            flyioMapToolbar.Controls.Add(CB_tuning);

            // Retain legacy controls and their synchronization handlers, but expose
            // larger zoom buttons in the DATA toolbar instead of the narrow slider.
            TRK_zoom.Visible = false;
            Zoomlevel.Visible = false;
            label1.Visible = false;
            flyioZoomOut = NewFlyioToolbarButton("\u2212", 40);
            flyioZoomIn = NewFlyioToolbarButton("+", 40);
            flyioZoomOut.Name = "flyioZoomOut";
            flyioZoomIn.Name = "flyioZoomIn";
            flyioZoomOut.AccessibleName = "Zoom out";
            flyioZoomIn.AccessibleName = "Zoom in";
            toolTip1.SetToolTip(flyioZoomOut, "Zoom out");
            toolTip1.SetToolTip(flyioZoomIn, "Zoom in");
            flyioZoomOut.Click += (sender, args) => ChangeFlyioMapZoom(-1);
            flyioZoomIn.Click += (sender, args) => ChangeFlyioMapZoom(1);
            var zoomButtons = new FlowLayoutPanel
            {
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false, Margin = Padding.Empty, Padding = Padding.Empty
            };
            zoomButtons.Controls.Add(flyioZoomOut);
            zoomButtons.Controls.Add(flyioZoomIn);
            flyioMapToolbar.Controls.Add(zoomButtons);

            flyioMapHost = new Panel
            {
                Name = "flyioMapHost",
                Dock = DockStyle.Fill,
                BackColor = FlyioDataSecondarySurface,
                Padding = Padding.Empty,
                Margin = Padding.Empty
            };
            // Move retained top overlays with the map before removing its old inset.
            // Bottom-anchored legacy labels remain in Panel2 with their existing bindings.
            distanceBar1.Parent = gMapControl1;
            distanceBar1.Dock = DockStyle.Top;
            distanceBar1.Height = FlyioPixels(22);
            but_disablejoystick.Parent = gMapControl1;
            but_disablejoystick.Location = new Point(FlyioPixels(4), distanceBar1.Height + FlyioPixels(4));
            windDir1.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            windDir1.Dock = DockStyle.None;
            windDir1.MinimumSize = Size.Empty;
            windDir1.MaximumSize = Size.Empty;
            windDir1.BackColor = FlyioDataSecondarySurface;
            windDir1.TelemetryAvailable = false;
            windDir1.AccessibleName = "Wind";
            flyioMapToolbar.Controls.Add(windDir1);

            // Equal-height slots center native combo boxes and the checkbox without
            // forcing a native combo-box height or changing existing event handlers.
            Control[] toolbarControls = { flyioFollowButton, flyioProviderSelector,
                flyioGridSelector, location, more, CB_tuning, zoomButtons, windDir1 };
            int[] logicalWidths = { 76, 180, 94, 76, 76, 0, 85, 40 };
            var slots = toolbarControls.Select(control => new Panel
            {
                BackColor = FlyioDataBackground,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                TabStop = false
            }).ToArray();
            for (int index = 0; index < toolbarControls.Length; index++)
            {
                Control control = toolbarControls[index];
                Panel slot = slots[index];
                control.Margin = Padding.Empty;
                slot.Controls.Add(control);
                flyioMapToolbar.Controls.Add(slot);
            }
            Action layoutToolbar = () =>
            {
                flyioMapToolbar.SuspendLayout();
                flyioMapToolbar.Padding = new Padding(FlyioPixels(8));
                int rowHeight = Math.Max(FlyioPixels(40), Math.Max(
                    flyioProviderSelector.PreferredHeight, CB_tuning.PreferredSize.Height));
                flyioZoomOut.Size = flyioZoomIn.Size = new Size(FlyioPixels(40), FlyioPixels(32));
                flyioZoomOut.Margin = new Padding(0, 0, FlyioPixels(5), 0);
                flyioZoomIn.Margin = Padding.Empty;
                for (int index = 0; index < toolbarControls.Length; index++)
                {
                    Control control = toolbarControls[index];
                    int width = logicalWidths[index] == 0 ? control.PreferredSize.Width : FlyioPixels(logicalWidths[index]);
                    if (control == windDir1) control.Size = new Size(width, FlyioPixels(40));
                    else if (control is Button || control == flyioFollowButton)
                        control.Size = new Size(width, FlyioPixels(32));
                    else control.Width = width;
                    slots[index].Size = new Size(width, rowHeight);
                    slots[index].Margin = new Padding(0, 0, FlyioPixels(6), FlyioPixels(4));
                    control.Location = new Point(0, Math.Max(0, (rowHeight - control.Height) / 2));
                }
                distanceBar1.Height = FlyioPixels(22);
                but_disablejoystick.Location = new Point(FlyioPixels(4), distanceBar1.Height + FlyioPixels(4));
                flyioMapToolbar.ResumeLayout(true);
            };
            Load += (sender, args) => layoutToolbar();
            flyioMapToolbar.DpiChangedAfterParent += (sender, args) => layoutToolbar();
            layoutToolbar();

            // Read existing packet metadata only. Passive telemetry currently supplies
            // no wind; neither a disconnected default nor an old sample is calm wind.
            var windAvailabilityTimer = new Timer(components) { Interval = 500 };
            windAvailabilityTimer.Tick += (sender, args) =>
            {
                var mav = MainV2.comPort.MAV;
                var wind = mav.getPacketLast((uint)MAVLink.MAVLINK_MSG_ID.WIND);
                bool available = flyioTelemetry == null && mav.cs.connected && wind != null &&
                    (DateTime.UtcNow - wind.rxtime).TotalSeconds >= 0 &&
                    (DateTime.UtcNow - wind.rxtime).TotalSeconds <= 5 &&
                    !double.IsNaN(windDir1.Speed) && !double.IsInfinity(windDir1.Speed) &&
                    !double.IsNaN(windDir1.Direction) && !double.IsInfinity(windDir1.Direction);
                windDir1.TelemetryAvailable = available;
                string description = available ? "Wind: " + windDir1.Speed.ToString("0.0") + " " + CurrentState.SpeedUnit
                    : "Wind unavailable: no recent wind telemetry";
                if (windDir1.AccessibleDescription != description)
                {
                    windDir1.AccessibleDescription = description;
                    toolTip1.SetToolTip(windDir1, description);
                }
            };
            windAvailabilityTimer.Start();
            gMapControl1.Parent = flyioMapHost;
            gMapControl1.Dock = DockStyle.Fill;
            flyioMapHost.Controls.Add(flyioMapToolbar);
            splitContainer1.Panel2.Controls.Add(flyioMapHost);
            flyioMapHost.SendToBack();
            flyioMapToolbar.BringToFront();
            // Reserve the full toolbar height before the map fills the remainder.
            gMapControl1.BringToFront();
            gMapControl1.OnMapTypeChanged += FlyioMapTypeChanged;
            Load += (sender, args) => SyncFlyioProviderSelector(gMapControl1.MapProvider);
        }

        private void ChangeFlyioMapZoom(int delta)
        {
            gMapControl1.Zoom = Math.Max(gMapControl1.MinZoom,
                Math.Min(gMapControl1.MaxZoom, gMapControl1.Zoom + delta));
            // OnMapZoomChanged keeps both legacy controls synchronized.
            UpdateOverlayVisibility();
        }

        private ComboBox NewFlyioToolbarComboBox(int width)
        {
            return new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = FlyioPixels(width),
                Height = FlyioPixels(32),
                BackColor = FlyioDataSecondarySurface,
                ForeColor = FlyioDataText,
                FlatStyle = FlatStyle.Flat,
                Font = flyioDataControlFont,
                Margin = new Padding(0, FlyioPixels(3), FlyioPixels(5), 0)
            };
        }

        private Button NewFlyioToolbarButton(string text, int width)
        {
            var button = new Button
            {
                Text = text,
                Width = FlyioPixels(width),
                Height = FlyioPixels(32),
                BackColor = FlyioDataSurface,
                ForeColor = FlyioDataText,
                FlatStyle = FlatStyle.Flat,
                Font = flyioDataButtonFont,
                Padding = new Padding(FlyioPixels(4), 0, FlyioPixels(4), 0),
                Margin = new Padding(0, 0, FlyioPixels(5), 0)
            };
            button.FlatAppearance.BorderColor = FlyioDataBorder;
            button.FlatAppearance.MouseOverBackColor = FlyioDataSecondarySurface;
            return button;
        }

        private void FlyioProviderSelectorChanged(object sender, EventArgs e)
        {
            if (flyioProviderSelectionChanging || !(flyioProviderSelector.SelectedItem is GMapProvider provider))
                return;

            try
            {
                if (FlightPlanner.instance != null && FlightPlanner.instance.comboBoxMapType != null)
                {
                    int index = FindProviderIndex(FlightPlanner.instance.comboBoxMapType.Items, provider.Name);
                    if (index >= 0)
                        FlightPlanner.instance.comboBoxMapType.SelectedIndex = index;
                }
                else
                {
                    gMapControl1.MapProvider = provider;
                    Settings.Instance["MapType"] = provider.Name;
                }
            }
            catch
            {
                SyncFlyioProviderSelector(gMapControl1.MapProvider);
                throw;
            }
        }

        private void FlyioMapTypeChanged(GMapProvider provider)
        {
            if (flyioProviderSelector == null || flyioProviderSelector.IsDisposed)
                return;

            SyncFlyioProviderSelector(provider ?? gMapControl1.MapProvider);
        }

        private void SyncFlyioProviderSelector(GMapProvider provider)
        {
            if (flyioProviderSelector == null || flyioProviderSelector.IsDisposed || provider == null)
                return;

            int index = FindProviderIndex(flyioProviderSelector.Items, provider.Name);
            if (index < 0)
                return;

            flyioProviderSelectionChanging = true;
            flyioProviderSelector.SelectedIndex = index;
            flyioProviderSelectionChanging = false;
        }

        private static int FindProviderIndex(ComboBox.ObjectCollection items, string providerName)
        {
            if (string.IsNullOrEmpty(providerName))
                return -1;

            for (int index = 0; index < items.Count; index++)
            {
                if (items[index] is GMapProvider provider &&
                    string.Equals(provider.Name, providerName, StringComparison.OrdinalIgnoreCase))
                    return index;
            }

            return -1;
        }
    }
}
