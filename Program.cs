using System;
using System.Drawing;
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

            Splash = new Splash();
            Splash.Show();

            Application.Run(new MainV2());
        }

        public static async void TraceMe(bool start = true)
        {
        }
    }
}
