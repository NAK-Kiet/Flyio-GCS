using System;
using System.Drawing;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
        private FlowLayoutPanel flyioDataNavigation;
        private Font flyioDataControlFont;
        private Font flyioDataButtonFont;
        private Font flyioDataCompactFont;
        private Dictionary<Control, TableLayoutPanelCellPosition> flyioActionCells;
        private Dictionary<Control, Control[]> flyioLongListControls;
        private bool flyioPolishingLayout;

        // Only DATA uses this host. Keep the original TabControl API and events,
        // but let the selected page occupy the native header area as well.
        private sealed class FlyioDataTabControl : TabControl
        {
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            private static extern IntPtr FindWindowEx(IntPtr parent, IntPtr after, string className, string title);
            [DllImport("user32.dll")]
            private static extern bool ShowWindow(IntPtr window, int command);

            public override Rectangle DisplayRectangle => ClientRectangle;

            protected override void OnLayout(LayoutEventArgs e)
            {
                base.OnLayout(e);
                // Native single-line tabs create scroll arrows even though their
                // headers are covered by the selected page. DATA uses buttons.
                if (IsHandleCreated && Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    IntPtr arrows = FindWindowEx(Handle, IntPtr.Zero, "msctls_updown32", null);
                    if (arrows != IntPtr.Zero) ShowWindow(arrows, 0);
                }
            }

            protected override void WndProc(ref Message m)
            {
                const int TcmAdjustRect = 0x1328;
                if (m.Msg == TcmAdjustRect && !DesignMode)
                {
                    m.Result = IntPtr.Zero;
                    return;
                }
                base.WndProc(ref m);
            }
        }

        private void InitializeFlyioDataLayout()
        {
            flyioResponsiveLayoutEnabled = true;

            MainH.FixedPanel = FixedPanel.None;
            MainH.BorderStyle = BorderStyle.None;
            MainH.SplitterWidth = Math.Max(6, (int)Math.Round(6 * DeviceDpi / 96F));
            StyleFlyioSplitter(MainH);
            MainH.Panel1MinSize = 360;
            MainH.Panel2MinSize = 420;
            MainH.Panel2.Padding = Padding.Empty;

            // Preserve the existing tuning toggle and graph when the workspace resizes.
            splitContainer1.Panel1Collapsed = !CB_tuning.Checked;
            splitContainer1.IsSplitterFixed = false;
            splitContainer1.Panel1MinSize = 0;
            splitContainer1.SplitterWidth = Math.Max(6, FlyioPixels(6));
            StyleFlyioSplitter(splitContainer1);
            var graph = zg1.GraphPane;
            graph.Fill = new ZedGraph.Fill(Color.White);
            graph.Chart.Fill = new ZedGraph.Fill(Color.White);
            graph.Title.FontSpec.FontColor = Color.Black;
            graph.Legend.Fill = new ZedGraph.Fill(Color.White);
            graph.Legend.FontSpec.FontColor = Color.Black;
            foreach (ZedGraph.Axis axis in new ZedGraph.Axis[] { graph.XAxis, graph.YAxis, graph.Y2Axis })
            {
                axis.Color = Color.Black;
                axis.MajorTic.Color = Color.Black;
                axis.MinorTic.Color = Color.Black;
                axis.Title.FontSpec.FontColor = Color.Black;
                axis.Scale.FontSpec.FontColor = Color.Black;
                axis.MajorGrid.Color = Color.LightGray;
                axis.MinorGrid.Color = Color.LightGray;
            }
            splitContainer1.Margin = Padding.Empty;
            splitContainer1.Padding = Padding.Empty;
            splitContainer1.Panel2.Padding = Padding.Empty;
            tableMap.Margin = Padding.Empty;
            tableMap.Padding = Padding.Empty;

            SubMainLeft.FixedPanel = FixedPanel.None;
            SubMainLeft.BorderStyle = BorderStyle.None;
            SubMainLeft.SplitterWidth = Math.Max(4, FlyioPixels(4));
            SubMainLeft.Panel1MinSize = 180;
            SubMainLeft.Panel2MinSize = 180;

            hud1.Margin = Padding.Empty;
            hud1.Dock = DockStyle.Fill;
            hud1.Paint += DrawFlyioHudFrame;
            tabControlactions.Margin = Padding.Empty;
            tabControlactions.Dock = DockStyle.Fill;
            tabControlactions.Multiline = false;
            flyioDataControlFont = new Font("Segoe UI", 8.25F);
            flyioDataButtonFont = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            flyioDataCompactFont = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            Disposed += (sender, args) =>
            {
                flyioDataControlFont.Dispose();
                flyioDataButtonFont.Dispose();
                flyioDataCompactFont.Dispose();
            };
            InitializeFlyioDataNavigation();
            InitializeFlyioDataScrolling();
            InitializeFlyioPanelPolish();

            StyleFlyioQuickTelemetry();

            CreateFlyioMapInformationRow();
            CreateFlyioDataStatusBar();

            Load += FlyioDataLoaded;
            MainH.SizeChanged += FlyioDataLayoutChanged;
            MainH.SplitterMoved += FlyioMainSplitterMoved;
            SubMainLeft.SizeChanged += FlyioLeftPanelSizeChanged;
            ApplyFlyioResponsiveGeometry();
        }

        private void InitializeFlyioDataNavigation()
        {
            flyioDataNavigation = new FlowLayoutPanel
            {
                Name = "flyioDataNavigation",
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = true,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                BackColor = FlyioDataBackground,
                ContextMenuStrip = contextMenuStripactionstab
            };
            SubMainLeft.Panel2.Controls.Add(flyioDataNavigation);
            flyioDataNavigation.SizeChanged += (sender, args) =>
            {
                SizeFlyioDataNavigation();
                FlyioLeftPanelSizeChanged(sender, args);
            };
            tabControlactions.BringToFront();
            // Customize still operates on the original page collection/settings.
            // The native multiline option has no meaning for this fixed header.
            multiLineToolStripMenuItem.Visible = false;
            tabControlactions.ControlAdded += FlyioDataTabsChanged;
            tabControlactions.ControlRemoved += FlyioDataTabsChanged;
            tabControlactions.SelectedIndexChanged += (sender, args) => UpdateFlyioDataNavigationSelection();
            SyncFlyioDataNavigation();
        }

        private void FlyioDataTabsChanged(object sender, ControlEventArgs e)
        {
            SyncFlyioDataNavigation();
        }

        private void FlyioDataTabTextChanged(object sender, EventArgs e)
        {
            SyncFlyioDataNavigation();
        }

        private void SyncFlyioDataNavigation()
        {
            if (Disposing || IsDisposed || flyioDataNavigation.IsDisposed)
                return;

            flyioDataNavigation.SuspendLayout();
            try
            {
                // ControlRemoved can run before the native tab count is updated.
                // Parent membership already reflects the actual content controls.
                var pages = tabControlactions.TabPages.Cast<TabPage>()
                    .Where(page => page.Parent == tabControlactions).ToArray();
                foreach (Button button in flyioDataNavigation.Controls.Cast<Button>().ToArray())
                {
                    var page = (TabPage)button.Tag;
                    if (pages.Contains(page)) continue;
                    page.TextChanged -= FlyioDataTabTextChanged;
                    button.Dispose();
                }

                for (int index = 0; index < pages.Length; index++)
                {
                    TabPage page = pages[index];
                    var button = flyioDataNavigation.Controls.Cast<Button>().FirstOrDefault(item => item.Tag == page);
                    if (button == null)
                    {
                        button = new Button
                        {
                            Name = "flyioNavigate_" + page.Name,
                            Tag = page,
                            AutoSize = false,
                            Padding = new Padding(FlyioPixels(4), 0, FlyioPixels(4), 0),
                            Margin = new Padding(0, 0, FlyioPixels(4), FlyioPixels(4)),
                            FlatStyle = FlatStyle.Flat,
                            Font = flyioDataButtonFont,
                            UseVisualStyleBackColor = false,
                            ContextMenuStrip = contextMenuStripactionstab
                        };
                        button.Click += (sender, args) =>
                        {
                            if (tabControlactions.TabPages.Contains(page))
                                tabControlactions.SelectedTab = page;
                        };
                        page.TextChanged += FlyioDataTabTextChanged;
                        flyioDataNavigation.Controls.Add(button);
                    }
                    button.Text = page.Text;
                    button.AccessibleName = page.Text;
                    button.TabIndex = index;
                    flyioDataNavigation.Controls.SetChildIndex(button, index);
                }
                flyioDataNavigation.Visible = pages.Length != 0;
                SizeFlyioDataNavigation();
                UpdateFlyioDataNavigationSelection();
            }
            finally
            {
                flyioDataNavigation.ResumeLayout(true);
            }
        }

        private void UpdateFlyioDataNavigationSelection()
        {
            // Selection changes colors only: no reorder, font or size changes.
            foreach (Button button in flyioDataNavigation.Controls)
            {
                bool selected = button.Tag == tabControlactions.SelectedTab;
                button.BackColor = selected ? FlyioDataAccent : FlyioDataSecondarySurface;
                button.ForeColor = selected ? FlyioDataBackground : FlyioDataText;
                button.FlatAppearance.BorderColor = selected ? FlyioDataAccent : FlyioDataBorder;
                button.AccessibleDescription = selected ? "Selected DATA page" : "Select DATA page";
            }
        }

        private int FlyioPixels(int value) => (int)Math.Round(value * DeviceDpi / 96F);

        private void SizeFlyioDataNavigation()
        {
            if (flyioDataNavigation == null || flyioDataNavigation.Controls.Count == 0)
                return;

            int gap = FlyioPixels(4);
            int available = Math.Max(1, flyioDataNavigation.ClientSize.Width);
            bool compact = available < FlyioPixels(300);
            // WinForms can scale fonts after construction. Measure and lay out
            // using the intended DATA font, including after a window resize.
            foreach (Button button in flyioDataNavigation.Controls)
            {
                button.Font = compact ? flyioDataCompactFont : flyioDataButtonFont;
                button.UseCompatibleTextRendering = compact;
                int padding = FlyioPixels(compact ? 1 : 4);
                button.Padding = new Padding(padding, 0, padding, 0);
            }
            int minimum = Math.Max(FlyioPixels(120), flyioDataNavigation.Controls.Cast<Button>()
                .Max(button => TextRenderer.MeasureText(button.Text, button.Font).Width) + FlyioPixels(12));
            int columns = Math.Max(1, available / (minimum + gap));


            var size = new Size(Math.Max(1, available / columns - gap), Math.Max(FlyioPixels(34), flyioDataButtonFont.Height + FlyioPixels(12)));
            flyioDataNavigation.SuspendLayout();
            foreach (Button button in flyioDataNavigation.Controls)
                if (button.Size != size) button.Size = size;
            flyioDataNavigation.ResumeLayout(true);
        }

        private void StyleFlyioSplitter(SplitContainer split)
        {
            split.IsSplitterFixed = false;
            split.Paint += (sender, args) =>
            {
                Rectangle bounds = split.SplitterRectangle;
                bool hover = bounds.Contains(split.PointToClient(Cursor.Position));
                using (var background = new SolidBrush(hover ? FlyioDataSecondarySurface : FlyioDataBackground))
                using (var grip = new Pen(hover ? FlyioDataAccent : FlyioDataBorder, 2))
                {
                    args.Graphics.FillRectangle(background, bounds);
                    int x = bounds.Left + bounds.Width / 2;
                    int y = bounds.Top + bounds.Height / 2;
                    if (split.Orientation == Orientation.Vertical)
                        args.Graphics.DrawLine(grip, x, y - 18, x, y + 18);
                    else
                        args.Graphics.DrawLine(grip, x - 18, y, x + 18, y);
                }
            };
            split.MouseMove += (sender, args) => split.Invalidate(split.SplitterRectangle);
            split.MouseLeave += (sender, args) => split.Invalidate(split.SplitterRectangle);
        }

        private void InitializeFlyioDataScrolling()
        {
            foreach (TabPage page in tabControlactions.TabPages)
                page.AutoScroll = true;
            tabControlactions.ControlAdded += (sender, args) =>
            {
                if (args.Control is TabPage page) page.AutoScroll = true;
            };

            // Grow the list containers vertically without resizing the original
            // Servo/Relay and Aux controls. The containing TabPage owns scrolling.
            foreach (FlowLayoutPanel panel in new[] { flowLayoutPanelServos, flowLayoutPanel1 })
            {
                panel.FlowDirection = FlowDirection.TopDown;
                panel.WrapContents = false;
                panel.Dock = DockStyle.Top;
                panel.AutoSize = true;
                panel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            }

            // The viewport determines table width. Rows grow vertically, so the
            // page scrolls without forcing a preferred-width horizontal scrollbar.
            foreach (TableLayoutPanel panel in new[] { tableLayoutPanel1, tableLayoutPanel2, tableLayoutPaneltlogs })
            {
                panel.Dock = DockStyle.Top;
                panel.AutoScroll = false;
                panel.AutoSize = false;
                panel.MinimumSize = Size.Empty;
                panel.Margin = Padding.Empty;
                panel.ColumnStyles.Clear();
                for (int index = 0; index < panel.ColumnCount; index++)
                    panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / panel.ColumnCount));
            }
        }

        private void InitializeFlyioPanelPolish()
        {
            flyioActionCells = tableLayoutPanel1.Controls.Cast<Control>()
                .ToDictionary(control => control, control => tableLayoutPanel1.GetCellPosition(control));
            flyioLongListControls = new[] { flowLayoutPanelServos, flowLayoutPanel1 }
                .SelectMany(list => list.Controls.Cast<Control>()).ToDictionary(row => row,
                    row => row.Controls.Cast<Control>()
                        .Where(child => !(row is AuxOptions) || child is Button || child is ComboBox)
                        .OrderBy(child => child.Left).ToArray());
            foreach (TabPage page in new[] { tabActions, tabScripts, tabPayload, tabTLogs, tablogbrowse, tabServo, tabAuxFunction })
            {
                page.Padding = new Padding(FlyioPixels(6));
                PolishFlyioControls(page);
                page.SizeChanged += (sender, args) => LayoutFlyioPanels();
            }
            foreach (ModifyandSet editor in new[] { modifyandSetAlt, modifyandSetSpeed, modifyandSetLoiterRad })
            {
                var flow = editor.Controls.OfType<FlowLayoutPanel>().Single();
                flow.FlowDirection = FlowDirection.TopDown;
                flow.WrapContents = false;
                flow.AutoSize = false;
                editor.AutoSize = false;
                editor.Button.AutoSize = false;
                editor.Button.Margin = new Padding(0, FlyioPixels(3), 0, 0);
                editor.NumericUpDown.Margin = Padding.Empty;
                editor.SizeChanged += (sender, args) =>
                {
                    editor.NumericUpDown.Width = editor.ClientSize.Width;
                    editor.Button.Size = new Size(editor.ClientSize.Width,
                        Math.Max(FlyioPixels(32), editor.Height - editor.NumericUpDown.Height - FlyioPixels(3)));
                };
            }
            labelSelectedScript.TextChanged += (sender, args) => LayoutFlyioPanels();
            labelScriptStatus.TextChanged += (sender, args) => LayoutFlyioPanels();
            tabControlactions.SelectedIndexChanged += (sender, args) => LayoutFlyioPanels();
            tabGauges.SizeChanged += (sender, args) => LayoutFlyioPanels();
            // The legacy resize handler can reveal this gauge behind the six-pack.
            Gvspeed.VisibleChanged += (sender, args) =>
            {
                if (flyioSixPackLayout != null && Gvspeed.Visible) Gvspeed.Visible = false;
            };
            LayoutFlyioPanels();
        }

        private void PolishFlyioControls(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                control.Font = control is Button ? flyioDataButtonFont : flyioDataControlFont;
                control.Margin = new Padding(FlyioPixels(3));
                if (control is Button button)
                {
                    button.AutoSize = false;
                    button.MinimumSize = Size.Empty;
                    button.Padding = new Padding(FlyioPixels(4), FlyioPixels(2), FlyioPixels(4), FlyioPixels(2));
                }
                PolishFlyioControls(control);
            }
        }

        private void LayoutFlyioPanels()
        {
            if (flyioPolishingLayout || flyioDataControlFont == null) return;
            flyioPolishingLayout = true;
            try
            {
                LayoutFlyioTable(tableLayoutPanel1, 64);
                LayoutFlyioTable(tableLayoutPanel2, 48);
                LayoutFlyioTable(tableLayoutPaneltlogs, 48);
                LayoutFlyioScripts();
                LayoutFlyioPayload();
                LayoutFlyioPlaybackSpeeds();
                LayoutFlyioLongLists();
                LayoutFlyioGaugeViewport();
            }
            finally { flyioPolishingLayout = false; }
        }

        private void LayoutFlyioTable(TableLayoutPanel table, int minimumRowHeight)
        {
            table.SuspendLayout();
            if (table == tableLayoutPanel1) LayoutFlyioActionCells();
            int rowHeight = FlyioPixels(minimumRowHeight);
            int cellWidth = Math.Max(FlyioPixels(30), table.ClientSize.Width / table.ColumnCount - FlyioPixels(6));
            using (var graphics = table.CreateGraphics())
            {
                foreach (Control child in table.Controls)
                {
                    child.Dock = DockStyle.Fill;
                    if (child is Button)
                        rowHeight = Math.Max(rowHeight, (int)Math.Ceiling(graphics.MeasureString(
                            child.Text, child.Font, Math.Max(1, cellWidth - FlyioPixels(8))).Height) + FlyioPixels(14));
                }
            }
            if (table == tableLayoutPaneltlogs)
                foreach (Button button in table.Controls.OfType<Button>())
                {
                    button.Dock = DockStyle.Top;
                    button.Height = rowHeight - button.Margin.Vertical;
                }
            table.RowStyles.Clear();
            for (int row = 0; row < table.RowCount; row++)
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, rowHeight));
            table.Height = rowHeight * table.RowCount;
            table.ResumeLayout(true);
            if (table.Parent is TabPage page)
                page.AutoScrollMinSize = new Size(0, table.Height + page.Padding.Vertical);
        }

        private void LayoutFlyioActionCells()
        {
            bool compact = tableLayoutPanel1.Width < FlyioPixels(400);
            tableLayoutPanel1.ColumnCount = compact ? 2 : 5;
            tableLayoutPanel1.ColumnStyles.Clear();
            for (int index = 0; index < tableLayoutPanel1.ColumnCount; index++)
                tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / tableLayoutPanel1.ColumnCount));
            // Change only cell positions in the original table. Selector/action
            // pairs stay together; the same controls retain all their handlers.
            int nextRow = 4;
            foreach (var cell in flyioActionCells.OrderBy(item => item.Value.Row).ThenBy(item => item.Value.Column))
            {
                tableLayoutPanel1.SetColumnSpan(cell.Key, 1);
                if (!compact || cell.Value.Column < 2)
                    tableLayoutPanel1.SetCellPosition(cell.Key, cell.Value);
                else
                    tableLayoutPanel1.SetCellPosition(cell.Key, new TableLayoutPanelCellPosition(0, nextRow++));
                if (compact && cell.Value.Column >= 2) tableLayoutPanel1.SetColumnSpan(cell.Key, 2);
            }
            tableLayoutPanel1.RowCount = compact ? nextRow : 5;
        }

        private void LayoutFlyioScripts()
        {
            // Keep the existing controls and handlers; replace only their bounds.
            int gap = FlyioPixels(6);
            int width = Math.Max(1, tabScripts.ClientSize.Width - tabScripts.Padding.Horizontal - SystemInformation.VerticalScrollBarWidth);
            int x = tabScripts.Padding.Left + tabScripts.AutoScrollPosition.X;
            int y = tabScripts.Padding.Top + tabScripts.AutoScrollPosition.Y;
            foreach (Label label in new[] { labelScriptStatus, labelSelectedScript })
            {
                label.MaximumSize = new Size(width, 0);
                label.AutoSize = true;
                label.Location = new Point(x, y);
                y = label.Bottom + gap;
            }
            checkBoxRedirectOutput.AutoSize = true;
            checkBoxRedirectOutput.Location = new Point(x, y);
            y = checkBoxRedirectOutput.Bottom + gap;
            var buttons = new[] { BUT_select_script, BUT_run_script, BUT_edit_selected, BUT_abort_script };
            int buttonWidth = (width - gap) / 2;
            int height = FlyioPixels(40);
            for (int index = 0; index < buttons.Length; index++)
                buttons[index].SetBounds(x + index % 2 * (buttonWidth + gap), y + index / 2 * (height + gap), buttonWidth, height);
            tabScripts.AutoScrollMinSize = new Size(0, y - tabScripts.AutoScrollPosition.Y + 2 * (height + gap));
        }

        private void LayoutFlyioPayload()
        {
            int gap = FlyioPixels(6);
            int width = Math.Max(1, tabPayload.ClientSize.Width - tabPayload.Padding.Horizontal - SystemInformation.VerticalScrollBarWidth);
            int column = (width - gap) / 2;
            int x = tabPayload.Padding.Left + tabPayload.AutoScrollPosition.X;
            int y = tabPayload.Padding.Top + tabPayload.AutoScrollPosition.Y;
            int row = FlyioPixels(82);
            groupBoxPitch.SetBounds(x, y, column, row * 2 + gap);
            groupBoxYaw.SetBounds(x + column + gap, y, column, row);
            groupBoxRoll.SetBounds(x + column + gap, y + row + gap, column, row);
            foreach (var group in new[] { groupBoxPitch, groupBoxYaw, groupBoxRoll })
            {
                var text = group.Controls.OfType<TextBox>().Single();
                var track = group.Controls.OfType<TrackBar>().Single();
                text.SetBounds(gap, FlyioPixels(20), Math.Max(FlyioPixels(40), group.Width - 2 * gap), text.PreferredHeight);
                track.SetBounds(gap, text.Bottom + gap, Math.Max(1, group.Width - 2 * gap), Math.Max(1, group.Height - text.Bottom - 2 * gap));
            }
            int buttonY = y + 2 * (row + gap);
            BUT_resetGimbalPos.SetBounds(x, buttonY, column, FlyioPixels(36));
            BUT_GimbalVideo.SetBounds(x + column + gap, buttonY, column, FlyioPixels(36));
            tabPayload.AutoScrollMinSize = new Size(0, buttonY - tabPayload.AutoScrollPosition.Y + FlyioPixels(42));
        }

        private void LayoutFlyioPlaybackSpeeds()
        {
            // Fit the seven existing speed buttons in their existing panel.
            int gap = FlyioPixels(3);
            label2.SetBounds(gap, 0, Math.Max(1, panel2.Width - gap * 2), FlyioPixels(18));
            var buttons = new[] { BUT_speed1_10, BUT_speed1_4, BUT_speed1_2, BUT_speed1, BUT_speed2, BUT_speed5, BUT_speed10 };
            int columns = Math.Max(1, panel2.Width / FlyioPixels(32));
            int width = Math.Max(1, panel2.Width / columns - gap);
            for (int index = 0; index < buttons.Length; index++)
                buttons[index].SetBounds(index % columns * (width + gap), label2.Bottom + index / columns * FlyioPixels(28), width, FlyioPixels(25));
            int requiredHeight = label2.Bottom + ((buttons.Length + columns - 1) / columns) * FlyioPixels(28) + gap * 2;
            tableLayoutPaneltlogs.RowStyles[2].Height = Math.Max(tableLayoutPaneltlogs.RowStyles[2].Height, requiredHeight);
            tableLayoutPaneltlogs.Height = (int)tableLayoutPaneltlogs.RowStyles.Cast<RowStyle>().Sum(row => row.Height);
            tabTLogs.AutoScrollMinSize = new Size(0, tableLayoutPaneltlogs.Height + tabTLogs.Padding.Vertical);
        }

        private void LayoutFlyioLongLists()
        {
            int gap = FlyioPixels(4);
            int height = FlyioPixels(32);
            foreach (FlowLayoutPanel list in new[] { flowLayoutPanelServos, flowLayoutPanel1 })
            {
                list.SuspendLayout();
                foreach (Control row in list.Controls)
                {
                    // Retain each composite control, its values and event wiring.
                    // Aux's hidden legacy fields must remain hidden and untouched.
                    var children = flyioLongListControls[row];
                    int x = 0;
                    int y = 0;
                    int available = Math.Max(FlyioPixels(100), list.ClientSize.Width - gap * 2);
                    foreach (Control child in children)
                    {
                        int width = child is ComboBox ? FlyioPixels(140) :
                            child is Label ? FlyioPixels(48) : FlyioPixels(44);
                        child.AutoSize = false;
                        child.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                        if (x > 0 && x + width > available) { x = 0; y += height + gap; }
                        child.SetBounds(x, y, Math.Min(width, available), height);
                        if (child is Label label) label.TextAlign = ContentAlignment.MiddleLeft;
                        x += width + gap;
                    }
                    row.Size = new Size(available, y + height);
                    row.Margin = new Padding(gap, gap, 0, 0);
                }
                list.ResumeLayout(true);
            }
        }

        private void LayoutFlyioGaugeViewport()
        {
            if (flyioSixPackLayout == null) return;
            int columns = Math.Max(1, Math.Min(3, tabGauges.ClientSize.Width / FlyioPixels(140)));
            int rows = (6 + columns - 1) / columns;
            var instruments = new Control[] { flyioAirspeed, flyioAttitude, flyioAltimeter,
                flyioTurn, flyioHeading, flyioVerticalSpeed };
            flyioSixPackLayout.SuspendLayout();
            flyioSixPackLayout.Dock = DockStyle.Top;
            flyioSixPackLayout.ColumnCount = columns;
            flyioSixPackLayout.RowCount = rows;
            flyioSixPackLayout.ColumnStyles.Clear();
            flyioSixPackLayout.RowStyles.Clear();
            for (int column = 0; column < columns; column++)
                flyioSixPackLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / columns));
            int height = Math.Max(FlyioPixels(170),
                (tabGauges.ClientSize.Height - tabGauges.Padding.Vertical - flyioSixPackLayout.Padding.Vertical) / rows);
            for (int row = 0; row < rows; row++)
                flyioSixPackLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
            for (int index = 0; index < instruments.Length; index++)
                flyioSixPackLayout.SetCellPosition(instruments[index], new TableLayoutPanelCellPosition(index % columns, index / columns));
            flyioSixPackLayout.Height = rows * height + flyioSixPackLayout.Padding.Vertical;
            flyioSixPackLayout.ResumeLayout(true);
            tabGauges.AutoScrollMinSize = new Size(0, flyioSixPackLayout.Height + tabGauges.Padding.Vertical);
        }

        private void InitializeFlyioHealthLayout()
        {
            var layout = flyioHealthTab.Controls.OfType<TableLayoutPanel>().Single();
            layout.ColumnStyles.Clear();
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            // AutoSize determines height; the viewport determines wrapping width.
            // Reserve scrollbar space so adding a wrapped line cannot oscillate layout.
            layout.SizeChanged += (sender, args) =>
            {
                int width = Math.Max(1, layout.ClientSize.Width - layout.Padding.Horizontal -
                    SystemInformation.VerticalScrollBarWidth);
                foreach (Control child in layout.Controls)
                {
                    var maximum = new Size(Math.Max(1, width - child.Margin.Horizontal), 0);
                    if (child.MaximumSize != maximum)
                        child.MaximumSize = maximum;
                }
            };
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

            var normalFont = CreateFlyioFont(9F, FontStyle.Bold);
            var compactFont = CreateFlyioFont(8.5F, FontStyle.Bold);
            Disposed += (sender, args) => { normalFont.Dispose(); compactFont.Dispose(); };
            QuickView[] cards = { quickView1, quickView2, quickView3, quickView4, quickView5, quickView6 };
            foreach (QuickView card in cards)
            {
                card.Dock = DockStyle.Fill;
                card.Margin = new Padding(FlyioPixels(4));
                card.BackColor = FlyioDataSurface;
                card.ForeColor = FlyioDataSecondaryText;
                card.Font = normalFont;
                card.SizeChanged += (sender, args) =>
                {
                    card.Font = card.ClientSize.Width < FlyioPixels(150) ? compactFont : normalFont;
                    card.Invalidate();
                };
                card.numberColor = FlyioDataText;
                card.numberColorBackup = FlyioDataText;
                card.ModernCardStyle = true;
                card.MaximumValueFontSize = 30F;
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
                int availableWidth = MainH.Width - MainH.SplitterWidth;
                int left = (int)Math.Round(availableWidth * flyioMainSplitRatio);
                float scale = DeviceDpi / 96F;
                int preferredLeftMinimum = (int)Math.Round(
                    (availableWidth >= 940 * scale ? 460 : 360) * scale);
                preferredLeftMinimum = Math.Min(preferredLeftMinimum,
                    Math.Max(1, availableWidth - MainH.Panel2MinSize));
                MainH.Panel1MinSize = preferredLeftMinimum;
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
            int preferredTabsMinimum = Math.Max(SubMainLeft.Panel2MinSize, (flyioDataNavigation?.Height ?? 0) + FlyioPixels(160));
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
                Height = FlyioPixels(48),
                BackColor = FlyioDataBackground,
                Padding = new Padding(FlyioPixels(8), FlyioPixels(5), FlyioPixels(8), FlyioPixels(5)),
                Margin = Padding.Empty,
                ColumnCount = 8,
                RowCount = 1,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            foreach (float width in new[] { 14F, 11F, 14F, 9F, 12F, 12F, 14F, 14F })
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
            var normalFont = CreateFlyioFont(9.25F, FontStyle.Bold);
            var compactFont = CreateFlyioFont(8F, FontStyle.Bold);
            Disposed += (sender, args) => { normalFont.Dispose(); compactFont.Dispose(); };
            EventHandler sizeStatusBar = (sender, args) =>
            {
                bool compact = flyioDataStatusBar.ClientSize.Width < FlyioPixels(1000);
                foreach (Control status in statuses)
                {
                    status.Font = compact ? compactFont : normalFont;
                    status.Margin = compact
                        ? new Padding(FlyioPixels(3), 0, FlyioPixels(3), 0)
                        : new Padding(FlyioPixels(8), 0, FlyioPixels(6), 0);
                }
                flyioDataStatusBar.Padding = new Padding(FlyioPixels(8), FlyioPixels(5), FlyioPixels(8), FlyioPixels(5));
                flyioDataStatusBar.Height = Math.Max(FlyioPixels(48),
                    (compact ? compactFont : normalFont).Height * 2 + flyioDataStatusBar.Padding.Vertical + FlyioPixels(4));
            };
            flyioDataStatusBar.SizeChanged += sizeStatusBar;
            flyioDataStatusBar.DpiChangedAfterParent += sizeStatusBar;
            sizeStatusBar(null, EventArgs.Empty);
            Controls.Add(flyioDataStatusBar);
            flyioDataStatusBar.BringToFront();
            // Docking runs in reverse z-order: reserve the bottom bar before Fill.
            MainH.BringToFront();
        }

        private void DrawFlyioStatusSeparators(object sender, PaintEventArgs e)
        {
            using (var pen = new Pen(FlyioDataBorder))
            {
                e.Graphics.DrawLine(pen, 0, 0, flyioDataStatusBar.Width, 0);
                int[] widths = flyioDataStatusBar.GetColumnWidths();
                for (int column = 1; column < widths.Length; column++)
                {
                    int x = flyioDataStatusBar.DisplayRectangle.Left + widths.Take(column).Sum();
                    e.Graphics.DrawLine(pen, x, FlyioPixels(8), x, flyioDataStatusBar.Height - FlyioPixels(8));
                }
            }
        }

        private Label NewFlyioStatusLabel()
        {
            var label = new Label
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(8, 0, 6, 0),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = FlyioDataBackground,
                ForeColor = FlyioDataText,
                Font = CreateFlyioFont(9.25F, FontStyle.Bold),
                AutoEllipsis = true
            };
            label.TextChanged += (sender, args) => toolTip1.SetToolTip(label, label.Text.Replace("\r\n", ": "));
            return label;
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
