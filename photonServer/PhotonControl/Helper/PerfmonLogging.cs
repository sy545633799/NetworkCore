using System.Diagnostics;

namespace PhotonControl.Helper
{
    public class PerfmonLogging
    {
        // Methods
        public static void Create(string workingDirectory)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "logman.exe",
                Arguments = string.Format("create counter \"{0}\" {1}", PhotonControlSettings.Default.LogManSetName, PhotonControlSettings.Default.LogManCreateOptions),
                WorkingDirectory = workingDirectory
            };
            Process.Start(startInfo);
        }

        public static void Delete()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "logman.exe",
                Arguments = string.Format("delete \"{0}\"", PhotonControlSettings.Default.LogManSetName),
                CreateNoWindow = true
            };
            Process.Start(startInfo);
        }

        public static void Start()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "logman.exe",
                Arguments = string.Format("start \"{0}\"", PhotonControlSettings.Default.LogManSetName),
                CreateNoWindow = true
            };
            Process.Start(startInfo);
        }

        public static void Stop()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "logman.exe",
                Arguments = string.Format("stop \"{0}\"", PhotonControlSettings.Default.LogManSetName)
            };
            Process.Start(startInfo);
        }
    }
}
