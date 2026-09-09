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
                Padding = new Padding(12, 10, 8, 8),
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
                Width = 76,
                Height = 32,
                Text = "Follow",
                TextAlign = ContentAlignment.MiddleCenter,
                Checked = CHK_autopan.Checked,
                FlatStyle = FlatStyle.Flat,
                BackColor = CHK_autopan.Checked ? FlyioDataAccent : FlyioDataSecondarySurface,
                ForeColor = CHK_autopan.Checked ? Color.White : FlyioDataText,
                Font = CreateFlyioFont(8.5F, FontStyle.Bold),
                Margin = new Padding(0, 0, 5, 0)
            };
            flyioFollowButton.FlatAppearance.BorderColor = FlyioDataBorder;
            flyioFollowButton.CheckedChanged += (sender, args) =>
            {
                CHK_autopan.Checked = flyioFollowButton.Checked;
                flyioFollowButton.BackColor = flyioFollowButton.Checked ? FlyioDataAccent : FlyioDataSecondarySurface;
                flyioFollowButton.ForeColor = flyioFollowButton.Checked ? Color.FromArgb(22, 28, 38) : FlyioDataText;
            };

            flyioProviderSelector = NewFlyioToolbarComboBox(220);
            flyioProviderSelector.DisplayMember = "Name";
            flyioProviderSelector.ValueMember = "Name";
            flyioProviderSelector.DataSource = GMapProviders.List.ToArray();
            SyncFlyioProviderSelector(gMapControl1.MapProvider);
            flyioProviderSelector.SelectedValueChanged += FlyioProviderSelectorChanged;

            flyioGridSelector = NewFlyioToolbarComboBox(108);
            flyioGridSelector.Items.AddRange(new object[] { "Grid: Off", "Tile Grid" });
            flyioGridSelector.SelectedIndex = gMapControl1.ShowTileGridLines ? 1 : 0;
            flyioGridSelector.SelectedIndexChanged += (sender, args) =>
            {
                gMapControl1.ShowTileGridLines = flyioGridSelector.SelectedIndex == 1;
                gMapControl1.Invalidate();
            };

            var location = NewFlyioToolbarButton("Location", 82);
            location.Click += (sender, args) =>
            {
                if (MainV2.comPort.MAV.cs.lat != 0 || MainV2.comPort.MAV.cs.lng != 0)
                    gMapControl1.Position = new GMap.NET.PointLatLng(MainV2.comPort.MAV.cs.lat, MainV2.comPort.MAV.cs.lng);
            };

            var more = NewFlyioToolbarButton("More", 66);
            more.Click += (sender, args) => contextMenuStripMap.Show(more, new Point(0, more.Height));

            flyioMapToolbar.Controls.Add(flyioFollowButton);
            flyioMapToolbar.Controls.Add(flyioProviderSelector);
            flyioMapToolbar.Controls.Add(flyioGridSelector);
            flyioMapToolbar.Controls.Add(location);
            flyioMapToolbar.Controls.Add(more);
            // Reuse the real tuning checkbox and its existing graph handler.
            CB_tuning.Visible = true;
            CB_tuning.AutoSize = true;
            CB_tuning.Text = "Tuning graph";
            CB_tuning.Margin = new Padding(6, 8, 5, 0);
            flyioMapToolbar.Controls.Add(CB_tuning);

            flyioMapHost = new Panel
            {
                Name = "flyioMapHost",
                Dock = DockStyle.Fill,
                BackColor = FlyioDataSecondarySurface,
                Padding = new Padding(0, 60, 0, 0),
                Margin = Padding.Empty
            };
            gMapControl1.Parent = flyioMapHost;
            gMapControl1.Dock = DockStyle.Fill;
            flyioMapHost.Controls.Add(flyioMapToolbar);
            splitContainer1.Panel2.Controls.Add(flyioMapHost);
            flyioMapHost.SendToBack();
            flyioMapToolbar.BringToFront();
            gMapControl1.OnMapTypeChanged += FlyioMapTypeChanged;
            Load += (sender, args) => SyncFlyioProviderSelector(gMapControl1.MapProvider);
        }

        private ComboBox NewFlyioToolbarComboBox(int width)
        {
            return new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = width,
                Height = 32,
                BackColor = FlyioDataSecondarySurface,
                ForeColor = FlyioDataText,
                FlatStyle = FlatStyle.Flat,
                Font = CreateFlyioFont(9F),
                Margin = new Padding(0, 1, 5, 0)
            };
        }

        private Button NewFlyioToolbarButton(string text, int width)
        {
            var button = new Button
            {
                Text = text,
                Width = width,
                Height = 32,
                BackColor = FlyioDataSurface,
                ForeColor = FlyioDataText,
                FlatStyle = FlatStyle.Flat,
                Font = CreateFlyioFont(9F, FontStyle.Bold),
                Margin = new Padding(0, 0, 5, 0)
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
