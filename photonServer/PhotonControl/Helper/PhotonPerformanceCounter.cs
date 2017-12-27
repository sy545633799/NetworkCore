using System.Diagnostics;

namespace PhotonControl.Helper
{
    public class PhotonPerformanceCounter
    {
        // Methods
        public static void Install(string workingDirectory)
        {
            new Process { StartInfo = { FileName = "PhotonSocketServer.exe", Arguments = "/InstallCounters", WorkingDirectory = workingDirectory } }.Start();
        }

        public static void Remove(string workingDirectory)
        {
            new Process { StartInfo = { FileName = "PhotonSocketServer.exe", Arguments = "/RemoveCounters", WorkingDirectory = workingDirectory } }.Start();
        }
    }
}
