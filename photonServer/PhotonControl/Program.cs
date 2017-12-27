using System;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using PhotonControl.Forms;

namespace PhotonControl
{
    internal static class Program
    {
        // Fields
        private static Mutex m;
        public static ResourceManager ResourceManager = new ResourceManager("PhotonControl.Resources.PhotonControlStrings", typeof(Program).Assembly);

        // Methods
        private static bool AppConfigFileExists()
        {
           
            return File.Exists(Assembly.GetEntryAssembly().Location + ".config");
        }

        [STAThread]
        private static void Main()
        {
            bool flag;
            m = new Mutex(true, "Photon Control", out flag);
            if (!flag)
            {
                MessageBox.Show(ResourceManager.GetString("programAlreadyRunningMsg"), ResourceManager.GetString("programAlreadyRunningCaption"));
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                if (AppConfigFileExists())
                {
                    new ControlForm();
                }
                else
                {
                    new LauncherForm().Show();
                }
                Application.Run();
                Console.WriteLine(Assembly.GetEntryAssembly().Location + ".config");
                Console.ReadKey();
            }
        }
    }
}
