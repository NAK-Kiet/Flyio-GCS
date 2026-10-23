using System.Drawing;
using System.Windows.Forms;

namespace MissionPlanner.GCSViews
{
    // Configuration-only boundary: no service discovery, process launch, or MAVLink access.
    public sealed class GazeboDashboard : MyUserControl
    {
        public GazeboDashboard()
        {
            Name = "GazeboDashboard";
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScroll = true;
            BackColor = Color.FromArgb(22, 28, 38);
            Dock = DockStyle.Fill;
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1,
                Padding = new Padding(24), BackColor = Color.FromArgb(22, 28, 38)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            Controls.Add(layout);
            AddText(layout, "GAZEBO", 18, FontStyle.Bold);
            AddText(layout, "Simulation workspace - Not configured", 11, FontStyle.Bold);
            AddText(layout, "PX4 SITL connection: Not configured", 10);
            AddText(layout, "Gazebo simulation: Not configured", 10);
            AddText(layout, "Vehicle model: X500", 10);
            var model = new ComboBox { Name = "GazeboVehicleModel", DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 };
            model.Items.Add("X500");
            model.SelectedIndex = 0;
            layout.Controls.Add(model);
            AddText(layout, "Simulation world: Not configured", 10);
            var world = new ComboBox { Name = "GazeboWorld", DropDownStyle = ComboBoxStyle.DropDownList, Width = 260, Enabled = false };
            world.Items.Add("Not configured");
            world.SelectedIndex = 0;
            layout.Controls.Add(world);
            AddText(layout, "MAVLink endpoint - Not configured", 10);
            layout.Controls.Add(new TextBox { Name = "GazeboMavlinkEndpoint", Text = "Not configured", ReadOnly = true, Enabled = false, Width = 260 });
            layout.Controls.Add(new Button { Name = "OpenGazeboViewer", Text = "Open Gazebo viewer (Not configured)", AutoSize = true, Enabled = false, Margin = new Padding(0, 16, 0, 8) });
            AddText(layout, "No simulation is started or connected by this page. World, endpoint and viewer configuration will be available when a simulation provider is integrated.", 10);
            layout.SizeChanged += (sender, args) =>
            {
                foreach (Control child in layout.Controls)
                    if (child is Label) child.MaximumSize = new Size(System.Math.Max(1, layout.ClientSize.Width - layout.Padding.Horizontal - child.Margin.Horizontal), 0);
            };
        }

        private static void AddText(TableLayoutPanel layout, string text, float size, FontStyle style = FontStyle.Regular)
        {
            var font = new Font("Segoe UI", size, style);
            var label = new Label { Text = text, AutoSize = true, ForeColor = Color.FromArgb(225, 231, 239), Font = font, Margin = new Padding(0, 6, 0, 10) };
            label.Disposed += (sender, args) => font.Dispose();
            layout.Controls.Add(label);
        }

        internal static Bitmap CreateNavigationIcon()
        {
            var image = new Bitmap(45, 39);
            using (var graphics = Graphics.FromImage(image))
            using (var pen = new Pen(Color.FromArgb(0, 174, 239), 2))
            {
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                graphics.DrawPolygon(pen, new[] { new Point(22, 3), new Point(37, 11), new Point(37, 28), new Point(22, 36), new Point(7, 28), new Point(7, 11) });
                graphics.DrawLines(pen, new[] { new Point(7, 11), new Point(22, 20), new Point(37, 11) });
                graphics.DrawLine(pen, 22, 20, 22, 36);
            }
            return image;
        }
    }
}
