using System;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using MissionPlanner.Controls;

namespace MissionPlanner.GCSViews
{
    public partial class FlightData
    {
        private static readonly Color FlyioDataBackground = Color.FromArgb(22, 28, 38);
        private static readonly Color FlyioDataSurface = Color.FromArgb(34, 43, 56);
        private static readonly Color FlyioDataSecondarySurface = Color.FromArgb(39, 48, 61);
        private static readonly Color FlyioDataBorder = Color.FromArgb(53, 64, 78);
        private static readonly Color FlyioDataText = Color.FromArgb(242, 245, 248);
        private static readonly Color FlyioDataSecondaryText = Color.FromArgb(159, 176, 195);
        private static readonly Color FlyioDataAccent = Color.FromArgb(237, 166, 59);
        private static readonly Color FlyioDataHover = Color.FromArgb(245, 188, 98);
        private static readonly Color FlyioDataPressed = Color.FromArgb(187, 124, 36);

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
                // The optional UI font must never prevent FlightData from opening.
            }

            return new Font(fallback, size, style, GraphicsUnit.Point);
        }

        private void ApplyFlyioLightTheme()
        {
            BackColor = FlyioDataBackground;
            ForeColor = FlyioDataText;
            Font = CreateFlyioFont(9.25F);

            MainH.BackColor = FlyioDataBorder;
            MainH.Panel1.BackColor = FlyioDataBackground;
            MainH.Panel2.BackColor = FlyioDataBackground;
            SubMainLeft.BackColor = FlyioDataBorder;
            SubMainLeft.Panel1.BackColor = FlyioDataSurface;
            SubMainLeft.Panel2.BackColor = FlyioDataSurface;
            tableMap.BackColor = FlyioDataSurface;
            SubMainLeft.Panel1.Padding = new Padding(6, 6, 6, 3);
            SubMainLeft.Panel2.Padding = new Padding(6, 3, 6, 6);

            tabControlactions.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControlactions.Font = CreateFlyioFont(9F, FontStyle.Bold);
            tabControlactions.Padding = new Point(14, 7);

            ApplyFlyioLightThemeToChildren(SubMainLeft.Panel2);
            ApplyFlyioLightThemeToChildren(tableMap);

            // The map canvas and HUD retain their native rendering palettes.
            hud1.BackColor = Color.Black;
            gMapControl1.BackColor = Color.Black;

            InitializeFlyioDataLayout();
            InitializeFlyioHudOverlay();
            InitializeFlyioSixPack();
            InitializeFlyioMapToolbar();
        }

        private void ApplyFlyioLightThemeToChildren(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                if (IsFlyioThemeExcluded(control))
                    continue;

                if (control is TabControl tabs)
                {
                    tabs.BackColor = FlyioDataBackground;
                    tabs.ForeColor = FlyioDataText;
                    tabs.Font = CreateFlyioFont(9F, FontStyle.Bold);
                    foreach (TabPage page in tabs.TabPages)
                    {
                        page.UseVisualStyleBackColor = false;
                        page.BackColor = FlyioDataSurface;
                        page.ForeColor = FlyioDataText;
                    }
                }
                else if (control is Button button)
                {
                    StyleFlyioButton(button);
                }
                else if (control is TextBoxBase || control is ComboBox || control is NumericUpDown)
                {
                    control.BackColor = FlyioDataSurface;
                    control.ForeColor = FlyioDataText;
                    control.Font = CreateFlyioFont(9F);
                }
                else if (control is Label)
                {
                    control.ForeColor = FlyioDataText;
                    control.Font = CreateFlyioFont(9F);
                }
                else if (control is GroupBox)
                {
                    control.BackColor = FlyioDataSurface;
                    control.ForeColor = FlyioDataText;
                    control.Font = CreateFlyioFont(10F, FontStyle.Bold);
                }
                else if (control is Panel || control is TableLayoutPanel || control is FlowLayoutPanel)
                {
                    control.BackColor = FlyioDataSurface;
                    control.ForeColor = FlyioDataText;
                }

                if (control is QuickView quickView)
                {
                    quickView.BackColor = FlyioDataSurface;
                    quickView.ForeColor = FlyioDataText;
                    quickView.Font = CreateFlyioFont(9F, FontStyle.Bold);
                }

                ApplyFlyioLightThemeToChildren(control);
            }
        }

        private static bool IsFlyioThemeExcluded(Control control)
        {
            string typeName = control.GetType().Name;
            string name = control.Name ?? string.Empty;

            return typeName.IndexOf("HUD", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   typeName.IndexOf("GMAP", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   typeName.IndexOf("Graph", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   typeName.IndexOf("Gauge", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   typeName.IndexOf("Terminal", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("tlog", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("logbrowse", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("messages", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void StyleFlyioButton(Button button)
        {
            bool semantic = IsSemanticColor(button.BackColor) || IsCriticalAction(button.Text);
            button.Font = CreateFlyioFont(9F, FontStyle.Bold);
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;

            if (!semantic)
            {
                button.UseVisualStyleBackColor = false;
                button.BackColor = FlyioDataSecondarySurface;
                button.ForeColor = FlyioDataText;
                button.FlatAppearance.BorderColor = FlyioDataBorder;
                button.FlatAppearance.MouseOverBackColor = FlyioDataHover;
                button.FlatAppearance.MouseDownBackColor = FlyioDataPressed;
            }
        }

        private static bool IsSemanticColor(Color color)
        {
            if (color.IsEmpty || color == Color.Transparent)
                return false;

            return color.R > color.G * 1.35 ||
                   color.G > color.R * 1.25 ||
                   (color.R > 150 && color.G > 90 && color.B < 100);
        }

        private static bool IsCriticalAction(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            return text.IndexOf("arm", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   text.IndexOf("disarm", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   text.IndexOf("abort", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   text.IndexOf("kill", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void DrawFlyioLightTab(TabControl tabs, DrawItemEventArgs e)
        {
            if (tabs == null || e.Index < 0 || e.Index >= tabs.TabPages.Count)
                return;

            bool selected = e.Index == tabs.SelectedIndex;
            Color background = selected ? FlyioDataSecondarySurface : FlyioDataBackground;
            Color foreground = selected ? FlyioDataText : FlyioDataSecondaryText;

            using (var backgroundBrush = new SolidBrush(background))
            using (var underlineBrush = new SolidBrush(FlyioDataAccent))
            {
                e.Graphics.FillRectangle(backgroundBrush, e.Bounds);
                if (selected)
                    e.Graphics.FillRectangle(underlineBrush, e.Bounds.Left + 8, e.Bounds.Bottom - 3,
                        Math.Max(1, e.Bounds.Width - 16), 3);
            }

            TextRenderer.DrawText(e.Graphics, tabs.TabPages[e.Index].Text, tabs.Font, e.Bounds,
                foreground, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                            TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);
        }
    }
}
