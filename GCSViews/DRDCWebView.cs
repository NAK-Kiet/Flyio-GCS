using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using MissionPlanner.Controls;
using MissionPlanner.Utilities;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace MissionPlanner.GCSViews
{
    [PreventTheming]
    public sealed class DRDCWebView : MyUserControl, IActivate
    {
        private const string DrdcUrl = "https://drdt-dev-gcs-frontend.drsys.asia/en/login";
        private readonly WebView2 browser;
        private readonly Panel statusPanel;
        private readonly Label statusLabel;
        private readonly MyButton retryButton;
        private bool initialized;

        public DRDCWebView()
        {
            BackColor = ThemeManager.PanelBackground;

            browser = new WebView2 { Dock = DockStyle.Fill };
            statusLabel = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 12F),
                ForeColor = ThemeManager.TextPrimary,
                Margin = new Padding(0, 0, 0, 16),
                Text = "Loading DRDC GCS..."
            };
            retryButton = new MyButton
            {
                Text = "Retry",
                Size = new Size(120, 38),
                Visible = false
            };
            retryButton.Click += RetryButton_Click;
            StyleRetryButton();

            var statusContent = new FlowLayoutPanel
            {
                AutoSize = true,
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.TopDown,
                Location = new Point(32, 32),
                WrapContents = false
            };
            statusContent.Controls.Add(statusLabel);
            statusContent.Controls.Add(retryButton);

            statusPanel = new Panel
            {
                BackColor = ThemeManager.PanelBackground,
                Dock = DockStyle.Fill
            };
            statusPanel.Controls.Add(statusContent);

            Controls.Add(browser);
            Controls.Add(statusPanel);
            statusPanel.BringToFront();
        }

        public void Activate()
        {
            if (!initialized)
                InitializeBrowserAsync();
        }

        private async void InitializeBrowserAsync()
        {
            initialized = true;
            ShowStatus("Loading DRDC GCS...", false);

            try
            {
                await browser.EnsureCoreWebView2Async();
                browser.CoreWebView2.Settings.IsScriptEnabled = true;
                browser.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = true;
                browser.CoreWebView2.Settings.IsWebMessageEnabled = true;
                browser.NavigationStarting += Browser_NavigationStarting;
                browser.NavigationCompleted += Browser_NavigationCompleted;
                browser.Source = new Uri(DrdcUrl);
            }
            catch (Exception ex)
            {
                initialized = false;
                ShowStatus("Unable to load DRDC GCS.\n\n" + ex.Message, true);
            }
        }

        private void Browser_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            ShowStatus("Loading DRDC GCS...", false);
        }

        private void Browser_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                statusPanel.Visible = false;
                browser.Focus();
            }
            else
            {
                ShowStatus("Unable to load DRDC GCS.\n\n" + e.WebErrorStatus, true);
            }
        }

        private void RetryButton_Click(object sender, EventArgs e)
        {
            if (browser.CoreWebView2 == null)
            {
                InitializeBrowserAsync();
                return;
            }

            ShowStatus("Loading DRDC GCS...", false);
            browser.CoreWebView2.Navigate(DrdcUrl);
        }

        private void ShowStatus(string message, bool showRetry)
        {
            statusLabel.Text = message;
            retryButton.Visible = showRetry;
            statusPanel.Visible = true;
            statusPanel.BringToFront();
        }

        private void StyleRetryButton()
        {
            retryButton.BGGradTop = ThemeManager.Accent;
            retryButton.BGGradBot = ThemeManager.Accent;
            retryButton.ColorMouseOver = ThemeManager.AccentHover;
            retryButton.ColorMouseDown = ThemeManager.AccentPressed;
            retryButton.Outline = ThemeManager.ControlBorder;
            retryButton.TextColor = ThemeManager.TextOnAccent;
        }
    }
}
