using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Windows.Forms;

namespace PhotonControl.Helper
{
    public class PhotonHandler
    {
        // Methods
        public static void AddPhotonToFirewall(string workingDirectory)
        {
            try
            {
                string path = Path.Combine(workingDirectory, @"..\bin_Tools\firewalltool");
                if (Directory.Exists(path))
                {
                    new Process { StartInfo = { FileName = Path.Combine(path, "ExitGames.FirewallTool.exe"), Arguments = string.Format("title:\"Photon Socket Server [Exit Games GmbH]\" path:\"{0}\" replace:{1}", Path.Combine(workingDirectory, "PhotonSocketServer.exe"), PhotonControlSettings.Default.ReplaceFirewallRules), WorkingDirectory = workingDirectory } }.Start();
                }
            }
            catch (Exception exception)
            {
                Console.Out.WriteLine("{0}: {1}", Program.ResourceManager.GetString("firewallExceptionMsg"), exception);
            }
        }

        public static void ExploreWorkingDirectory(string workingDirectory)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = workingDirectory
            };
            Process.Start(startInfo);
        }

        public static List<ProcessInfo> GetActiveProcesses()
        {
            int num;
            int num2;
            long num3;
            return GetActiveProcesses(out num, out num2, out num3);
        }

        public static List<ProcessInfo> GetActiveProcesses(out int serviceCount, out int appCount, out long memory)
        {
            serviceCount = 0;
            appCount = 0;
            memory = 0L;
            try
            {
                Process[] processesByName = Process.GetProcessesByName("PhotonSocketServer");
                List<ProcessInfo> list = new List<ProcessInfo>();
                foreach (Process process in processesByName)
                {
                    if (!process.HasExited)
                    {
                        ProcessInfo info;
                        try
                        {
                            info = new ProcessInfo(process);
                        }
                        catch (ManagementException exception)
                        {
                            if (exception.ErrorCode != ManagementStatus.NotFound)
                            {
                                throw;
                            }
                            continue;
                        }
                        bool flag = false;
                        if ((info.Command == ProcessInfo.PhotonCommand.RunAsApplication) && info.IsValid)
                        {
                            appCount++;
                            flag = true;
                        }
                        if ((info.Command == ProcessInfo.PhotonCommand.RunAsService) && info.IsValid)
                        {
                            serviceCount++;
                            flag = true;
                        }
                        if (flag)
                        {
                            list.Add(new ProcessInfo(process));
                            memory += process.WorkingSet64;
                        }
                    }
                }
                memory /= 0x100000L;
                return list;
            }
            catch (Exception exception2)
            {
                MessageBox.Show(string.Format("{0}: {1}", Program.ResourceManager.GetString("failedProcessQueryMsg"), exception2), Program.ResourceManager.GetString("photonControlErrorCaption"));
                return new List<ProcessInfo>();
            }
        }

        public static Dictionary<string, ServiceInfo> GetPhotonServices(string photonServicePrefix)
        {
            try
            {
                Dictionary<string, ServiceInfo> dictionary = new Dictionary<string, ServiceInfo>();
                foreach (ServiceController controller in ServiceController.GetServices())
                {
                    if (controller.ServiceName.StartsWith(photonServicePrefix))
                    {
                        ServiceInfo info;
                        try
                        {
                            info = new ServiceInfo(controller);
                        }
                        catch (ManagementException exception)
                        {
                            if (exception.ErrorCode != ManagementStatus.NotFound)
                            {
                                throw;
                            }
                            continue;
                        }
                        dictionary.Add(info.InstanceName, info);
                    }
                }
                return dictionary;
            }
            catch (Exception exception2)
            {
                MessageBox.Show(string.Format("{0}: {1}", Program.ResourceManager.GetString("failedServiceQueryMsg"), exception2), Program.ResourceManager.GetString("photonControlErrorCaption"));
                return new Dictionary<string, ServiceInfo>();
            }
        }

        public static void InstallService(string instanceName, string workingDirectory)
        {
            new Process { StartInfo = { FileName = "PhotonSocketServer.exe", Arguments = "/Install " + instanceName, WorkingDirectory = workingDirectory } }.Start();
        }

        public static bool OpenLogs(string workingDirectory, string logCmdDirectory, out string errorMessage)
        {
            errorMessage = null;
            if (PhotonControlSettings.Default.UseCmdFileToOpenLogs)
            {
                string str = Path.Combine(logCmdDirectory, "baretail-open-photonlogs.cmd");
                new Process { StartInfo = { FileName = str, Arguments = string.Empty, WorkingDirectory = workingDirectory } }.Start();
            }
            else
            {
                try
                {
                    string str3;
                    string str2 = Path.Combine(logCmdDirectory, "baretail.exe");
                    try
                    {
                        Func<FileInfo, bool> predicate = null;
                        DateTime twelveHoursAgo = DateTime.Now - TimeSpan.FromHours(12.0);
                        string[] strArray = new string[] { "log", @"..\log" };
                        List<string> list = new List<string>();
                        string[] strArray3 = strArray;
                        for (int i = 0; i < strArray3.Length; i++)
                        {
                            Func<FileInfo, string> selector = null;
                            string logDir = strArray3[i];
                            DirectoryInfo info = new DirectoryInfo(Path.Combine(workingDirectory, logDir));
                            if (info.Exists)
                            {
                                if (predicate == null)
                                {
                                    predicate = Finfo => info.LastWriteTime > twelveHoursAgo;
                                }
                                if (selector == null)
                                {
                                    selector = Finfo => Path.Combine(logDir, info.Name);
                                }
                                List<string> collection = info.GetFiles("*.log").Where<FileInfo>(predicate).Select<FileInfo, string>(selector).ToList<string>();
                                list.AddRange(collection);
                            }
                        }
                        str3 = string.Join(" ", list.ToArray());
                        if (str3.Length == 0)
                        {
                            return false;
                        }
                    }
                    catch (Exception)
                    {
                        str2 = Path.Combine(logCmdDirectory, "baretail-open-photonlogs.cmd");
                        str3 = string.Empty;
                    }
                    new Process { StartInfo = { FileName = str2, Arguments = str3, WorkingDirectory = workingDirectory } }.Start();
                }
                catch (Exception exception)
                {
                    errorMessage = Program.ResourceManager.GetString("failedToViewLogsMsg") + Environment.NewLine + exception.Message;
                }
            }
            return true;
        }

        public static void RemoveService(string instanceName, string workingDirectory)
        {
            new Process { StartInfo = { FileName = "PhotonSocketServer.exe", Arguments = "/Remove " + instanceName, WorkingDirectory = workingDirectory } }.Start();
        }

        public static bool StartPhotonApplication(string instanceName, string workingDirectory, out string errorMessage)
        {
            errorMessage = null;
            if (Process.GetProcessesByName("PhotonSocketServer").Length == 0)
            {
                Process process = new Process
                {
                    StartInfo = { FileName = "PhotonSocketServer.exe", Arguments = "/debug " + instanceName, WorkingDirectory = workingDirectory }
                };
                try
                {
                    process.Start();
                }
                catch (Exception exception)
                {
                    errorMessage = string.Format("{0}: {1}", Program.ResourceManager.GetString("photonStartExceptionMsg"), exception);
                    return false;
                }
            }
            return true;
        }

        public static bool StopPhotonApplication(string instanceName, string workingDirectory, out string errorMessage)
        {
            errorMessage = null;
            try
            {
                Process.Start(Path.Combine(workingDirectory, "PhotonSocketServer.exe"), "/stop1 " + instanceName);
            }
            catch (Exception exception)
            {
                errorMessage = string.Format("{0}: {1}", Program.ResourceManager.GetString("photonStopExceptionMsg"), exception);
                return false;
            }
            return true;
        }
    }
}
