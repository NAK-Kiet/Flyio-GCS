using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace MissionPlanner
{
    public static class Program
    {
        public static DateTime starttime = DateTime.Now;
        public static string name { get; internal set; } = "MissionPlanner";
        public static bool WindowsStoreApp = false;
        public static Image Logo = null;
        public static Image Logo2 = null;
        public static Image IconFile = null;
        public static Splash Splash = null;
        public static string[] args = new string[] { };
        public static bool MONO = Type.GetType("Mono.Runtime") != null;

        [STAThread]
        public static void Main(string[] args)
        {
            Program.args = args;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var runningDirectory = Utilities.Settings.GetRunningDirectory();
            var logoPath = Path.Combine(runningDirectory, "logo.png");
            var logo2Path = Path.Combine(runningDirectory, "logo2.png");
            var iconPath = Path.Combine(runningDirectory, "icon.png");

            if (File.Exists(logoPath))
                Logo = new Bitmap(logoPath);
            if (File.Exists(logo2Path))
                Logo2 = new Bitmap(logo2Path);
            IconFile = File.Exists(iconPath) ? new Bitmap(iconPath) : Properties.Resources.mpdesktop.ToBitmap();

            Splash = new Splash();
            Splash.Show();

            MAVLinkInterface.CreateIProgressReporterDialogue += title =>
            {
                var dialog = new Controls.ProgressReporterDialogue
                {
                    StartPosition = FormStartPosition.CenterScreen,
                    Text = title
                };
                Utilities.ThemeManager.ApplyThemeTo(dialog);
                return dialog;
            };

            // Initialize CustomMessageBox for the Windows desktop application
            CustomMessageBox.ShowEvent += (text, caption, buttons, icon, yestext, notext) =>
            {
                return (CustomMessageBox.DialogResult)(int)MsgBox.CustomMessageBox.Show(
                    text,
                    caption,
                    (MessageBoxButtons)(int)buttons,
                    (MessageBoxIcon)(int)icon,
                    yestext,
                    notext);
            };

            Application.Run(new MainV2());
        }

        public static async void TraceMe(bool start = true)
        {
        }
    }
}
