using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace MissionPlanner
{
    public partial class Splash : Form
    {
        public Splash()
        {
            InitializeComponent();

            string strVersion = typeof(Splash).GetType().Assembly.GetName().Version.ToString();

            TXT_version.Text = "Version: " + Application.ProductVersion; // +" Build " + strVersion;

            Console.WriteLine(strVersion);

            // The splash JPG already contains the complete Fly.io branding. Do not
            // layer Program.Logo/mpdesktop artwork over it.
            pictureBox1.Image = null;
            pictureBox1.BackgroundImage = null;
            pictureBox1.Visible = false;

            // Select splash image based on current theme brightness
            var bgColor = Utilities.ThemeManager.BGColor;
            if (bgColor.GetBrightness() < 0.5f)
                BackgroundImage = MissionPlanner.Properties.Resources.splashdark;
            else
                BackgroundImage = MissionPlanner.Properties.Resources.splash;

            label1.Visible = false;
            TXT_version.Location = new Point(20, ClientSize.Height - 34);
            TXT_version.Size = new Size(ClientSize.Width - 40, 24);
            TXT_version.TextAlign = ContentAlignment.MiddleCenter;
            TXT_version.ForeColor = Utilities.ThemeManager.TextPrimary;

            Console.WriteLine("Splash .ctor");
        }
    }
}
