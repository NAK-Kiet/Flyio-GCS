using MissionPlanner.Controls;
using MissionPlanner.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MissionPlanner.GCSViews.ConfigurationView
{
    /// <summary>
    /// Hosts existing configuration controls without coupling their underlying logic.
    /// Only the visible child is active, matching BackstageView's lifecycle behavior.
    /// </summary>
    public abstract class ConfigCombinedTabPage : MyUserControl, IActivate, IDeactivate
    {
        private readonly TabControl tabs = new TabControl();
        private readonly Dictionary<Control, bool> activatedOnce = new Dictionary<Control, bool>();
        private readonly HashSet<Control> activateOnlyOnce = new HashSet<Control>();
        private Control activeChild;
        private bool isActive;

        protected ConfigCombinedTabPage()
        {
            tabs.Dock = DockStyle.Fill;
            tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabs.Padding = new Point(18, 6);
            tabs.SelectedIndexChanged += Tabs_SelectedIndexChanged;
            tabs.DrawItem += Tabs_DrawItem;
            Controls.Add(tabs);
            ThemeManager.ApplyThemeTo(this);
        }

        protected void AddPage(string title, Control child, bool activateOnce = false)
        {
            child.Dock = DockStyle.Fill;

            var page = new TabPage(title) { Padding = new Padding(3) };
            page.Controls.Add(child);
            tabs.TabPages.Add(page);
            activatedOnce[child] = false;

            if (activateOnce)
                activateOnlyOnce.Add(child);
        }

        public void Activate()
        {
            if (isActive)
                return;

            isActive = true;
            ActivateSelectedChild();
            ThemeManager.ApplyThemeTo(this);
            tabs.Invalidate();
        }

        public void Deactivate()
        {
            if (!isActive)
                return;

            DeactivateChild(activeChild);
            activeChild = null;
            isActive = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Deactivate();
                tabs.SelectedIndexChanged -= Tabs_SelectedIndexChanged;
                tabs.DrawItem -= Tabs_DrawItem;
            }

            base.Dispose(disposing);
        }

        private void Tabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!isActive)
                return;

            DeactivateChild(activeChild);
            activeChild = null;
            ActivateSelectedChild();
            tabs.Invalidate();
        }

        private void ActivateSelectedChild()
        {
            if (tabs.SelectedTab == null || tabs.SelectedTab.Controls.Count == 0)
                return;

            var child = tabs.SelectedTab.Controls[0];
            bool wasActivated;
            activatedOnce.TryGetValue(child, out wasActivated);

            if (child is IActivate && (!activateOnlyOnce.Contains(child) || !wasActivated))
            {
                ((IActivate)child).Activate();
                activatedOnce[child] = true;
            }

            activeChild = child;
            ThemeManager.ApplyThemeTo(child);
        }

        private static void DeactivateChild(Control child)
        {
            if (child is IDeactivate)
                ((IDeactivate)child).Deactivate();
        }

        private void Tabs_DrawItem(object sender, DrawItemEventArgs e)
        {
            var selected = e.Index == tabs.SelectedIndex;
            var background = selected ? ThemeManager.Accent : ThemeManager.ControlBGColor;
            var foreground = ThemeManager.TextColor;

            using (var brush = new SolidBrush(background))
                e.Graphics.FillRectangle(brush, e.Bounds);

            TextRenderer.DrawText(e.Graphics, tabs.TabPages[e.Index].Text, tabs.Font, e.Bounds,
                foreground, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPrefix);
        }
    }
}
